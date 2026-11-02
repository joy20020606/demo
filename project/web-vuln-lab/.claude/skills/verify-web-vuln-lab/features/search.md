# search — SQL Injection

## Sub-features
- 正常搜尋:用關鍵字 `LIKE` 查 `notes` 的標題。
- 漏洞版:查詢字串拼接,可用 `UNION` 把其他資料表(如 `secrets`)的欄位併進結果。
- 安全版(`SECURE=1`):參數化 + `%` 當字面值綁定,撈不到別的表。

## How to get to it (user POV)
瀏覽器開 `/search?q=關鍵字`,頁面列出符合的筆記標題與內容。

## Driving it with drive.py
```powershell
python .claude\skills\verify-web-vuln-lab\drive.py check search --target vuln
python .claude\skills\verify-web-vuln-lab\drive.py check search --target secure
```
送一個試圖把 `secrets` 表欄位併進結果的查詢字串,檢查回應裡有沒有出現只存在於 `secrets` 的值(`api_key` / `sk-lab` / flag)。

**通過標準**
- vuln:回應含 `secrets` 表才有的字串(跨表資料被撈出)。
- secure:回應不含那些字串。

## Gotchas
- 依賴種子資料 `secrets` 表有 `api_key` 與 flag(見 `app/db.py`)。若種子改動導致這些值消失,此關會回報失敗——應更新種子或本檔,而非放寬檢查。
- 欄位數要對齊(`notes` 查的是 title, body 兩欄),對照 `app.py` 的 SELECT 欄位數再調整 payload。
