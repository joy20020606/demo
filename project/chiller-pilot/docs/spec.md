# chiller-pilot 規格書

> 多案場冰水主機遙測與控制後端(demo)
> 狀態:草稿,尚未實作
> 最後更新:2026-10-13

## 1. 背景與目的

空調節能系統的運作方式是:從各案場的冰水主機讀取運轉資料,在雲端計算較省電的設定值,再寫回設備,並向客戶證明省了多少電。

這個 demo 重現這條資料鏈的**後端部分**,重點是多案場、資料可信度、斷線處理與可驗證性。AI 模型本身刻意做得很簡單。

### 前提與假設

- 設備是模擬的:用 `pymodbus` 模擬兩個案場的冰水主機,兩場的點位表不同。
- 流程依據的是暖通空調(HVAC)業界的通用做法,不代表任何特定公司的內部設計。
- 模擬器的耗電公式是簡化過的,節能百分比只用來驗證資料流,不代表真實效益。

## 2. 範圍

### 要做

| 模組 | 內容 |
|---|---|
| Simulator | 模擬 2 個案場的 Modbus TCP 設備 |
| Gateway | 依點位表輪詢設備、上傳資料、接收指令、斷線時自行退回預設值 |
| Worker | 訂閱 MQTT 寫入資料庫、定時執行最佳化並下指令 |
| API | 查詢資料、指令紀錄、告警、每月節能;切換 AI 開關 |
| Dashboard | 一個即時頁面(WebSocket) |
| 部署 | 單台 AWS EC2 + Docker Compose,GitHub Actions 自動部署 |
| 文件 | 本規格、資料定義、決策紀錄、runbook、一份事故報告 |

### 不做,以及原因

| 不做 | 原因 |
|---|---|
| BACnet | 一種協定已足以示範多案場抽象;保留 `PointReader` 介面,新增協定不需改核心 |
| BI 報表工具 | 需求只是一個可信的節能數字,一支 API 就夠 |
| 深度學習或強化學習 | 這個 demo 的重點是後端資料流,不是模型 |
| 拆成多個微服務 | 兩支程式(api、worker)共用一份程式碼就能滿足需求 |
| Kubernetes、ECS | 單台 EC2 足夠,且更便宜 |
| 完整的使用者帳號系統 | 每個案場一把 API 金鑰即可示範隔離 |

## 3. 名詞

| 名詞 | 說明 |
|---|---|
| 點位(point) | 設備上一個可讀或可寫的值,例如冰水出水溫度 |
| 點位表 | 描述每個點位在設備上的位址、倍率、單位、合理範圍的設定檔 |
| 設定值(setpoint) | 要求設備達到的目標值 |
| Gateway | 放在案場內網的小電腦,上面跑我們的程式 |
| Broker | MQTT 的訊息轉運站,本專案使用 Mosquitto |
| Baseline | 「沒有這套系統時原本會用多少電」的估計值 |
| 護欄(guardrail) | 指令送出前的安全檢查 |

## 4. 需求與驗收條件

需求來自對案場工程師與節能報告負責人的訪談。

| 編號 | 需求 | 驗收條件 |
|---|---|---|
| R1 | 新案場只靠新增設定檔上線 | 新增 `site_c.yaml` 並啟動該案場的 gateway 後,不修改任何 `.py` 檔,`site_c` 的資料在 1 分鐘內可由 API 查到 |
| R2 | 自動發現不合理的數值 | 超出點位表所定範圍的資料標記為異常、不提供給最佳化計算,並在 5 分鐘內產生一筆告警;同一點位持續異常時只維持一筆進行中的告警 |
| R3 | 斷線期間的資料不遺失 | 中斷 gateway 對外網路 2 小時後恢復,資料庫中該期間的資料沒有缺口,也沒有重複 |
| R4 | 斷線時設備回到安全狀態 | Gateway 超過 10 分鐘未收到雲端訊息時,自行把設定值寫回點位表中的預設值,並留下紀錄 |
| R5 | 主動得知 gateway 離線 | Gateway 停止回報超過 2 分鐘,該案場狀態顯示為離線並產生告警;恢復後告警自動結束 |
| R6 | 每月節能數字可即時取得 | 指定案場與月份,API 回傳 baseline 用電、實際用電、節省度數與百分比 |
| R7 | 客戶可隨時關閉 AI | 呼叫 API 關閉後 1 分鐘內,設備設定值回到預設值;關閉期間的用電不計入 R6 |
| R8 | 每筆指令可追溯 | 可依案場與時間範圍查詢指令,包含當時的輸入、模型預測、護欄是否修改、設備讀回結果 |
| R9 | 案場之間資料隔離 | 案場識別由憑證決定,不由請求參數決定;用案場 A 的金鑰查詢案場 B 的資料會被拒絕,且有自動化測試證明 |

