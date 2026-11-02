"""Web 弱點靶機 — Phase 6(漏洞版 / 安全版 雙模式)

以環境變數 SECURE 切換:
  SECURE 未設 → 漏洞版(預設,給攻擊練習與 Cairn 用)
  SECURE=1    → 安全版(正確寫法,攻擊會失敗)

每個 handler 都用 if SECURE 分成【安全版】與【漏洞版】,方便前後對照。

⚠️ 漏洞版僅限本機 / 隔離環境自我測試使用。
"""
import base64
import json
import os
import pickle

from flask import (
    Flask,
    request,
    render_template,
    render_template_string,
    redirect,
    url_for,
    session,
    flash,
    send_from_directory,
)
from werkzeug.utils import secure_filename
from markupsafe import escape

import db

app = Flask(__name__)
app.secret_key = "phase6-dev-key-not-for-production"

SECURE = os.environ.get("SECURE") == "1"

UPLOAD_DIR = os.path.join(os.path.dirname(__file__), "uploads")
os.makedirs(UPLOAD_DIR, exist_ok=True)

# 安全版上傳白名單
ALLOWED_EXT = {".txt", ".png", ".jpg", ".jpeg", ".gif", ".pdf"}


@app.route("/")
def home():
    return render_template(
        "home.html", user=session.get("username"), role=session.get("role"), secure=SECURE
    )


# ──────────────────────────────────────────────────────────────
# 弱點 1:SQLi + 認證繞過  (/login)
# ──────────────────────────────────────────────────────────────
@app.route("/login", methods=["GET", "POST"])
def login():
    if request.method == "POST":
        username = request.form.get("username", "")
        password = request.form.get("password", "")
        conn = db.get_conn()
        try:
            if SECURE:
                # 【安全版】參數化查詢:輸入只當資料,不當指令
                row = conn.execute(
                    "SELECT * FROM users WHERE username=? AND password=?",
                    (username, password),
                ).fetchone()
            else:
                # 【漏洞版】字串拼接 → 可用 admin'-- - 繞過
                query = (
                    "SELECT * FROM users WHERE username = '%s' AND password = '%s'"
                    % (username, password)
                )
                row = conn.execute(query).fetchone()
        except Exception as e:
            conn.close()
            flash(f"SQL 錯誤:{e}")
            return render_template("login.html")
        conn.close()

        if row:
            session["username"] = row["username"]
            session["role"] = row["role"]
            return redirect(url_for("home"))
        flash("帳號或密碼錯誤")

    return render_template("login.html")


@app.route("/logout")
def logout():
    session.clear()
    return redirect(url_for("home"))


# ──────────────────────────────────────────────────────────────
# 弱點 2:SQL Injection  (/search)
# ──────────────────────────────────────────────────────────────
@app.route("/search")
def search():
    q = request.args.get("q", "")
    rows, error = [], None
    if q:
        conn = db.get_conn()
        try:
            if SECURE:
                # 【安全版】參數化,% 當字面值綁定
                rows = conn.execute(
                    "SELECT title, body FROM notes WHERE title LIKE ?", ("%" + q + "%",)
                ).fetchall()
            else:
                # 【漏洞版】字串拼接 → 可 UNION 撈其他表
                rows = conn.execute(
                    "SELECT title, body FROM notes WHERE title LIKE '%" + q + "%'"
                ).fetchall()
        except Exception as e:
            error = str(e)
        conn.close()
    return render_template("search.html", q=q, rows=rows, error=error)


# ──────────────────────────────────────────────────────────────
# 弱點 3:不安全檔案上傳  (/upload)
# ──────────────────────────────────────────────────────────────
@app.route("/upload", methods=["GET", "POST"])
def upload():
    saved = None
    if request.method == "POST":
        f = request.files.get("file")
        if f and f.filename:
            if SECURE:
                # 【安全版】清洗檔名 + 副檔名白名單
                fname = secure_filename(f.filename)
                ext = os.path.splitext(fname)[1].lower()
                if ext in ALLOWED_EXT:
                    f.save(os.path.join(UPLOAD_DIR, fname))
                    saved = fname
                else:
                    flash(f"拒絕:不允許的副檔名 {ext}(只收 {', '.join(sorted(ALLOWED_EXT))})")
            else:
                # 【漏洞版】不檢查、不清洗 → 可上傳 webshell
                dest = os.path.join(UPLOAD_DIR, f.filename)
                os.makedirs(os.path.dirname(dest), exist_ok=True)
                f.save(dest)
                saved = f.filename
    files = os.listdir(UPLOAD_DIR)
    return render_template("upload.html", saved=saved, files=files)


@app.route("/uploads/<path:name>")
def serve_upload(name):
    return send_from_directory(UPLOAD_DIR, name)


# ──────────────────────────────────────────────────────────────
# 弱點 4:SSTI  (/profile)
# ──────────────────────────────────────────────────────────────
@app.route("/profile")
def profile():
    name = request.args.get("name", "guest")
    if SECURE:
        # 【安全版】固定模板 + 變數傳參,輸入絕不被當模板語法執行
        body = render_template_string(
            "<div class='card'><h2>Profile</h2><p>Hello, {{ name }}!</p></div>", name=name
        )
    else:
        # 【漏洞版】把輸入拼進模板字串 → {{7*7}} 會被執行
        tpl = "<div class='card'><h2>Profile</h2><p>Hello, " + name + "!</p></div>"
        body = render_template_string(tpl)
    return render_template("profile.html", body=body, name=name)


# ──────────────────────────────────────────────────────────────
# 弱點 6:反射型 XSS  (/greet)
# ──────────────────────────────────────────────────────────────
@app.route("/greet")
def greet():
    name = request.args.get("name", "guest")
    if SECURE:
        # 【安全版】先 HTML 跳脫 → <b> 變 &lt;b&gt;,瀏覽器當純文字顯示
        # str() 包起來:Markup.__radd__ 會把左側 HTML 也一起跳脫,只保留 name 被跳脫
        html = "<div class='card'><h2>Greet</h2><p>Hi, " + str(escape(name)) + "!</p></div>"
    else:
        # 【漏洞版】name 原樣拼進 HTML → <b> 被瀏覽器當標記執行
        html = "<div class='card'><h2>Greet</h2><p>Hi, " + name + "!</p></div>"
    return render_template("greet.html", body=html, name=name, secure=SECURE)


# ──────────────────────────────────────────────────────────────
# 弱點 5:不安全反序列化  (/api/load)
# ──────────────────────────────────────────────────────────────
@app.route("/api/load", methods=["GET", "POST"])
def api_load():
    result = None
    if request.method == "POST":
        data = request.form.get("data", "")
        try:
            if SECURE:
                # 【安全版】用 JSON,只還原純資料,不會執行任何程式
                obj = json.loads(data)
                result = f"loaded (json): {obj!r}"
            else:
                # 【漏洞版】pickle.loads 不可信資料 → RCE
                obj = pickle.loads(base64.b64decode(data))
                result = f"loaded: {obj!r}"
        except Exception as e:
            result = f"error: {e}"
    return render_template("load.html", result=result)


# ──────────────────────────────────────────────────────────────
# 目標:/flag
# ──────────────────────────────────────────────────────────────
@app.route("/flag")
def flag():
    if session.get("role") == "admin":
        return render_template("flag.html", flag=db.FLAG)
    return "需要 admin 權限才能看到 flag。", 403


if __name__ == "__main__":
    db.init_db()
    app.run(host="0.0.0.0", port=8080, debug=not SECURE)
