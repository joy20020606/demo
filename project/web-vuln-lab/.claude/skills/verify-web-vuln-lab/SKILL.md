---
name: verify-web-vuln-lab
description: "驅動 web-vuln-lab 教學靶機(本機 Docker、Flask),對每一關做漏洞版 vs 安全版的對照驗證:漏洞版(:8080)應觸發該弱點行為,安全版(:8081, SECURE=1)應擋下。用於改完 handler 後證明『漏洞還在、修正真的有修到』,或 Phase 6 防禦對照。"
---

# verify-web-vuln-lab

web-vuln-lab 是一套**只綁 127.0.0.1 的本機教學靶機**,同一份程式用 `SECURE` 環境變數切兩種模式:

- **漏洞版** `http://127.0.0.1:8080`(預設)— 每關都應該會出現該弱點的行為
- **安全版** `http://127.0.0.1:8081`(`SECURE=1`)— 同樣的操作應該被擋下

所以這個 skill 的「證明」就是一組對照:**漏洞版觸發、安全版擋下**,兩邊都符合預期才算這一關的修正是對的。只打本機這兩個實例,不碰任何外部網站。

> ⚠️ 這些檢查只能對你自己這套、在本機或隔離環境的靶機跑。不要指向任何別人的系統。

## Launch

需求:Docker Desktop(WSL2 後端)。這台機器的 Docker 是 per-user 安裝,`docker.exe` 不在預設 PATH,**每次跑 docker 前先補 PATH**:

```powershell
$env:PATH = "C:\Users\JOY\AppData\Local\Programs\DockerDesktop\resources\bin;$env:PATH"
```

從乾淨狀態把兩個實例一起開起來(`docker-compose.yml` 已定義 `web`=:8080 與 `web-secure`=:8081):

```powershell
cd C:\JOY\demo_joycc\project\web-vuln-lab
docker compose up --build -d
```

**Ready 判定**:不是「容器 up 就好」,而是 HTTP 真的可用。等到兩個 port 的首頁都回 200:

```powershell
python .claude\skills\verify-web-vuln-lab\drive.py doctor
```

`app/db.py` 每次啟動會重建 `lab.db`(刪掉再建、重灌種子),所以 launch 本身就把資料帶回乾淨可重現的狀態,不需要額外 migration。

## Doctor

一個唯讀檢查,回答「這兩個實例值不值得驅動」:兩個 port 的 `/` 是否都回 200。任何地方看起來怪怪的,先跑這個:

```powershell
python .claude\skills\verify-web-vuln-lab\drive.py doctor
```

退出碼 0 = 兩邊都活著。非 0 代表有實例沒起來,回去看 `docker compose ps` 和 `docker compose logs`。

## Drive

用 `drive.py`(純標準庫,靠 HTTP 操作,等於模擬使用者在瀏覽器送表單/查詢)。穩定握柄是**路由**(`/login`、`/search`、`/upload`、`/profile`、`/api/load`、`/flag`)和回應內容,不靠座標或 tab 順序。

- 單一關、單一版本:
  ```powershell
  python .claude\skills\verify-web-vuln-lab\drive.py check login --target vuln
  python .claude\skills\verify-web-vuln-lab\drive.py check login --target secure
  ```
- 全部關、全部做 vuln+secure 對照:
  ```powershell
  python .claude\skills\verify-web-vuln-lab\drive.py check-all
  ```

feature 名稱:`login` `search` `upload` `profile` `api-load` `flag`。每關在做什麼、使用者視角怎麼走、什麼結果算通過,見 [`features/`](features/README.md)。

每個 check 回傳的 `passed` 意思是**「這個版本的行為符合預期」**:
- `--target vuln` 的 PASS = 弱點行為有出現(例如繞過成功、資料外洩、模板被求值)
- `--target secure` 的 PASS = 被擋下(例如繞過失敗、撈不到、原樣輸出)

改完某個 handler 後,那一關**兩個版本都要 PASS**:漏洞版還是會觸發,安全版確實擋住。只有漏洞版 PASS、安全版 FAIL,代表修正沒修到;反過來則代表漏洞版被你改掉了。

## Evidence

證據預設寫到 `_evidence/`(可用 `--evidence <dir>` 改)。每個 check 存一份 `<feature>.<target>.json`,記錄 `passed`、`detail`(狀態碼與關鍵判斷依據)、時間戳。

證明標準:
- 走**真實使用者路徑**(送表單、送查詢字串、送 multipart 上傳),不呼叫內部函式或測試專用端點。
- 記錄**動作與造成的狀態**,不只是最後畫面:login/flag 會記到底拿沒拿到 session 與 flag。
- payload 一律用**無害的驗證標記**(`{{7*7}}`→49、`probe.verify` 檔、純資料 dict 的 pickle、`UNION ... FROM secrets` 撈既有種子),目的是觀察「行為有沒有發生」,不是真的植入 webshell 或執行系統指令。

## Cleanup

只關掉這個 skill 開的東西,用 compose 專案收掉,不要用 process 名稱去 kill:

```powershell
$env:PATH = "C:\Users\JOY\AppData\Local\Programs\DockerDesktop\resources\bin;$env:PATH"
cd C:\JOY\demo_joycc\project\web-vuln-lab
docker compose down -v
```

`down -v` 會一併清掉實例與靶機資料。**`_evidence/` 不是靶機狀態,不會被清掉**,證據在 teardown 後仍留在原地。`uploads/` 裡若有驗證留下的 `probe.verify`,隨容器銷毀一起消失(它在容器內),host 上不留殘跡。

## Helpers

- [`drive.py`](drive.py) — 上面所有指令的實作。純 Python 標準庫,不需要額外安裝套件。環境變數 `VULN_URL` / `SECURE_URL` 可覆蓋預設 port。

## 維護

app 改了之後要讓對照表跟著更新,見 pstack 的 `/maintain-verification-skill`。
