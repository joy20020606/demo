# api-load — 不安全反序列化

## Sub-features
- 接收一段資料字串,還原成物件並回報。
- 漏洞版:`pickle.loads(base64.b64decode(data))`,會對不可信資料做 pickle 還原(這正是反序列化風險的來源)。
- 安全版(`SECURE=1`):改用 `json.loads`,只還原純資料,不會執行任何程式。

## How to get to it (user POV)
`POST /api/load`,表單欄位 `data` 放要還原的資料字串,頁面回報還原結果。

## Driving it with drive.py
```powershell
python .claude\skills\verify-web-vuln-lab\drive.py check api-load --target vuln
python .claude\skills\verify-web-vuln-lab\drive.py check api-load --target secure
```
送一個**純資料 dict**(`{"verify": "ok"}`)的 base64 pickle,觀察伺服器走哪條路徑:漏洞版會用 pickle 還原(回應含 `loaded:`),安全版會嘗試 JSON 解析這段 pickle 位元組而失敗(回應含 `json` / `error`)。

**通過標準**
- vuln:回應顯示用 pickle 還原成功(含 `loaded:`,不含 `json`)。
- secure:沒有走 pickle(回應是 JSON 路徑或解析錯誤)。

## Gotchas
- 送的是**純資料物件**,只為區分「走 pickle 還是走 JSON」,不建構任何會在還原時產生副作用的 payload。
- 判斷依據是回應字串的路徑標記(`loaded:` vs `json`/`error`),不是實際副作用。
