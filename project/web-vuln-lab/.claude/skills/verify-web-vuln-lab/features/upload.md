# upload — 不安全檔案上傳

## Sub-features
- 上傳檔案並列出 `uploads/` 內容。
- 漏洞版:不檢查副檔名、不清洗檔名,任何檔案原樣存檔。
- 安全版(`SECURE=1`):`secure_filename` 清洗 + 副檔名白名單(`.txt/.png/.jpg/.jpeg/.gif/.pdf`),非白名單拒收。

## How to get to it (user POV)
瀏覽器開 `/upload`,選檔送出。成功後頁面顯示已存檔名與 `uploads/` 清單。

## Driving it with drive.py
```powershell
python .claude\skills\verify-web-vuln-lab\drive.py check upload --target vuln
python .claude\skills\verify-web-vuln-lab\drive.py check upload --target secure
```
上傳一個**非白名單副檔名**的無害檔案(`probe.verify`,內容只是驗證標記),檢查它有沒有被原樣收下。

**通過標準**
- vuln:回應裡出現 `probe.verify`(非白名單檔名被收下)。
- secure:沒出現,被拒收。

## Gotchas
- 用的是無害的驗證檔,只為觀察「白名單有沒有生效」,不涉及可執行內容。
- 檔案存進容器內的 `uploads/`,`docker compose down -v` 會隨容器清掉,host 不留殘跡。
- 重跑時漏洞版 `uploads/` 會累積 `probe.verify`,不影響判斷(看的是本次回應,不是計數)。
