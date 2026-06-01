# 進度紀錄 (Progress)

> 最後更新：2026-06-01
> 狀態：**Phase 5 完成 — ContactLog + AI 摘要 + AI 評分上線**
> 目標職位：**藍圖思維 軟工 L2**（68k-198k，期望落點 95-115k）

---

## 🌐 Live URLs

| 服務 | URL |
|---|---|
| 前端 (Vercel) | <https://demo-1l53.vercel.app/> |
| API (Railway) | <https://demo-production-7897.up.railway.app/> |
| Swagger UI | <https://demo-production-7897.up.railway.app/docs> |
| GitHub | <https://github.com/joy20020606/demo/tree/main/project/mini-sme-crm> |

---

## 整體進度 Roadmap

| Phase | 主題 | 目的 / L2 對應點 | 狀態 |
|-------|------|-----------------|------|
| **0** | Monorepo 骨架 + 三件套文件 | 基礎工程素養 | ✅ |
| **1** | Customer CRUD + Fastify Plugin + Zod 驗證 + Swagger 自動文件 | API 設計、分層架構、DI | ✅ |
| **2** | Deal Kanban CRUD + 狀態機（終態保護） + 409 Conflict 語意 | 商業流程系統化、HTTP 規範 | ✅ |
| **3** | Next.js 前端 + TanStack Query + dnd-kit 拖拉 + Optimistic UI | 全棧能力、現代前端 | ✅ |
| **4** | Railway (API + Postgres) + Vercel (Web) 自動部署 | DevOps、CI/CD | ✅ |
| **5** | ContactLog CRUD + Claude 摘要 + 商機 AI 評分 | LLM API 整合、Prompt engineering | ✅ |
| **6** | ★ **RAG**：pgvector + Voyage embedding + 相似度檢索 | **L2 加分：RAG** | 🔜 |
| **7** | ★ **Agent**：Claude Tool Use + 多輪對話 + chat UI | **L2 加分：Agent** | 🔜 |
| **8** | LINE Bot 整合（bonus） | **L2 加分：第三方串接** | 🔜 |

★ = L2 面試殺手鐧

---

## 🎯 核心 selling point（履歷 + 面試講法）

### 已交付的 5 大能力

1. **全棧 TypeScript Monorepo**
   - pnpm workspace × 5 packages（shared / db / ai / api / web）
   - 前後端共用 Zod schema → runtime 驗證 + TS 型別 + Swagger 文件，三合一
   - 改一份 schema 全棧自動同步，零型別漂移

2. **分層架構 + DI**
   - Route → Service → Data 三層
   - Service constructor 注入 PrismaClient，方便 mock / transaction reuse
   - Phase 7 Agent 也走同一個 service 入口（不重新寫業務邏輯）

3. **狀態機 + HTTP 語意精準**
   - Deal stage 轉移走獨立 endpoint（`PATCH /:id/stage`）
   - 一般 PATCH 用 `Omit<>` 編譯期擋掉 stage 欄位（defense in depth）
   - 自定義 `ConflictError` (409) / `NotFoundError` (404) / `ValidationError` (400)
   - Service 拋業務錯誤，errorHandler 統一翻譯成 HTTP

4. **Optimistic UI + Rollback**
   - dnd-kit 拖卡片瞬間 UI 反應（0ms 體感）
   - React Query mutation 在背景打 API
   - 409 Conflict → 自動 rollback 回原狀 + alert

5. **AI 整合（Phase 5）**
   - `summarizeContacts`：structured JSON output（system prompt + fallback parsing）
   - `scoreDeal`：多維度評分（金額、stage、互動頻率、最近聯絡時間）
   - AI Service 層做「DB 拉資料 → call Claude → 寫回 DB」
   - 按鈕觸發避免重複燒 token；評分結果 cache 在 deal.aiScore

### Phase 6-7 開發中（差異化）

6. **RAG**：pgvector + Voyage AI embedding
7. **Agent**：Claude Tool Use 跨模組執行

---

## 📁 Monorepo 結構

```
mini-sme-crm/
├── packages/
│   ├── shared/      Zod schemas (Customer/Deal/ContactLog) + AppError 子類
│   ├── db/          Prisma schema + client + seed + namespace re-export
│   ├── ai/          Claude API wrapper（summarize、score）
│   ├── api/         Fastify
│   │   ├── plugins/    prisma plugin (fastify-plugin DI), validation errorHandler
│   │   ├── services/   customer-service, deal-service, contact-log-service, ai-service
│   │   └── routes/     health, customers, deals, contact-logs, ai
│   └── web/         Next.js 14 App Router
│       ├── lib/        api-client（型別化 fetch wrapper）, query-keys
│       ├── components/ Header, KanbanBoard, DealCard, ContactLogList, AiSummaryPanel...
│       └── app/        /, /customers, /customers/[id], /deals
├── railway.json
├── vercel.json
├── docker-compose.yml
├── pnpm-workspace.yaml
└── tsconfig.base.json
```

依賴方向：`web → api → ai → db → shared`（單向，無循環）

---

## ✅ 已完成功能清單

### 後端 API（部署在 Railway）

