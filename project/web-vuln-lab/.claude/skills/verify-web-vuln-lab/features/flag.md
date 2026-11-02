# flag — 綜合目標

## Sub-features
- `GET /flag` 只有 `role == "admin"` 的 session 看得到旗標,否則回 403。
- 這關把前面的攻擊串起來:先拿到 admin session,才拿得到 flag。

## How to get to it (user POV)
登入成 admin 後開 `/flag`,看到 `FLAG{...}`;沒有 admin 權限則看到 403。

## Driving it with drive.py
```powershell
python .claude\skills\verify-web-vuln-lab\drive.py check flag --target vuln
python .claude\skills\verify-web-vuln-lab\drive.py check flag --target secure
```
先用 login 那關的認證繞過拿 session,再帶著 cookie 打 `/flag`,檢查是否回 200 且含旗標。

**通過標準**
- vuln:認證繞過拿到 admin session → `/flag` 回 200 且含 `FLAG{web_vuln_lab_pwned}`。
- secure:繞過失敗 → 沒有 admin session → `/flag` 回 403,拿不到旗標。

## Gotchas
- 此關依賴 login 關的繞過成立。若 login 關改動,這關要跟著看。
- flag 字串定義在 `app/db.py` 的 `FLAG`;若改了旗標值,同步更新 `drive.py` 的 `FLAG` 常數。
