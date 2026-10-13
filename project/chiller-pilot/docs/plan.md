# chiller-pilot 實作計畫

> 依據:[spec.md](spec.md)
> 狀態:待審,尚未開始實作

## 原則

- 每個階段結束時都有一個**可以親手操作並看到結果**的驗證步驟。
- 每個階段結束時補上對應的測試與一小段文件,不留到最後。
- 需求編號(R1 到 R9)對應 spec 第 4 節。
- 程式放在 `project/chiller-pilot/`,與其他 demo 同在 `demo` 這個 repo;CI 設定檔放在 repo 根目錄的 `.github/workflows/`,以路徑過濾只在本專案變動時執行。

## 目錄結構

```
project/chiller-pilot/
├── README.md
├── docker-compose.yml          雲端容器
├── docker-compose.site.yml     案場端容器(模擬器與 gateway)
├── .env.example
├── sites/                      每個案場一份點位表
│   ├── site_a.yaml
│   └── site_b.yaml
├── simulator/                  模擬設備
├── gateway/                    案場端程式
├── cloud/
│   ├── app/
│   │   ├── api/                FastAPI
│   │   ├── worker/             MQTT 訂閱與定時最佳化
│   │   ├── domain/             護欄、最佳化、節能計算
│   │   └── db/
│   └── tests/
├── dashboard/                  靜態頁面
├── deploy/                     Caddy、Mosquitto 設定與部署腳本
└── docs/
    ├── spec.md
    ├── plan.md
    ├── data-dictionary.md
    ├── runbook.md
    ├── decisions/              決策紀錄
    └── incidents/              事故報告
```

## 階段

| # | 階段 | 完成後你可以親手驗證的事 | 需求 |
|---|---|---|---|
| 0 | 專案骨架與模擬器 | 啟動兩個模擬設備,用一支小腳本讀出原始數字;改變設定值後耗電跟著變 | |
| 1 | Gateway 上行 | 訂閱 broker,看到兩個案場各自送出已換算好的 JSON | R1 |
| 2 | 資料入庫、查詢 API、案場隔離 | 用 API 查到資料;拿案場 A 的金鑰查不到案場 B | R1、R9 |
| 3 | 資料品質與告警 | 打開「倍率錯誤」故障開關,5 分鐘內出現告警;停掉 gateway,2 分鐘內顯示離線 | R2、R5 |
| 4 | 斷線暫存與補傳 | 切斷 gateway 網路一段時間再恢復,資料沒有缺口也沒有重複 | R3 |
| 5 | 指令下行、護欄、紀錄 | 手動下一筆指令,設備的值真的改變;下一筆超出範圍的指令,被護欄修正並留下紀錄 | R8 |
| 6 | AI 開關與斷線退回 | 關閉 AI,設定值回到預設;停掉雲端,10 分鐘後 gateway 自行退回 | R4、R7 |
| 7 | 最佳化與節能計算 | 開啟 AI 後總耗電下降;查詢每月節能,關閉期間未被計入 | R6 |
| 8 | 即時頁面 | 瀏覽器上的數字每 10 秒自動更新,並顯示告警與最近的指令 | |
| 9 | 部署到 AWS | 用公開的 HTTPS 網址開啟頁面;推送一次程式變更後自動上線 | |
| 10 | 文件收尾與事故報告 | 依 runbook 從零重建一次環境;閱讀一份完整的事故報告 | |

## 各階段內容

### 階段 0:專案骨架與模擬器

- 建立目錄結構、`.gitignore`、`.env.example`、`.gitattributes`(`*.sh` 使用 LF)。
- 模擬器:`pymodbus` server,依 spec 第 12 節的簡化公式每秒更新暫存器。
- 兩個案場使用不同的位址配置、倍率與係數。
- 故障開關:感測器卡住、倍率錯誤。
- 測試:公式的單元測試(出水溫調高時主機耗電下降;風扇轉速與耗電的三次方關係)。

### 階段 1:Gateway 上行

- 點位表的讀取與驗證(用 Pydantic,設定檔寫錯時啟動即失敗並指出哪一行)。
- `PointReader` 介面與 `ModbusReader` 實作。
- 輪詢、套用倍率、發布到 `site/{id}/telemetry`。
- Mosquitto 容器。
- 測試:點位表驗證、倍率換算。
- 決策紀錄:為什麼差異放設定檔而不是子類別。

### 階段 2:資料入庫、查詢 API、案場隔離

- TimescaleDB 容器、資料表建立(Alembic)。
- Worker:訂閱 `site/+/telemetry`,寫入 `readings`,重複的略過。
- API:`/health`、`/site`、`/readings`、`/readings/latest`,以 `X-API-Key` 識別案場。
- Mosquitto 帳號與 topic 權限:每個案場只能使用自己的 topic。
- CI:GitHub Actions 執行測試與程式檢查。
- 測試:重複寫入不產生重複資料;跨案場查詢被拒絕。
- 決策紀錄:窄表的選擇;api 與 worker 分開。