## 5. 架構

```
[案場內網,每個案場一組]                 [AWS EC2,Docker Compose]

 simulator ←Modbus TCP→ gateway ──MQTT──→ mosquitto ──→ worker ──→ timescaledb
 (模擬設備)              (SQLite outbox)      ↑            │  ↑
                              ↑               │            ↓  │
                              └───指令/MQTT───┴────────── redis
                                                           ↓
                              瀏覽器 ←─HTTPS/WebSocket─ caddy → api
```

| 容器 | 角色 |
|---|---|
| `simulator` | 模擬設備。demo 時與 gateway 一起跑,代表「案場端」 |
| `gateway` | 每個案場一個。輪詢、上傳、收指令、斷線退回 |
| `mosquitto` | MQTT broker |
| `worker` | 長時間執行。訂閱 MQTT、寫入資料庫、定時最佳化 |
| `api` | FastAPI。查詢與操作,WebSocket 推送 |
| `timescaledb` | PostgreSQL 加 TimescaleDB 外掛 |
| `redis` | 最新值暫存、worker 與 api 之間的通知管道 |
| `caddy` | 反向代理與 HTTPS 憑證 |

`api` 與 `worker` 分開的原因:API 重啟或擴充時,資料接收不受影響,也不會重複訂閱。

## 6. 點位表

每個案場一份 YAML,放在 git 內。

```yaml
site_id: site_a
protocol: modbus_tcp
host: simulator-a
port: 502
poll_interval_seconds: 10
points:
  - name: chw_supply_temp
    address: 0
    scale: 0.1
    unit: degC
    valid_range: [4, 15]
  - name: chiller_power
    address: 9
    scale: 1
    unit: kW
    valid_range: [0, 800]
  - name: chw_setpoint
    address: 20
    scale: 0.1
    unit: degC
    writable: true
    default: 7.0
    limits: [6, 10]
    max_step: 0.5
```

- `valid_range` 用於 R2 的範圍檢查。
- `default`、`limits`、`max_step` 只出現在可寫入的點位,用於 R4 的退回與護欄。
- 全系統只認 `name`。同一個 `name` 在不同案場可以對應不同的位址與倍率。

## 7. MQTT 設計

| Topic | 方向 | 內容 |
|---|---|---|
| `site/{site_id}/telemetry` | gateway → 雲端 | 一批讀值 |
| `site/{site_id}/command` | 雲端 → gateway | 一筆指令 |
| `site/{site_id}/ack` | gateway → 雲端 | 指令執行結果 |
| `site/{site_id}/heartbeat` | 雲端 → gateway | 雲端存活訊號,供 R4 判斷 |

- 每個案場一組 broker 帳號,權限限定在 `site/{自己的 site_id}/` 之下。
- Worker 以 `site/+/telemetry` 一次訂閱所有案場。

指令訊息:

```json
{
  "command_id": "0d5e6c1a-…",
  "name": "chw_setpoint",
  "value": 8.0,
  "expires_at": "2026-10-13T10:10:00Z"
}
```

## 8. 資料模型

時間一律以 UTC 儲存,案場時區記在 `sites`。文字欄位使用 `TEXT`。

### sites

| 欄位 | 型別 | 說明 |
|---|---|---|
| `site_id` | TEXT, PK | |
| `name` | TEXT | |
| `timezone` | TEXT | 例如 `Asia/Taipei` |
| `api_key_hash` | TEXT | 只存雜湊值 |
| `last_seen_at` | TIMESTAMPTZ | 最後一次收到資料的時間 |

### readings(TimescaleDB hypertable)

| 欄位 | 型別 | 說明 |
|---|---|---|
| `site_id` | TEXT | PK 之一 |
| `name` | TEXT | PK 之一 |
| `ts` | TIMESTAMPTZ | PK 之一 |
| `value` | DOUBLE PRECISION | |
| `quality` | TEXT | `ok` 或 `out_of_range` |

