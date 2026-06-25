# retail-elt-platform

電商營收 ELT 平台 — Airflow + dbt + Postgres + Metabase 全套示範,
JD 對位:Data Engineer (Python + SQL + Airflow + dbt)。

> **狀態:Week 1 Day 1**
> 只有 hello world DAG(simulator → stg_orders → done),目的是先把整條鏈打通。
> Week 2 會加上 dim/fact、SCD2、incremental、tests、Slack 通知。
> Week 3 會加上 Metabase、dbt docs、漂移偵測、Railway 部署。

## 架構(目前)

```
┌──────────────────────────────────────────────────────────────┐
│ Airflow (LocalExecutor)        webserver: localhost:8080      │
│   └─ DAG: hello_elt                                           │
│        ├─ simulate_orders  (Python, Faker)                    │
│        ├─ dbt_run          (bash → /home/airflow/dbt_venv)    │
│        └─ hello            (print 成功)                       │
└─────────────────────────────────┬────────────────────────────┘
                                  │
                                  ▼
┌──────────────────────────────────────────────────────────────┐
│ Postgres 16                                                   │
│   db: airflow_meta   (Airflow metadata)                       │
│   db: warehouse      (我們的資料倉儲)                          │
│     ├─ raw.customers / raw.products / raw.orders              │
│     └─ analytics_staging.stg_orders   ← dbt 跑完會建這個      │
└──────────────────────────────────────────────────────────────┘
```

## 環境需求

- Docker Desktop(Windows / Mac / Linux)
- 4 GB RAM 配給 Docker(Settings → Resources)
- Port 5432 / 8080 沒被其他東西佔用

## Quickstart(PowerShell)

```powershell
# 1. 切到專案根目錄
cd C:\Users\asus5\Documents\00_demo\project\retail-elt-platform

# 2. 第一次先 build image(把 dbt 裝進 Airflow image,約 3~5 分鐘)
docker compose build

# 3. 起所有服務(背景跑)
docker compose up -d

# 4. 等 airflow-init 完成(它會 migrate db + 建 admin 使用者)
docker compose logs -f airflow-init
# 看到 "User 'admin' created with role 'Admin'" 後按 Ctrl+C 跳出 logs

# 5. 打開 Airflow Web UI
#    http://localhost:8080
#    帳號: admin  密碼: admin
```

## 親手跑一次 hello_elt(第一個里程碑)

1. 進 Web UI,在左邊 DAG 列表找到 **hello_elt**
2. 點左邊的開關把它 **unpause**(從灰色變藍色)
3. 點右邊的 ▶ (play) → Trigger DAG
4. 點 DAG 名稱進去看 Graph view,等三個 task 都變綠

## 驗證資料真的進去了(自己連 Postgres 看)

```powershell
docker exec -it retail-elt-postgres psql -U warehouse -d warehouse
```

進 psql 後:
```sql
-- 該有 100 筆訂單
SELECT COUNT(*) FROM raw.orders;

-- dbt 跑完應該建好 stg_orders view
SELECT * FROM analytics_staging.stg_orders LIMIT 5;

-- 看 amount_dollars 是不是已經從 cents 轉成元
SELECT order_status, COUNT(*), SUM(amount_dollars)
FROM analytics_staging.stg_orders
GROUP BY order_status;

\q
```

## 常用指令

```powershell
# 看哪些容器在跑
docker compose ps

# 看 scheduler 的 log(DAG 不執行時除錯用)
docker compose logs -f airflow-scheduler

# 全部收掉(資料保留在 docker volume)
docker compose down

# 全部收掉 + 砍 volume(從頭來過)
docker compose down -v
```

## 踩雷小抄(Windows + PowerShell)

| 雷 | 修法 |
|---|---|
| `docker compose build` 卡很久 | 第一次正常,3~5 分鐘 |
| Airflow Web 開不起來 | `docker compose ps` 看 airflow-webserver 是否 healthy,沒 healthy 看 logs |
| 5432 被佔 | 改 docker-compose.yml 裡 postgres 的 ports 成 `"5433:5432"` |
| DAG 不出現 | scheduler 預設 30 秒掃一次,等等就好;或 `docker compose restart airflow-scheduler` |
| 改了 dag 沒生效 | 同上,scheduler 會自動 reload |
| 改了 docker-compose.yml | `docker compose up -d` 不是 restart(restart 不重讀) |

## 接下來(下個里程碑)

- [ ] Week 1 完成本檔 quickstart,你親手跑一次,看到三個 task 都綠
- [ ] Week 2:加 `dim_customer`(SCD2)、`fct_orders`(incremental)、`metrics_daily_revenue`,DAG 串完整 ELT
- [ ] Week 3:Metabase + dashboard + dbt docs + 漂移偵測 + Railway 部署
