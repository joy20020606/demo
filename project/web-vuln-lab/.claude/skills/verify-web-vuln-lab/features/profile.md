# profile — SSTI(伺服器端模板注入)

## Sub-features
- 顯示一張「Hello, <name>!」的 profile 卡片。
- 漏洞版:把 `name` 直接拼進模板字串再 `render_template_string`,輸入裡的模板語法會被伺服器求值。
- 安全版(`SECURE=1`):固定模板 + 變數傳參,輸入永遠當純文字。

## How to get to it (user POV)
瀏覽器開 `/profile?name=你的名字`,頁面顯示問候卡片。

## Driving it with drive.py
```powershell
python .claude\skills\verify-web-vuln-lab\drive.py check profile --target vuln
python .claude\skills\verify-web-vuln-lab\drive.py check profile --target secure
```
送 `name={{7*7}}`——一個**無害的算式**,只用來判斷模板有沒有被求值。檢查回應裡出現的是 `49`(被算了)還是原字串 `{{7*7}}`(沒被算)。

**通過標準**
- vuln:回應含 `49` 且不含原字串 `{{7*7}}`(模板被求值)。
- secure:回應原樣含 `{{7*7}}`,沒有 `49`。

## Gotchas
- 刻意用 `{{7*7}}` 這種純算式當探針,不觸碰任何物件存取或系統呼叫——只要證明「輸入會不會被當模板執行」即可。
- 判斷同時看「有 49」與「沒有原字串」,避免名字本身剛好含 49 造成誤判。
