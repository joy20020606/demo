# Web 弱點靶機實驗室(Vuln Lab)

> ## ⚠️ 安全警語 — 務必閱讀
> - 本專案為**故意埋入漏洞**的教學靶機。
> - **只能針對你自己擁有的這套系統**,在**本機 / 隔離環境**練習。
> - **絕不可對外開放**,也**絕不可**用來掃描或攻擊任何他人的網站。
> - 在台灣,未經授權存取他人電腦系統可能觸犯**刑法妨害電腦使用罪(§358–360)**。

完整設計見 [SPEC.md](SPEC.md)。

---

## 啟動方式

需求:Docker Desktop(WSL 2 後端)。

```bash
docker compose up --build
```

啟動後開瀏覽器:

```
http://127.0.0.1:8080
```

測試帳密:`admin` / `password123`

## 停止與銷毀

```bash
docker compose down -v
```

---

## 目前進度

- **Phase 1 ✅** 骨架:登入頁可正常運作
- **Phase 2 ✅** 五大弱點已埋入,全部驗證可打穿,目標 `/flag`

## 五大弱點與打穿方式(自我練習用)

| 頁面 | 弱點 | 打穿範例 |
|---|---|---|
| `/login` | SQLi + 認證繞過 | 帳號填 `admin'-- -`,密碼隨便 → 以 admin 登入 |
| `/search` | SQL Injection | `?q=' UNION SELECT name, value FROM secrets-- -` → 撈出 flag |
| `/upload` | 不安全上傳 | 上傳任意副檔名(如 `.php`),存檔且可取回 |
| `/profile` | SSTI | `?name={{7*7}}` → 顯示 49 |
| `/api/load` | 不安全反序列化 | POST base64 的 pickle → 伺服器 `pickle.loads()` |

> 🎯 目標旗標:`FLAG{web_vuln_lab_pwned}`(存在 secrets table,或登入成 admin 後看 `/flag`)

## Phase 6:防禦對照

同一份程式用環境變數 `SECURE` 切換:
- `web`(8080)= 漏洞版(預設)
- `web-secure`(8081)= 安全版(`SECURE=1`)

相同五招打兩邊,安全版全部擋下:

| 弱點 | 病根 | 正確修法 |
|---|---|---|
| SQLi / 認證繞過 | 字串拼接 SQL | **參數化查詢** `execute(sql, (a, b))` |
| SQL Injection | 同上 | 同上(`LIKE ?` 綁定) |
| 不安全上傳 | 不檢查副檔名/檔名 | **白名單副檔名 + `secure_filename()`** |
| SSTI | 輸入拼進模板 | **固定模板 + 變數傳參**,不把輸入當模板 |
| 不安全反序列化 | `pickle.loads()` 吃外部資料 | **改用 `json.loads()`**,絕不 pickle 外部輸入 |

> 一句話病根:**把「使用者輸入」當成「程式/指令」處理**。防禦核心就是永遠把兩者分開。

啟動兩版對照:
```bash
docker compose up --build -d   # 8080 漏洞版 + 8081 安全版
```
