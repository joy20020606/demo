# 🎯 進度看板 Status

> 詳細紀錄請看 `PROGRESS.md`，整體說明看 `README.md`。

---

## 🌐 Live Demo

- **前端**：<https://demo-1l53.vercel.app/>
- **API**：Fastify 部署在 Railway（首頁有 Swagger UI 連結）

---

## 📍 我在哪

```
Phase 0  ✅  Monorepo 骨架 + 5 packages 空殼 + Prisma schema
Phase 1  ✅  Customer CRUD + Fastify Plugin + Zod 驗證 + Swagger UI
Phase 2  ✅  Deal Kanban + 狀態機 + 409 Conflict 保護
Phase 3  ✅  Next.js 前端 + TanStack Query + dnd-kit 拖拉
Phase 4  ✅  Railway + Vercel 部署上線
Phase 5  ✅  ContactLog CRUD + Claude API 摘要 + 商機 AI 評分
                          ↓
Phase 6  🔄 ◀── 下一步   RAG（pgvector + Voyage embedding）
Phase 7  ⭐⬜            Agent（Claude Tool Use + multi-turn）
Phase 8  ⬜              LINE Bot（bonus）
```

⭐ = L2 加分重點

---

## ✅ 目前已交付能力

### 後端（API on Railway）
- [x] Fastify + Zod (type provider) + fastify-plugin
- [x] Customer CRUD：5 endpoints
- [x] Deal CRUD：6 endpoints（含狀態機專屬入口）
- [x] ContactLog CRUD：3 endpoints
- [x] AI Service 層：summarizeCustomerContacts + scoreDealById
- [x] 2 個 AI endpoints：`POST /api/customers/:id/ai-summary` + `POST /api/deals/:id/ai-score`
- [x] 自定義 AppError 子類：NotFoundError (404) / ValidationError (400) / ConflictError (409)
- [x] Swagger UI 自動產出（Zod schema → OpenAPI）
- [x] Prisma plugin（fastify-plugin DI + onClose disconnect）
- [x] CORS production 白名單
- [x] Migration 自動跑（部署時 prisma migrate deploy）
- [x] Healthcheck `/health`

### 前端（Web on Vercel）
- [x] Next.js 14 App Router + Tailwind
- [x] 首頁（hero + 試用引導 + 技術亮點）
- [x] 客戶列表頁 + 詳細頁 (`/customers/[id]`)
- [x] Kanban 商機看板 + 拖拉 + 終態鎖
- [x] 聯絡紀錄 CRUD UI
- [x] AI 摘要面板（按鈕觸發，顯示「現況 + 下一步」）
- [x] Deal 卡 AI 評分 badge + 重評按鈕
- [x] TanStack Query：cache + invalidate + optimistic update + rollback
- [x] dnd-kit：6 欄 Kanban，pointer + keyboard sensor (a11y)
- [x] Type-safe API client：reuse @sme-crm/shared Zod schema

### AI（Claude API）
- [x] `@sme-crm/ai` package：summarizeContacts + scoreDeal
- [x] 結構化 JSON 輸出（system prompt + fallback parsing）
- [x] AI Service 層做「DB 拉資料 → call Claude → 寫回 DB」
- [x] 評分結果 cache 在 `deal.aiScore` / `deal.aiScoreReason`
- [x] 按鈕觸發機制（避免重複燒 token）

### Infrastructure
- [x] pnpm workspace monorepo（5 packages）
- [x] Docker compose local PostgreSQL
- [x] Railway 部署（railway.json + start/build scripts + migrate deploy）
- [x] Vercel 部署（vercel.json + monorepo Root Directory）
- [x] git push → Vercel + Railway 自動部署

---

## 🎯 Phase 6 預告（下一步）— RAG ⭐ L2 重點

**做什麼**：
- PostgreSQL 加 **pgvector** extension
- 用 **Voyage AI** 把每筆 ContactLog 算成 embedding 存進 DB
- 語意搜尋 endpoint：`POST /api/search` body `{ query: "提到報價單的客戶" }` → 回相關 ContactLog
- 前端搜尋框：輸入自然語言 → 顯示相關客戶/聯絡紀錄

**面試可講**：
- chunking 策略（怎麼切資料）
- embedding 模型選擇（為什麼 Voyage 不用 OpenAI）
- similarity threshold（餘弦相似度怎麼定門檻）
- hybrid search（向量 + keyword 混合）

預估時間：**6-8 小時**。

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
**完成 Phase 6-7 是 L2 加分項，會大幅提升被叫面試的機率。**

---

## 🆘 卡住的時候

| 症狀 | 看哪裡 |
|---|---|
| 環境跑不起來 | `SETUP.md` Troubleshooting 表 |
| 不知道現在該做什麼 | 這份 `STATUS.md`（你正在看的） |
| 想看完整脈絡 | `PROGRESS.md` |
| 想了解整體架構 | `README.md` |