採窄表:新增點位不需要改表結構。複合主鍵同時保證補傳時不會重複寫入。

### alerts

| 欄位 | 型別 | 說明 |
|---|---|---|
| `alert_id` | UUID, PK | |
| `site_id` | TEXT | |
| `type` | TEXT | `out_of_range`、`gateway_offline`、`command_failed` |
| `point_name` | TEXT, 可空 | |
| `started_at` | TIMESTAMPTZ | |
| `ended_at` | TIMESTAMPTZ, 可空 | 空值代表進行中 |
| `detail` | JSONB | |

### ai_mode_periods

| 欄位 | 型別 | 說明 |
|---|---|---|
| `period_id` | UUID, PK | |
| `site_id` | TEXT | |
| `mode` | TEXT | `on` 或 `off` |
| `started_at` | TIMESTAMPTZ | |
| `ended_at` | TIMESTAMPTZ, 可空 | 空值代表目前的狀態 |
| `changed_by` | TEXT | |

存時段而非事件,計算節能時可直接排除 `off` 的時段。

### commands

| 欄位 | 型別 | 說明 |
|---|---|---|
| `command_id` | UUID, PK | 由雲端產生,gateway 用它判斷重複 |
| `site_id` | TEXT | |
| `ts` | TIMESTAMPTZ | |
| `point_name` | TEXT | |
| `source` | TEXT | `ai`、`fallback`、`manual` |
| `previous_value` | DOUBLE PRECISION | |
| `proposed_value` | DOUBLE PRECISION | 模型的建議值 |
| `final_value` | DOUBLE PRECISION | 護欄處理後實際送出的值 |
| `inputs` | JSONB | 當時的外氣、負載等輸入 |
| `predicted_kw` | DOUBLE PRECISION | |
| `status` | TEXT | `pending`、`confirmed`、`failed`、`expired` |
| `readback_value` | DOUBLE PRECISION, 可空 | |

### baseline_models

| 欄位 | 型別 | 說明 |
|---|---|---|
| `model_id` | UUID, PK | |
| `site_id` | TEXT | |
| `trained_from` / `trained_to` | TIMESTAMPTZ | 訓練資料的期間 |
| `coefficients` | JSONB | |
| `created_at` | TIMESTAMPTZ | |

### Gateway 本地:outbox(SQLite)

| 欄位 | 說明 |
|---|---|
| `id` | 自動遞增 |
| `topic` | 要發布到哪個 topic |
| `payload` | 訊息內容 |
| `created_at` | |
| `sent_at` | 空值代表尚未上傳 |

選 SQLite 而非 Redis:資料寫在硬碟上,斷電不會遺失,也不需要多跑一個服務。

## 9. 流程

### 9.1 資料上行(每 10 秒)

```
1. gateway → 設備    用 Modbus 讀取原始數字
2. gateway           套點位表(取名、乘倍率)、範圍檢查並標上 quality
                     寫入 outbox
3. gateway → broker  發布到 site/{id}/telemetry,成功後標記 sent_at
4. broker → worker   worker 收到訊息
5. worker → DB       寫入 readings(重複的略過)
                     更新 sites.last_seen_at
                     quality 異常且無進行中告警 → 新增 alerts
                     恢復正常 → 結束該筆 alerts
6. worker → redis    覆蓋該點位的最新值,並發出通知
7. api → 瀏覽器      收到通知後以 WebSocket 推送
```

異常資料照樣上傳並保存,作為事後追查的依據。

### 9.2 指令下行(每 5 分鐘,每個案場)

```
1. worker            查 ai_mode_periods,目前為 off → 結束
2. worker ← DB       讀最近的 readings(只取 quality = ok)
                     資料過舊或不足 → 結束
3. 最佳化            以迴歸模型預測各候選設定值的總耗電,取最低者為建議值
4. 護欄              套用 limits 與 max_step,得出實際值
5. worker → DB       寫入 commands,status = pending
6. worker → broker   發布到 site/{id}/command
7. gateway           command_id 重複或已過期 → 略過
                     再次檢查 limits → 用 Modbus 寫入設備
8. gateway → 設備    讀回該點位
9. gateway → broker  發布到 site/{id}/ack
10. worker → DB      更新 commands 為 confirmed 或 failed;failed → 新增 alerts
```

順序上的決定:

- **先檢查 AI 開關**:關閉時不做任何計算,也最不容易漏掉。
- **先寫入紀錄再送出**:避免出現「設備已被修改但沒有紀錄」的情況。
- **護欄做兩次**:雲端為主;gateway 再檢查一次,作為雲端程式出錯時的最後防線。

### 9.3 斷線

```
上行  發布失敗 → 資料留在 outbox,恢復後依時間順序補傳
      worker 超過 2 分鐘未收到資料 → 新增 gateway_offline 告警

下行  gateway 超過 10 分鐘未收到 heartbeat
      → 把所有可寫入點位寫回 default
      → 在 outbox 記一筆 source = fallback 的指令
      恢復 → 補傳資料與 fallback 紀錄,worker 結束離線告警
      斷線前發出、恢復後才送達的指令 → 已過期,gateway 丟棄
```

### 9.4 AI 開關(R7)

```
1. 使用者 → api      POST 關閉
2. api → DB          結束目前的 on 時段,新增一筆 off 時段
3. api → broker      發布 source = manual 的指令,把設定值設回 default
4. 之後的 9.2        在第 1 步即結束
```

### 9.5 節能計算(R6)

```
1. 取該案場該月的 readings(外氣、負載、實際用電)
2. 排除 ai_mode_periods 為 off 的時段,以及 quality 異常的資料
3. 用 baseline_models 預測同條件下的原始用電
4. 節省 = baseline − 實際
```

## 10. API(初稿)

所有請求以 `X-API-Key` 識別案場,路徑中不帶 `site_id`。

| 方法 | 路徑 | 用途 | 需求 |
|---|---|---|---|
| GET | `/health` | 健康檢查 | |
| GET | `/site` | 案場狀態、是否離線 | R5 |
| GET | `/readings` | 依點位與時間範圍查詢 | R1 |
| GET | `/readings/latest` | 所有點位的最新值 | |
| GET | `/alerts` | 告警清單 | R2、R5 |
| GET | `/commands` | 指令紀錄 | R8 |
| GET | `/ai-mode` | 目前開關狀態與歷史 | R7 |
| POST | `/ai-mode` | 切換開關 | R7 |
| GET | `/savings?month=2026-10` | 每月節能 | R6 |
| WS | `/ws` | 即時資料 | |

## 11. 部署

- 一台 EC2,所有雲端容器以 Docker Compose 執行。
- GitHub Actions:測試 → 建置 image → 推送到 ECR → 在 EC2 上更新。
- Dashboard 靜態檔由 `caddy` 直接提供。
- 開啟 AWS Budgets 費用警示。

### 上 production 會補的部分

| 現況 | 會改成 | 原因 |
|---|---|---|
| 自架 TimescaleDB | 代管的時序資料庫 | RDS 不提供 TimescaleDB 外掛,不能直接換成 RDS |
| 自架 Mosquitto | AWS IoT Core 或有叢集的 broker | 設備憑證管理與可用性 |
| 自架 Redis | ElastiCache | 減少維運負擔 |
| 單台 EC2 | 多台加負載平衡 | 單點故障 |
| 查詢時過濾 `site_id` | 加上 PostgreSQL Row Level Security | 第二層防護 |
| 原始資料永久保存 | 壓縮與降採樣規則 | 儲存成本 |

## 12. 已決定

| 項目 | 決定 |
|---|---|
| EC2 | `t3.small`,東京區域,30 GB 硬碟,固定 IP |
| 模擬器 | 簡化公式:冷氣需求隨時間與外氣變化;主機耗電受出水溫與冷卻水溫影響;風扇耗電與轉速三次方成正比 |
| 可控制的設定值 | 兩個:冰水出水溫、冷卻水塔風扇轉速 |
| 兩個案場 | 使用不同係數,各自有不同的最佳設定 |
| 故障開關 | 感測器卡住、倍率錯誤、斷線,用於示範 R2 到 R5 |
| Gateway 遠端更新 | 只寫進文件,不實作 |
| Image 倉庫 | ECR。GitHub Actions 與 EC2 都以 IAM 角色存取,不放長期金鑰 |
| 網域 | 先用 sslip.io,之後可換成自有網域 |
| 最佳化工作的鎖 | 不實作。demo 只有一個 worker,不會有兩個工作同時對同一案場下指令;多個 worker 時再以 Redis 鎖補上 |
