# web-vuln-lab 功能對照表

這是靶機的維護性驗證來源。每一關對應一個使用者會碰到的頁面/端點,一個檔案一關。
每關都從**使用者視角**回答:這關是什麼、怎麼走到、怎麼用 `drive.py` 驅動、什麼結果算通過。

驗證的核心形狀是**對照**:同一個操作,漏洞版(:8080)應觸發弱點行為,安全版(:8081)應擋下。

| 關 | 路由 | 弱點類型 | 檔案 |
|---|---|---|---|
| login | `POST /login` | 認證繞過 + SQLi | [login.md](login.md) |
| search | `GET /search` | SQL Injection | [search.md](search.md) |
| upload | `POST /upload` | 不安全檔案上傳 | [upload.md](upload.md) |
| profile | `GET /profile` | SSTI | [profile.md](profile.md) |
| greet | `GET /greet` | 反射型 XSS | [greet.md](greet.md) |
| api-load | `POST /api/load` | 不安全反序列化 | [api-load.md](api-load.md) |
| flag | `GET /flag` | 綜合目標 | [flag.md](flag.md) |

全部跑一遍:`python .claude\skills\verify-web-vuln-lab\drive.py check-all`

> 種子資料在 `app/db.py`:users(admin/alice/bob)、notes(3 筆)、secrets(flag + api_key)。
> 若某個 journey 需要的前置資料種子裡沒有,就讓它回報失敗並把原因寫進該關的「Gotchas」,不要為了讓它通過而把檢查放寬。
