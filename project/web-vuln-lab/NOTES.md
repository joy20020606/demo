# Web 弱點靶機 — 五關講解(學習筆記)

> ⚠️ 本專案為**故意有漏洞**的教學靶機,僅限**本機 / 隔離環境**、針對**自己擁有的系統**練習。
> 設計見 [SPEC.md](SPEC.md),啟動與打穿範例見 [README.md](README.md)。

## 專案概觀

- 技術:Python Flask + SQLite,Docker 部署,只綁 `127.0.0.1`
- 雙模式(環境變數 `SECURE` 切換):
  - 漏洞版 `http://127.0.0.1:8080`(攻擊練習)
  - 安全版 `http://127.0.0.1:8081`(正確寫法,攻擊會失敗)
- 程式:每個 handler 都是 `if SECURE: 安全版 / else: 漏洞版` 並排,方便對照 → [app/app.py](app/app.py)

## 核心觀念(一句話)

**五個漏洞的共同病根,都是「把使用者的輸入,當成程式 / 指令來處理」。**
防禦的核心就是——**永遠把「資料」和「指令」分開。**

---

## 五個關卡

### 關卡 1：SQL Injection + 認證繞過 — `/login`
- **出錯行**:[app.py:69](app/app.py:69)(漏洞版 `else` 分支)
- **病根**:把帳密用字串拼進 SQL。
- **打穿**:帳號填 `admin'-- -`、密碼隨便。`'` 提前關閉字串、`-- ` 把密碼檢查註解掉 → 不需密碼就以 admin 登入。
- **正確修法**:參數化查詢 `execute("... WHERE username=? AND password=?", (u, p))`。

### 關卡 2：SQL Injection — `/search`
- **出錯行**:[app.py:112](app/app.py:112)
- **病根**:查詢字串直接拼接。
- **打穿**:`' UNION SELECT name, value FROM secrets-- -` → 用聯集把別張表接進結果,撈出 flag / api_key。
- **延伸觀念**:攻擊者不是亂猜表名,而是查資料庫內建目錄(SQLite 的 `sqlite_master`)先列出結構,再精準偷取(這叫列舉 enumeration)。
- **正確修法**:參數化 `LIKE ?`,輸入永遠當字面值。

### 關卡 3：不安全檔案上傳 — `/upload`
- **出錯行**:[app.py:140](app/app.py:140)
- **病根**:不檢查副檔名、不清洗檔名、存進可公開存取目錄。
- **打穿**:上傳任意 `.php`/`.txt`,存檔且可從 `/uploads/<檔名>` 原樣取回。本台 Flask 不執行 PHP,所以只是存檔;但在 **PHP/ASP 伺服器上,上傳的 `.php` 會被伺服器執行 → webshell → RCE**。
- **正確修法**:白名單副檔名 + `secure_filename()` + 存到非 web 根目錄 + 目錄設不可執行。

### 關卡 4：伺服器端模板注入 SSTI — `/profile`
- **出錯行**:[app.py:166](app/app.py:166)
- **病根**:把輸入拼進模板字串再 `render_template_string()` 執行。
- **打穿**:`?name={{7*7}}` → 顯示 `49`(證明輸入被當程式執行)。
- **為何嚴重**:可從 `{{7*7}}` 沿 Python 物件屬性「爬物件鏈」爬到 `os` 模組 → 在伺服器執行系統指令(RCE)。
- **正確修法**:固定模板 + 變數傳參 `render_template_string("...{{ name }}...", name=name)`,輸入絕不被當模板語法。

### 關卡 5：不安全反序列化 — `/api/load`
- **出錯行**:[app.py:186](app/app.py:186)
- **病根**:對不可信資料呼叫 `pickle.loads()`。
- **打穿**:pickle 在「還原物件」時會執行物件的重建邏輯,攻擊者可構造成執行系統指令 → **直接 RCE**。
- **關鍵**:pickle **沒有安全用法**處理外部資料,唯一解是不要用它碰外部輸入。
- **正確修法**:改用 `json.loads()`,JSON 只還原純資料、不執行任何程式。

---

## 嚴重度總覽

| 關卡 | 最壞後果 | 等級 |
|---|---|---|
| 1 登入繞過 | 冒充 admin | 高 |
| 2 SQLi | 偷光資料庫 | 高 |
| 3 上傳 | webshell → **RCE** | 🔴 極高 |
| 4 SSTI | 爬物件鏈 → **RCE** | 🔴 極高 |
| 5 反序列化 | 直接 **RCE** | 🔴 極高 |

> **RCE(Remote Code Execution,遠端程式碼執行)**= 攻擊者能在你伺服器上執行任意指令,等於遠端拿到一台主機。關卡 3/4/5 能走到 RCE,所以最該優先修。

---

## 啟動 / 重置

```bash
docker compose up --build -d     # 8080 漏洞版 + 8081 安全版
docker compose restart           # 重置(DB 每次啟動重建)
docker compose down -v           # 整組銷毀
```

> 本機 Docker 為 per-user 安裝,`docker` 可能不在 PATH,跑前先補:
> `$env:PATH = "C:\Users\JOY\AppData\Local\Programs\DockerDesktop\resources\bin;$env:PATH"`

<!-- skip-deploy 測試:2026-11-02 -->
