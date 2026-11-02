# Web 弱點靶機實驗室 — 規格書

> ⚠️ **僅限本機 / 隔離環境自我測試使用。**
> 本專案為「故意埋入漏洞」的教學靶機,只能針對你自己擁有的這套系統練習。
> 絕不可對外開放,也絕不可用來掃描或攻擊任何他人的網站(在台灣可能觸犯刑法妨害電腦使用罪)。

---

## 1. 專案目標

在本機 Docker 隔離環境中,建立一個包含五大常見 Web 弱點的靶機,
用來**親手理解漏洞原理**,並依序用「手動 → 單點工具 → 自主 agent」三階段練習攻防。

學習目標弱點:
1. SQL Injection(SQL 注入)
2. 不安全檔案上傳(Insecure File Upload)
3. 不安全反序列化(Insecure Deserialization)
4. 認證繞過(Authentication Bypass)
5. 伺服器端模板注入(SSTI)

---

## 2. 技術架構

| 項目 | 選擇 | 理由 |
|---|---|---|
| 後端 | Python 3 + Flask | 輕量,五種漏洞都好示範,SSTI 用 Jinja2 直覺 |
| 資料庫 | SQLite | 零設定,方便示範 SQLi |
| 部署 | Docker + Docker Compose | 隔離、可一鍵銷毀重建 |
| 網路綁定 | `127.0.0.1:8080` | 只有本機連得到,不對外 |
| 預設帳密 | `admin` / `password123`(寫死) | 當作「正常登入」基準線 |

---

## 3. 靶機頁面設計(一頁一弱點)

| 路由 | 弱點類型 | 刻意埋的問題 | 通關目標 |
|---|---|---|---|
| `GET/POST /login` | 認證繞過 + SQLi | 字串拼接 SQL,帳密寫死但留 SQLi 繞過路徑 | 不知道密碼也能登入 |
| `GET /search` | SQL Injection | 查詢參數直接進 SQL | 撈出其他使用者資料 |
| `POST /upload` | 不安全檔案上傳 | 不檢查副檔名/內容,可傳 webshell | 上傳並執行程式 |
| `GET /profile` | SSTI | 使用者輸入進 `render_template_string()` | 執行模板運算式 |
| `POST /api/load` | 不安全反序列化 | `pickle.loads()` 吃使用者資料 | 觸發反序列化行為 |
| `GET /flag` | 綜合目標 | 需打穿其中一關後才拿得到的旗標 | 驗證攻擊是否成功 |

---

## 4. 安全護欄(不可省略)

- Docker port 綁 `127.0.0.1`,**不可**綁 `0.0.0.0`
- 容器內執行,與 Windows 主機隔離
- 根目錄放醒目 `README`,標明「僅限本機測試」
- 每次練完:`docker compose down -v` 整組銷毀
- 之後若用 Cairn,其控制台一樣只綁本機,不對外

---

## 5. 開發與練習階段(Phase)

### Phase 0 — 環境就緒 ✅
- Docker Desktop 安裝完成、WSL 2 正常
- 驗證:`docker run hello-world` 成功

### Phase 1 — 建靶機骨架
- Flask app 基本結構 + SQLite 初始化(含種子資料)
- 登入頁(先做成「正常可登入」)
- Docker Compose,綁 `127.0.0.1:8080`
- 驗證:瀏覽器開 `http://127.0.0.1:8080` 能登入

### Phase 2 — 埋入五大弱點
- 依第 3 節,逐頁加入漏洞與 `/flag` 旗標
- 每頁加註解說明「這裡為什麼有漏洞、正確寫法是什麼」
- 驗證:五個頁面都能正常顯示

### Phase 3 — 手動攻擊(理解原理)
- 用瀏覽器 / `curl` 對每頁手打對應 payload
- 目標:親手拿到 `/flag`,搞懂每個漏洞為什麼成立
- 這是最重要的一步——先懂原理再上工具

### Phase 4 — 單點工具
- `sqlmap` 對 `/search`、`/login` 自動化測 SQLi
- (選)Burp Suite 社群版手動攔截/改包
- 目標:看工具怎麼把手動步驟自動化

### Phase 5 — 自主 agent(Cairn)
- 在同一隔離環境跑 Cairn,控制台綁本機
- 讓它自己列舉、嘗試、串連漏洞
- 目標:觀察自主 agent 的思路與人類手動的差異

### Phase 6 — 防禦對照(收尾)
- 針對每個漏洞寫出「正確的修法」
- 對照修補前後,理解防禦要點(參數化查詢、白名單上傳、避免 pickle 等)

---

## 6. 目錄結構(預計)

```
demo_joycc/
├── SPEC.md              # 本規格書
├── README.md           # 安全警語 + 啟動說明
├── docker-compose.yml
├── Dockerfile
├── app/
│   ├── app.py          # Flask 主程式
│   ├── db.py           # SQLite 初始化與種子資料
│   ├── templates/      # 頁面樣板
│   └── uploads/        # 上傳目錄(測試用)
└── requirements.txt
```
