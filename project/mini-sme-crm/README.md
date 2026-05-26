# Mini SME CRM — AI-powered CRM for Small & Medium Enterprises

> An AI-augmented customer relationship management system for SMEs.
> 為中小企業打造的 AI 強化型 CRM —— 把散落在 Excel、LINE、Email 的客戶互動，統一進系統並轉成可行動的洞察。

---

## 解決的問題 The Problem

台灣中小企業常見痛點：

- 業務團隊用 **Excel** 管客戶，公司一變大就難維護
- 客戶聯絡記錄散在 **LINE / Email / 紙本**，沒人能彙整
- 老闆問「這個月哪些案子快成交了？」沒人答得出來
- 想導入 AI 卻不知從何下手，只會包 ChatGPT 介面

**這個 demo 解決四件事：**
1. 統一的客戶 + 商機 + 聯絡記錄資料模型
2. AI 自動產生「客戶現況摘要 + 下一步建議」
3. AI 給每個商機打 0-100 分（成交可能性）+ 評分理由
4. **RAG + Agent**：用 pgvector 檢索歷史互動 + Claude Tool Use 讓 AI 跨模組執行動作

---

## AI 能力 AI Capabilities

| 能力 | 技術 | 為什麼這樣設計 |
|------|------|---------------|
| 客戶摘要 | Claude API + structured output | 用 system prompt + JSON schema 強制結構化輸出，便於下游使用 |
| 商機評分 | Claude API + 多維度評分 prompt | 給定評分維度（金額、stage、互動頻率）避免模型自由發揮 |
| **RAG** | pgvector + Voyage AI embedding | 中小企業上萬筆聯絡記錄，全塞 prompt 不可行；用相似度檢索只挑相關的 |
| **Agent** | Claude Tool Use + multi-turn loop | AI 不只回答，能跨模組執行操作（搜尋客戶、建立商機、寫入聯絡記錄） |

---

## 技術架構 Architecture

| 層級 | 技術 | 部署 |
|------|------|------|
| Web (前端) | Next.js 14 App Router + Tailwind | **Vercel** |
| API (後端) | Fastify + TypeScript | **Railway** |
| DB | PostgreSQL + Prisma | Railway (managed Postgres) |
| AI | Claude API (`claude-sonnet-4-6`) | — |
| CI/CD | GitHub Actions → 自動觸發 Vercel + Railway 部署 | — |

**為什麼用 Monorepo（pnpm workspace）**：
前後端共用 TypeScript types（`@sme-crm/shared`）、共用 Prisma client（`@sme-crm/db`）、共用 AI 邏輯（`@sme-crm/ai`），單一 PR 改動可同時影響全棧，避免前後端型別漂移。

---

## Quick Start

```powershell
# 1. 安裝
pnpm install

# 2. 起本機 PostgreSQL
pnpm db:up

# 3. 建表 + 塞範例資料
pnpm db:migrate
pnpm db:seed

# 4. 啟動 API
pnpm dev:api
# → http://localhost:3001/health

# 5. 啟動前端（另一個視窗）
pnpm dev:web
# → http://localhost:3000
```

完整步驟、踩雷預防、commit 計畫見 [SETUP.md](./SETUP.md)。

---

## Monorepo Layout

```
mini-sme-crm/
├── packages/
│   ├── shared/      # 共用 TS types + Zod schemas + errors
│   ├── db/          # Prisma schema + client + seed
│   ├── ai/          # Claude API wrapper（summarize、score）
│   ├── api/         # Fastify 後端 → Railway
│   └── web/         # Next.js 前端 → Vercel
├── docker-compose.yml
├── pnpm-workspace.yaml
├── tsconfig.base.json
├── README.md
├── SETUP.md
└── PROGRESS.md
```

**依賴方向**：`web → api → ai → db → shared`（單向，無循環）

---

## Roadmap

| Phase | 主題 | 狀態 |
|-------|------|------|
| 0 | Monorepo 骨架 + 三件套文件 | 🔄 進行中 |
| 1 | Customer CRUD + Prisma plugin | ⬜ |
| 2 | Deal Kanban（拖拉式 stage 切換） | ⬜ |
| 3 | ContactLog + 基礎 AI 摘要 | ⬜ |
| 4 | AI 商機評分 | ⬜ |
| 5 | Next.js 前端基礎頁面 | ⬜ |
| **6** | **RAG**：pgvector + Voyage embedding 相似度檢索 | ⬜ |
| **7** | **Agent**：Claude Tool Use + chat UI | ⬜ |
| 8 | GitHub Actions CI + Vercel + Railway 部署 | ⬜ |
| 9 | LINE Bot 整合（bonus） | ⬜ |
| 10 | ADR 文件 + Demo 影片 | ⬜ |

詳細進度與每個 Phase 設計理由見 [PROGRESS.md](./PROGRESS.md)。

---

## License

MIT