### 階段 3:資料品質與告警

- Gateway 依 `valid_range` 標記 `quality`。
- Worker:開啟與結束 `out_of_range` 告警;定時檢查 `last_seen_at`,開啟與結束 `gateway_offline` 告警。
- API:`/alerts`。
- 測試:持續異常只產生一筆告警;恢復後告警結束。

### 階段 4:斷線暫存與補傳

- Gateway 的 SQLite outbox:先寫入、發布成功後才標記。
- 恢復連線後依時間順序補傳,並限制補傳速度。
- 測試:模擬發布失敗後恢復,資料筆數正確且無重複。
- 決策紀錄:SQLite 而非 Redis。

### 階段 5:指令下行、護欄、紀錄

- `commands` 資料表;護欄(上下限、每次最大變化量)為純函式,便於測試。
- Worker 發布指令;gateway 檢查重複與過期、再次檢查上下限、寫入設備、讀回、回報。
- API:`/commands`,以及一支手動下指令的端點(供驗證與示範)。
- 測試:護欄的邊界條件;重複的指令只執行一次;過期的指令被丟棄。
- 決策紀錄:先記錄再送出;護欄做兩次。

### 階段 6:AI 開關與斷線退回

- `ai_mode_periods` 資料表與 `/ai-mode` 端點。
- 雲端定時發送 heartbeat;gateway 逾時後寫回預設值並把紀錄放入 outbox。
- 測試:關閉 AI 後最佳化不執行;逾時後退回預設值;恢復後紀錄補傳。

### 階段 7:最佳化與節能計算

- 用模擬器在 AI 關閉下跑出的資料訓練 baseline 模型,存入 `baseline_models`。
- 最佳化:迴歸模型預測總耗電,對兩個設定值的候選組合逐一計算,取最低者。
- API:`/savings`。
- 測試:關閉時段與異常資料被排除;已知輸入下的節能計算結果正確。
- 決策紀錄:為什麼模型刻意做得簡單。

### 階段 8:即時頁面

- Redis 容器:最新值與通知。
- API 的 WebSocket 端點。
- 一個靜態頁面:即時數值、耗電趨勢圖、告警、最近的指令、AI 開關。
- 不使用前端框架的建置流程,以保持「內部頁面」的簡單程度。

### 階段 9:部署到 AWS

- AWS 帳號安全設定與費用警示。
- EC2(`t3.small`,東京)、固定 IP、防火牆規則(只開放 HTTPS 與加密的 MQTT)。
- ECR;GitHub Actions 以 OIDC 取得臨時權限推送 image;EC2 以執行個體角色拉取。
- Caddy 與 sslip.io 的 HTTPS;Mosquitto 啟用 TLS。
- 自動部署:推送到主分支後更新 EC2 上的容器。
- 資料庫每日備份到 S3。

### 階段 10:文件收尾與事故報告

- README:一句話介紹、線上連結、截圖、架構圖、需求與程式碼對應表、本地啟動、部署、設計取捨。
- `data-dictionary.md`:每張表每個欄位的定義與單位。
- `runbook.md`:從零部署、常見問題排查、gateway 遠端更新的做法(只寫文件)。
- 事故報告:實際觸發一次故障(例如倍率錯誤),記錄時間軸、原因、處理、預防措施。
- 截圖前清理測試資料。

## 與職缺的對應

| 職缺要求 | 對應階段 |
|---|---|
| Python、FastAPI、RESTful API | 2、5、6、7 |
| 資料庫設計與維護 | 2、文件中的資料定義 |
| Docker 開發與部署 | 全部、9 |
| Git、CI/CD、Unit Test | 2 起每個階段、9 |
| 上線後的維運與事故處理 | 3、9、10 |
| 文件習慣 | 每個階段的決策紀錄、10 |
| MQTT、WebSocket、Modbus | 0、1、5、8 |
| 時序資料 | 2 |
| 多案場 | 1、2、3 |
| Linux、TCP/IP | 9 |
| AWS、Redis | 8、9 |

## 你需要自己動手的地方

| 階段 | 事項 |
|---|---|
| 每個階段 | 依我給的指令親手跑一次驗證 |
| 9 | 建立 AWS 帳號、綁定付款方式、在主控台做第一次登入與安全設定 |
| 9 | 把 AWS 帳號 ID 等資訊填入 GitHub 的設定 |
| 10 | 截圖 |

## 風險

| 風險 | 處理方式 |
|---|---|
| `t3.small` 記憶體不足 | 階段 9 先量測實際用量;不足時加 swap 或升一級 |
| 模擬器公式產生不出明顯的節能差異 | 階段 0 就驗證設定值與耗電的關係,不留到階段 7 才發現 |
| 範圍過大做不完 | 階段 0 到 7 完成即為可展示的後端;8 之後各自獨立,可依時間取捨 |
