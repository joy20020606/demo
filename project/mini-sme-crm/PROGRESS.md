# 進度紀錄 (Progress)

> 最後更新：2026-05-26
> 狀態：**Day 0 — Monorepo 骨架完成**。尚未 `pnpm install`、尚未跑通。
> 目標職位：**藍圖思維 軟工 L2**（68k-198k，期望落點 95-115k）

---

## 回來時做什麼

### 一、確認環境

```powershell
cd C:\Users\asus5\Documents\00_demo\project\mini-sme-crm
node --version    # >= v20
pnpm --version    # >= 9
docker --version  # Docker Desktop 在跑
```

### 二、第一次跑起來（Day 0 驗收）

```powershell
# 1. 安裝
pnpm install

# 2. 環境變數
copy .env.example packages\db\.env
copy .env.example packages\api\.env

# 3. 起 DB
pnpm db:up

# 4. 建表 + 塞資料
pnpm db:generate
pnpm db:migrate    # 名稱輸入 init
pnpm db:seed

# 5. 啟動 API
pnpm dev:api

# 6. 另開視窗測試
curl http://localhost:3001/health
```

看到 `{"status":"ok",...}` 就算 Day 0 通過。

### 三、Day 0 通過後 → 進 Phase 1

開始寫 `/api/customers` 的 CRUD。

---

## 整體進度 Roadmap（L2 版本）

| Phase | 主題 | 目的 / L2 對應點 | 狀態 |
|-------|------|-----------------|------|
| **0** | Monorepo 骨架 + 三件套文件 | 基礎工程素養 | 🔄 程式生完，未跑通 |
| **1** | Customer CRUD + Prisma plugin | API 設計、ORM | ⬜ |
| **2** | Deal Kanban（拖拉式 stage 切換） | 商業流程系統化 | ⬜ |
| **3** | ContactLog CRUD + 基礎 AI 摘要 | LLM API 整合 | ⬜ |
| **4** | AI 商機評分（Claude API） | Prompt engineering | ⬜ |
| **5** | Next.js 前端基礎頁面 + Tailwind | 全端能力 | ⬜ |
| **6** | ★ **RAG**：pgvector + Voyage embedding + 相似度檢索 | **L2 要求第 9 點：RAG** | ⬜ |
| **7** | ★ **Agent**：Claude Tool Use + 多輪對話 + chat UI | **L2 要求第 9 點：Agent** | ⬜ |
| **8** | GitHub Actions CI + Vercel + Railway 部署 | DevOps、自動化 | ⬜ |
| **9** | LINE Bot 整合（bonus） | **L2 要求第 8 點：第三方串接** | ⬜ |
| **10** | ADR 文件 + Demo 影片 + README 完善 | 技術寫作、Tech Leadership | ⬜ |

**核心三大殺手鐧**（L2 面試 selling point）：
1. **RAG**：對應「客戶上萬筆聯絡記錄如何 retrieval」的真實 SME 痛點
2. **Agent**：用 Claude Tool Use 讓 AI 跨模組操作，**不是只會包 ChatGPT**
3. **Monorepo + ADR**：架構演進故事可講，每個決策有書面紀錄

---

## 已完成（Day 0）

- ✅ pnpm workspace monorepo 骨架
- ✅ 5 個 packages：`shared`、`db`、`ai`、`api`、`web`
- ✅ `tsconfig.base.json` 嚴格 TS 設定
- ✅ `docker-compose.yml`（PostgreSQL 16）
- ✅ Prisma schema（Customer、Deal、ContactLog）
- ✅ Claude API wrapper 骨架（`summarizeContacts`、`scoreDeal`）
- ✅ Fastify server + `/health` route
- ✅ Next.js 14 App Router 骨架 + Tailwind
- ✅ 三件套文件骨架（README、SETUP、PROGRESS）

---

## 重要參考

| 路徑 | 用途 |
|------|------|
| `README.md` | 專案介紹 + 架構 + Quick Start |
| `SETUP.md` | 手把手安裝 + commits 計畫 + 部署 |
| `packages/db/prisma/schema.prisma` | 資料模型（將在 Phase 6 加 embedding 欄位） |
| `packages/ai/src/` | AI 邏輯（Phase 6/7 主戰場） |
| `docs/adr/` | 架構決策紀錄（之後建） |

---

## 各 Phase 預告

### Phase 1 — Customer CRUD（預估 4-6h）

