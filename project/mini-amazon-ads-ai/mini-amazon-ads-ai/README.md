# Mini Amazon Ads AI Dashboard

> 求職作品集 demo:FastAPI + Next.js + PostgreSQL + ClickHouse + Claude Agent

模擬 Amazon 賣家廣告數據分析平台,重點展示:
- ✅ FastAPI + async/await + JWT 認證
- ✅ 多租戶 SaaS(tenant_id 隔離)
- ✅ PostgreSQL(業務資料)+ ClickHouse(時序廣告數據)雙層架構
- ✅ Next.js 14 App Router + TypeScript + Tailwind
- ✅ Claude Agent 自動分析 ACOS 並給建議
- ✅ Docker Compose 一鍵啟動

## 🚀 啟動

```bash
# 1. 設定環境變數
cp .env.example .env
# 編輯 .env,填入 ANTHROPIC_API_KEY

# 2. 啟動所有服務
docker compose up -d --build

# 3. 等服務起來後初始化(約 30 秒)
docker compose exec backend python -m scripts.init_db
docker compose exec backend python -m scripts.seed_data

# 4. 開啟瀏覽器
# 前端:        http://localhost:3000
# API 文件:    http://localhost:8000/docs
# ClickHouse:  http://localhost:8123/play
```

預設帳號:`joy@test.com` / 密碼:`password123`

## 📁 結構

```
mini-amazon-ads-ai/
├── backend/                FastAPI 後端
│   ├── app/
│   │   ├── api/            API routes(auth、campaigns、ai)
│   │   ├── core/           設定、資料庫、安全
│   │   ├── models/         SQLAlchemy ORM
│   │   ├── services/       業務邏輯 + AI Agent
│   │   └── main.py
│   ├── scripts/            初始化 + 種子資料
│   ├── requirements.txt
│   └── Dockerfile
├── frontend/               Next.js 前端
│   ├── app/                App Router 頁面
│   ├── components/         React 元件
│   ├── lib/                API client
│   ├── package.json
│   └── Dockerfile
├── docker-compose.yml
├── .env.example
└── README.md
```

## 🎯 功能展示

| 路徑 | 說明 |
|------|------|
| `/login` | JWT 登入 |
| `/campaigns` | 廣告活動列表 + ACOS 指標 |
| `/campaigns/[id]` | 詳細頁 + 時序圖表(查 ClickHouse) |
| `/campaigns/[id]/ai` | Claude Agent 分析 + 建議 |

## 🛠 技術選型理由

**為什麼 PostgreSQL + ClickHouse?**
業務資料(會員、設定)需要事務性 → PostgreSQL。
廣告 metrics 是高頻寫入、聚合查詢主導 → ClickHouse 比 PG 快 10~100 倍。

**為什麼 FastAPI 而非 Flask/Django?**
要呼叫多個外部 API(Amazon、Claude),async/await 大幅提升併發能力。
自動產生 OpenAPI 文件方便前後端協作。

**為什麼 Next.js App Router?**
Server Components 可以在伺服器直接抓 API,不用做 BFF。
shadcn/ui 在 App Router 整合最順。

## ⚠️ Demo 簡化

- Amazon API 用 mock data(真環境要申請 Ads API access)
- LLM Agent 工具只實作 3 個(真產品會更多)
- 沒做 OAuth(只用 email + password 登入示意)
- 沒做 Celery(同步處理就夠示範)

## 📝 License

MIT
