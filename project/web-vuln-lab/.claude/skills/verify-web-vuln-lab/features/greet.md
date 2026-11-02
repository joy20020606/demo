# greet — 反射型 XSS(Reflected XSS)

## Sub-features
- 顯示一張「Hi, <name>!」的問候卡片。
- 漏洞版:把 `name` 直接拼進 HTML 字串,輸入裡的標記會被瀏覽器當 HTML 執行。
- 安全版(`SECURE=1`):先用 `escape(name)` 做 HTML 跳脫,輸入永遠當純文字顯示。

## How to get to it (user POV)
瀏覽器開 `/greet?name=你的名字`,頁面顯示問候卡片。

## Driving it with drive.py
```powershell
python .claude\skills\verify-web-vuln-lab\drive.py check greet --target vuln
python .claude\skills\verify-web-vuln-lab\drive.py check greet --target secure
```
送 `name=<b>xss7</b>`——一個**無害標記**,只用來判斷輸入有沒有被跳脫。檢查問候卡片那段出現的是原樣 `<b>xss7</b>`(未跳脫,被當 HTML)還是跳脫後的 `&lt;b&gt;xss7&lt;/b&gt;`(當純文字)。

**通過標準**
- vuln:問候卡片原樣含 `Hi, <b>xss7</b>!`(標記未跳脫,被瀏覽器當 HTML)。
- secure:問候卡片含 `Hi, &lt;b&gt;xss7&lt;/b&gt;!` 且不含原樣 `Hi, <b>xss7</b>!`。

## Gotchas
- 刻意用 `<b>xss7</b>` 這種純標記當探針,不放任何 `<script>` 或真實攻擊載荷——只要證明「輸入會不會被當 HTML 執行」即可。
- 只看問候卡片那段("Hi, ...!"):表單 `value=` 會被 Jinja 自動跳脫、提示文字用 `xss`(無 7),都不會干擾判斷。
- 探針帶 `7` 是為了和頁面上的提示文字 `xss` 區隔,避免誤判。