**要做**：
1. `packages/api/src/plugins/prisma.ts` — Fastify plugin 注入 Prisma client
2. `packages/api/src/middleware/validation.ts` — Zod 驗證中介層
3. `packages/api/src/routes/customers.ts` — GET / POST / PATCH / DELETE
4. 單元測試 `customers.test.ts`

**面試話術**：「Fastify plugin 模式 + Zod runtime validation + Prisma type-safe DB layer——三層責任明確分離。」

---

### Phase 6 — RAG（預估 8-10h，最硬的一段）

**要做**：
1. docker-compose 換 image：`pgvector/pgvector:pg16`
2. Prisma schema：`ContactLog` 加 `embedding Unsupported("vector(1024)")?`
3. Migration + `CREATE EXTENSION vector`
4. `packages/ai/src/embeddings.ts` — Voyage AI client
5. `packages/db/src/vector-search.ts` — raw SQL 封裝（cosine similarity）
6. `packages/ai/src/rag/retrieve.ts` — RAG 檢索 + Claude 回答
7. API endpoint：`POST /api/customers/:id/ask` — 用 RAG 回答關於該客戶的問題
8. `docs/adr/002-pgvector-over-pinecone.md`

**為什麼選 pgvector 而非 Pinecone**（ADR-002 重點）：
- 中小企業客戶不想再多管一個服務
- 資料不用跨網段同步（embedding 跟業務資料同庫，transaction 一致）
- 成本：pgvector 免費，Pinecone 起跳每月 $70+

**為什麼選 Voyage AI 而非 OpenAI embeddings**：
- Anthropic 官方推薦，跟 Claude 同生態
- `voyage-3` 模型在繁體中文檢索任務表現好（針對 ACTGSYS 客戶情境）

---

### Phase 7 — Agent（預估 10-12h，最有 wow factor）

**要做**：
1. `packages/ai/src/agent/tools.ts` — 定義 4 個 tools：
   - `search_customers`（reuse Phase 1 service）
   - `create_deal`（reuse Phase 2 service）
   - `log_contact`（reuse Phase 3 service）
   - `rag_recall`（reuse Phase 6 retrieval）
2. `packages/ai/src/agent/loop.ts` — Claude Tool Use 迴圈
3. `packages/db/prisma/schema.prisma` — 加 `Conversation`、`Message` 表
4. API endpoint：`POST /api/agent/chat`（SSE streaming）
5. Web：`packages/web/src/app/chat/page.tsx` — chat UI
6. `docs/adr/003-agent-loop-design.md`

**Demo 劇本**（面試現場演）：
1. 使用者輸入：「幫我看看王大明最近狀況，順便建一筆 50 萬的商機」
2. AI 先 call `search_customers("王大明")` → 拿到 customerId
3. AI call `rag_recall(customerId)` → 抓最近聯絡記錄
4. AI 回覆「最近一次聯絡是 5/20 寄報價單...」
5. AI call `create_deal(customerId, "新案", 500000)` → 商機建立
6. AI 回覆「商機已建立，ID: ...」

**這段演示完，L2 拿不到我頭給你。**

---

## 時程估算

| 階段 | 時數 | 累積 |
|------|------|------|
| Phase 0 | 已完成 | 0h |
| Phase 1-5（基礎全棧） | 25-30h | ~30h |
| Phase 6（RAG） | 8-10h | ~40h |
| Phase 7（Agent） | 10-12h | ~52h |
| Phase 8（CI/CD） | 4-6h | ~58h |
| Phase 9（LINE Bot bonus） | 6-8h | ~66h |
| Phase 10（文件 + demo 影片） | 4-6h | ~72h |

**理想時程**：每天 4 小時 → **18 天完成**（含 RAG + Agent + 部署）
**快速版**：每天 8 小時 → **9 天完成**

---

## 風險清單

| 風險 | 機率 | 影響 | 對策 |
|------|------|------|------|
| Voyage API 註冊困難 | 低 | 中 | Plan B：用 OpenAI embeddings（差別不大，ADR 改寫即可） |
| pgvector + Prisma 整合卡關 | 中 | 高 | 預留 4h 緩衝，寫成 ADR 反而加分 |
| Railway 對 monorepo 部署不友善 | 中 | 中 | 用 Dockerfile 部署，繞過 Railway 自動偵測 |
| Vercel monorepo build 卡 transpilePackages | 低 | 低 | 已在 next.config.mjs 預設好 |
| Claude API 費用爆炸 | 低 | 低 | demo 期間用 sonnet-4-6，控制 max_tokens |
