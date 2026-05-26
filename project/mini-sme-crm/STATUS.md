# 🎯 進度看板 Status

> 每天打開 VS Code 第一件事看這份。詳細紀錄請看 `PROGRESS.md`。
>
> **預覽方式**：VS Code 開這檔 → 按 `Ctrl+Shift+V` 開 Markdown Preview

---

## 📍 我在哪

```
Phase 0  ✅ ───────────  Monorepo 骨架完成
                          ↓
Phase 1  🔄 ◀── 你在這  Customer CRUD（要開始）
                          ↓
Phase 2  ⬜              Deal Kanban
Phase 3  ⬜              ContactLog + 基礎 AI 摘要
Phase 4  ⬜              AI 商機評分
Phase 5  ⬜              Next.js 前端頁面
Phase 6  ⭐⬜            RAG（pgvector + Voyage）
Phase 7  ⭐⬜            Agent（Claude Tool Use）
Phase 8  ⬜              CI/CD（Vercel + Railway）
Phase 9  ⬜              LINE Bot（bonus）
Phase 10 ⬜              ADR 文件 + Demo 影片
```

---

## ✅ Day 0 已完成清單

- [x] pnpm workspace monorepo 結構
- [x] 5 個 packages：shared / db / ai / api / web
- [x] PostgreSQL 16 in Docker
- [x] Prisma schema（Customer、Deal、ContactLog）
- [x] Seed 範例資料（3 客戶、2 商機、1 聯絡記錄）
- [x] Fastify `/health` endpoint
- [x] Next.js 14 App Router + Tailwind 骨架
- [x] Claude API wrappers（summarizeContacts、scoreDeal）
- [x] 三件套文件（README、SETUP、PROGRESS）
- [ ] **Git commit + push to GitHub** ← 現在做這個

---

## 🎯 下一個 30 分鐘做什麼

### Step 1: 建立 GitHub repo
1. 開 https://github.com/new
2. Repository name: `mini-sme-crm`
3. **設 Private**（DEMO 用，不公開）
4. **不要** 勾任何「Initialize with」選項
5. Create

### Step 2: 本地 git + push

```powershell
cd C:\Users\asus5\Documents\00_demo\project\mini-sme-crm

git init
git branch -m main
git add .
git commit -m "feat: bootstrap mini-sme-crm monorepo (Day 0)"

# 把下面網址換成你剛剛建的 repo
git remote add origin https://github.com/<你的帳號>/mini-sme-crm.git
git push -u origin main
```

### Step 3: 驗收
- 打開你的 GitHub repo 頁面
- README 自動顯示 ✅
- 看得到 packages/ 結構 ✅

---

## ➡️ Phase 1 預告（commit 完進這個）

**目標**：5 個 Customer CRUD endpoints 上線

要做的檔案（依序）：
1. `packages/api/src/plugins/prisma.ts` — Fastify 注入 Prisma client
2. `packages/api/src/plugins/validation.ts` — Zod 驗證中介層
3. `packages/api/src/services/customer-service.ts` — 業務邏輯層
4. `packages/api/src/routes/customers.ts` — 5 個 endpoints
5. `packages/api/tests/customers.test.ts` — 單元測試

預期完成後可以：
```powershell
curl http://localhost:3001/api/customers           # 列表
curl http://localhost:3001/api/customers/<id>      # 單筆
curl -X POST http://localhost:3001/api/customers -H "Content-Type: application/json" -d '{"name":"新客戶"}'
```

**預估時間**：4-6 小時。

---

## 🆘 卡住的時候

| 症狀 | 看哪裡 |
|---|---|
| 環境跑不起來 | `SETUP.md` Troubleshooting 表 |
| 不知道現在該做什麼 | 這份 `STATUS.md`（你正在看的） |
| 想看完整脈絡 | `PROGRESS.md` |
| 想了解整體架構 | `README.md` |

---

## 📊 距離投履歷還差多少

| 里程碑 | 狀態 |
|---|---|
| Day 0 骨架 | ✅ |
| Phase 1-5 基礎全棧 | 0/5 |
| Phase 6 RAG（L2 加分） | ⬜ |
| Phase 7 Agent（L2 加分） | ⬜ |
| Phase 8 部署上線 | ⬜ |
| 公開 demo URL（給面試官試用） | ⬜ |
| README 完善 + 截圖 | ⬜ |

**完成 Phase 1-7 可投履歷。Phase 8-10 是 bonus。**