**Customers（5 endpoints）**
- `GET    /api/customers` — list + 搜尋 + 分頁
- `GET    /api/customers/:id`
- `POST   /api/customers`
- `PATCH  /api/customers/:id`
- `DELETE /api/customers/:id`

**Deals（6 endpoints）**
- `GET    /api/deals` — list + stage / customerId 過濾
- `GET    /api/deals/:id`
- `POST   /api/deals` — 驗 customerId 存在
- `PATCH  /api/deals/:id` — 一般欄位（不含 stage）
- `PATCH  /api/deals/:id/stage` — 狀態機轉移（終態 WON/LOST 鎖死，違規 409）
- `DELETE /api/deals/:id`

**ContactLogs（3 endpoints）**
- `GET    /api/customers/:customerId/contact-logs`
- `POST   /api/contact-logs`
- `DELETE /api/contact-logs/:id`

**AI（2 endpoints）**
- `POST   /api/customers/:id/ai-summary` — Claude 摘要客戶現況
- `POST   /api/deals/:id/ai-score` — Claude 給商機評分（寫回 DB cache）

**其他**
- `/health` — Railway healthcheck
- `/docs` — Swagger UI（從 Zod schema 自動產生 OpenAPI）

### 前端 Web（部署在 Vercel）

- `/` — Hero 首頁 + 5 步試用引導 + 6 個技術亮點
- `/customers` — 客戶列表 + 搜尋 + 建立表單
- `/customers/[id]` — 客戶詳細頁 + 聯絡紀錄 timeline + AI 摘要面板
- `/deals` — Kanban 6 欄拖拉 + 終態保護 + AI 評分 badge + 重評按鈕

### 部署 / CI/CD

- Railway：API + PostgreSQL，git push 自動 build → migrate → start
- Vercel：Web，git push 自動 build & deploy
- monorepo 對應設定（railway.json / vercel.json + Root Directory）
- Env vars：DATABASE_URL（Railway 內部）、ANTHROPIC_API_KEY、FRONTEND_URL、API_PUBLIC_URL

---

## 🎯 下一步：Phase 6 RAG（預估 6-8 小時）

### 要做的

1. **DB 層**
   - 在 PostgreSQL 加 `pgvector` extension（Prisma 5.18 支援）
   - schema 加 `embedding` 欄位到 ContactLog（vector(1024) 對應 Voyage AI）
   - migration 跑下去

2. **AI 層**
   - 加 Voyage AI 套件（`voyageai`）
   - 寫 embed function：`embedText(text: string): Promise<number[]>`
   - ContactLog 建立 / 編輯時自動產 embedding

3. **API 層**
   - `POST /api/search` body `{ query: string, customerId?: string }`
     - 把 query embed 成 vector
     - SQL `<->` 餘弦相似度比對
     - 回 top-K 相關 ContactLog
   - 把 customer-level AI 摘要升級成 RAG：只挑相關 contact log 餵 prompt

4. **前端**
   - 全局搜尋框（Header 右上）
   - 搜尋頁 `/search?q=xxx` 顯示語意相關結果

### 面試可講的技術點

- **chunking 策略**：ContactLog summary 大多 < 500 字，一筆當一個 chunk 即可
- **embedding 模型選擇**：為什麼 Voyage 不用 OpenAI（中文表現更好、便宜）
- **similarity threshold**：cosine distance < 0.3 算「相關」
- **hybrid search**：之後可加 keyword (`ILIKE`) + vector 混合
- **cost 控制**：建立時才算 embedding，不在每次 search 重算

---

## 📊 距離投履歷還差多少

| 里程碑 | 狀態 |
|---|---|
| Day 0 骨架 | ✅ |
| Phase 1-2 後端基礎 | ✅ |
| Phase 3 前端 Kanban | ✅ |
| Phase 4 部署上線 + 公開 demo URL | ✅ |
| Phase 5 AI 基礎功能（摘要 + 評分） | ✅ |
| Phase 6 RAG（L2 加分） | ⬜ |
| Phase 7 Agent（L2 加分） | ⬜ |
| README 完善 + 截圖 | ✅ |

**目前已可作為「全棧 + 部署 + AI 整合」demo 投履歷。**
完成 Phase 6-7 是 L2 加分項，會大幅提升被叫面試的機率。

---

## 重要參考

| 路徑 | 用途 |
|------|------|
| `README.md` | 專案介紹 + 架構 + Live Demo + 技術亮點 |
| `STATUS.md` | 進度看板（簡短版） |
| `SETUP.md` | 手把手安裝 + 部署細節 |
| `packages/db/prisma/schema.prisma` | 資料模型（Phase 6 會加 embedding 欄位） |
| `packages/ai/src/` | AI 邏輯（Phase 5 已完成基礎，Phase 6/7 主戰場） |
| `docs/screenshots/` | 給 README 用的截圖 |

---

## 🆘 卡住的時候

| 症狀 | 看哪裡 |
|---|---|
| 環境跑不起來 | `SETUP.md` Troubleshooting 表 |
| 不知道現在該做什麼 | `STATUS.md` |
| 想看完整脈絡 | 這份 `PROGRESS.md` |
| 想了解整體架構 | `README.md` |
