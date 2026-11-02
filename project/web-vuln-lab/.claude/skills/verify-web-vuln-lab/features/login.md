# login — 認證繞過 + SQLi

## Sub-features
- 正常帳密登入(`admin` / `password123`)取得 session 與 role。
- 漏洞版:登入查詢是字串拼接,輸入可讓 WHERE 條件恆真,不知道真密碼也能登入。
- 安全版(`SECURE=1`):參數化查詢,輸入只當資料,繞過失敗。

## How to get to it (user POV)
瀏覽器開 `/login`,填使用者名稱與密碼送出。成功會轉址回首頁並帶 `session` cookie;失敗留在登入頁並顯示錯誤。

## Driving it with drive.py
```powershell
python .claude\skills\verify-web-vuln-lab\drive.py check login --target vuln
python .claude\skills\verify-web-vuln-lab\drive.py check login --target secure
```
送一個「使用者名稱含讓條件恆真的片段、密碼是任意非真值」的登入表單,觀察是否拿到轉址 + session cookie。

**通過標準**
- vuln:回應是 302 轉址且帶 `session=` cookie(等於不知道密碼也登入了)。
- secure:沒有拿到 session,停在登入頁。

## Gotchas
- 判斷依據是「有沒有拿到 session cookie」,不是畫面文字,避免模板改字就誤判。
- `drive.py` 刻意不跟隨轉址,這樣才看得到登入當下的 302 與 Set-Cookie。
