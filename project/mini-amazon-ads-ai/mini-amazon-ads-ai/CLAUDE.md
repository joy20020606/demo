# CLAUDE.md

> 給 Claude Code 的工作守則。每次 session 自動載入。

## 專案概覽

Mini Amazon Ads AI Dashboard — 求職作品集 demo,展示全端 + AI Agent 能力。
- 後端:FastAPI(async)+ PostgreSQL + ClickHouse + Claude SDK
- 前端:Next.js 14 App Router + TypeScript + Tailwind
- 編排:Docker Compose

## 架構規則

1. **多租戶**:所有 PostgreSQL 表帶 `tenant_id`,所有 query 必須加 `WHERE tenant_id = ?`。違反 = 安全漏洞。
2. **資料分層**:業務資料(會員、設定)→ PostgreSQL;時序廣告數據(metrics)→ ClickHouse。
3. **JWT**:登入時把 `tenant_id` 寫進 token,每個 API 用 `get_current_user` 取出。

## 程式風格

- Python:type hints 必加、用 `async def`、SQLAlchemy 用 2.0 語法(`Mapped[]`、`select()`)
- TypeScript:`strict` 模式、interface 優於 type、避免 `any`
- 命名:snake_case(Python)、camelCase(TS)、kebab-case(URL)

## 開發流程

- 每完成一個邏輯單元就 `git commit`,訊息用英文
- 改後端後 docker compose 會自動 reload(volume mount)
- 改前端後 Next.js dev server 自動 HMR
- 改資料模型 → 重跑 `python -m scripts.init_db`(會清空)

## 測試指令

```bash
# 後端啟動
docker compose up -d backend

# 看 log
docker compose logs -f backend

# 重置資料
docker compose exec backend python -m scripts.init_db
docker compose exec backend python -m scripts.seed_data

# 進 DB
docker compose exec postgres psql -U admin -d ads_ai
docker compose exec clickhouse clickhouse-client -u admin --password secret
```

## 已知簡化(求職 demo 範圍)

- 沒做 Alembic migration(直接 drop + create)
- 沒做 Celery(同步處理)
- Amazon API 用 mock,沒做 OAuth
- 沒寫測試(可以加 pytest)
- shadcn/ui 用簡單 Tailwind 替代(避免初始化複雜)

## 不要做的事

- ❌ 不要把 `tenant_id` 過濾邏輯遺漏
- ❌ 不要把 ClickHouse 當 OLTP 用(別寫 UPDATE/DELETE 單筆)
- ❌ 不要在前端硬編 API URL,用 `process.env.NEXT_PUBLIC_API_URL`
- ❌ 不要把 secret 硬編,用環境變數
