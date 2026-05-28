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
                          ↓
Phase 5  🔄 ◀── 下一步   ContactLog + Claude 摘要 + AI 評分
                          ↓
Phase 6  ⭐⬜            RAG（pgvector + Voyage embedding）
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
- [x] 自定義 AppError 子類：NotFoundError (404) / ValidationError (400) / ConflictError (409)
- [x] Swagger UI 自動產出（Zod schema → OpenAPI）
- [x] Prisma plugin（fastify-plugin DI + onClose disconnect）
- [x] CORS production 白名單（FRONTEND_URL + *.vercel.app）
- [x] Migration 自動跑（部署時 prisma migrate deploy）
- [x] Healthcheck `/health`

### 前端（Web on Vercel）
- [x] Next.js 14 App Router + Tailwind
- [x] 首頁、客戶頁、Kanban 頁
- [x] TanStack Query：cache + invalidate + optimistic update + rollback
- [x] dnd-kit：6 欄 Kanban，pointer + keyboard sensor (a11y)
- [x] 終態 (WON/LOST) 卡片前端 disable
- [x] Type-safe API client：reuse @sme-crm/shared Zod schema

### Infrastructure
- [x] pnpm workspace monorepo（5 packages）
- [x] Docker compose local PostgreSQL
- [x] Railway 部署設定（railway.json + start/build scripts）
- [x] Vercel 部署設定（vercel.json + monorepo Root Directory）
- [x] git push → Vercel + Railway 自動部署

---

## 🎯 Phase 5 預告（下一步）

**目標**：把後端的「3 個資料模型」收齊，前端加 AI 功能

要做的：
1. `ContactLog` CRUD endpoints（schema 已存在，缺 service + route）
2. `summarizeContacts` 串前端：客戶詳細頁顯示 AI 摘要
3. `scoreDeal` 串前端：商機卡片顯示 AI 分數 + 理由

預估時間：**4-6 小時**。

---

## 📊 距離投履歷還差多少

| 里程碑 | 狀態 |
|---|---|
| Day 0 骨架 | ✅ |
| Phase 1-2 後端基礎 | ✅ |
| Phase 3 前端 Kanban | ✅ |
| Phase 4 部署上線 + 公開 demo URL | ✅ |
| Phase 5 AI 基礎功能 | ⬜ |
| Phase 6 RAG（L2 加分） | ⬜ |
| Phase 7 Agent（L2 加分） | ⬜ |
| README 完善 + 截圖 | ✅ |

**目前已可作為「全棧 + 部署」demo 投履歷。**
**完成 Phase 5-7 是 L2 加分項，會大幅提升被叫面試的機率。**

---

## 🆘 卡住的時候

| 症狀 | 看哪裡 |
|---|---|
| 環境跑不起來 | `SETUP.md` Troubleshooting 表 |
| 不知道現在該做什麼 | 這份 `STATUS.md`（你正在看的） |
| 想看完整脈絡 | `PROGRESS.md` |
| 想了解整體架構 | `README.md` |
