# SETUP — Step-by-Step (中英對照)

> Day 1 完成後手把手操作指南。預期 30 分鐘內看到 `{"status":"ok"}`。

---

## 0. Prerequisites 前置需求檢查

打開 PowerShell 或 Windows Terminal，逐行貼上確認版本：

```powershell
node --version    # 應 >= v20.0.0
pnpm --version    # 應 >= 9.0.0
docker --version  # 確認 Docker Desktop 跑起來
git --version
```

如果 `pnpm` 沒裝：

```powershell
corepack enable
corepack prepare pnpm@latest --activate
pnpm --version
```

---

## 1. Open the project 開啟專案

打開 PowerShell 進到該資料夾：

```powershell
cd C:\Users\asus5\Documents\00_demo\project\mini-maas-ops
```

---

## 2. Init git 起始 git repo

```powershell
git init
git branch -m main
```

> 完整的 commit 計畫見最末段 §8，建議分批 commit 而非一次 dump，展示工程素養。

---

## 3. Install dependencies 安裝套件

```powershell
pnpm install
```

這會：
- 解析 `pnpm-workspace.yaml`
- 安裝 root 和所有 `packages/*` 的 deps
- 自動建立 `packages/shared` 與 `packages/api-gateway` 的 symlink

如果 install 失敗，常見原因：

| 錯誤 | 解法 |
|------|------|
| `ERR_PNPM_PEER_DEP_ISSUES` | 加 `--strict-peer-dependencies=false` |
| `EACCES` 權限問題 | PowerShell 以系統管理員身分執行 |
| 公司防火牆擋 npm registry | `pnpm config set registry https://registry.npmmirror.com` |

---

## 4. Setup environment 環境變數

⚠️ **重要**：`.env` 必須建立在 `packages/api-gateway/` 底下，
不是 monorepo 根目錄。原因：Prisma CLI 與 api-gateway server 都在
該 package 內執行，只會讀同層的 `.env`，不會往上找根目錄。

在專案根目錄執行：

```powershell
copy packages\api-gateway\.env.example packages\api-gateway\.env
```

然後用 VS Code 打開 `packages/api-gateway/.env` **手動編輯**：
- `JWT_SECRET` 改成隨機字串（至少 16 字元，建議 32 bytes）。可以這樣產：
  ```powershell
  node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
  ```
- `DATABASE_URL` 在本機 Docker 不用改（預設值即可）。

---

## 5. Start PostgreSQL 起 PostgreSQL

確認 Docker Desktop 已在執行，然後：

```powershell
pnpm db:up
```

確認 container 跑起來：

```powershell
docker ps
# 應該看到 maas-ops-postgres
```

要看 log 的話：
```powershell
docker logs -f maas-ops-postgres
```

要停掉的話：
```powershell
pnpm db:down
```

---

## 6. Run migrations + seed 建表 + 塞測試資料

```powershell
pnpm db:migrate
```

第一次跑時 Prisma 會：
1. 連到 PostgreSQL
2. 比對 `schema.prisma` 與 DB 狀態
3. 產生 SQL migration 檔
4. 執行 migration
5. 產生 type-safe Prisma client

問你 migration 名字時輸入：`init`

接著塞測試資料：

```powershell
pnpm db:seed
```

成功會印出：
```
✓ Tenant: Houston Transit Authority
✓ User: admin@houston-transit.gov (ADMIN)
✓ User: operator@houston-transit.gov (OPERATOR)
✓ User: viewer@houston-transit.gov (VIEWER)
✓ Route: Downtown → Med Center
✓ Route: Energy Corridor → Galleria
✓ Route: Sugar Land → Downtown
✓ IncentiveProgram: Off-peak Carpool Reward Q3
✅ Seed complete!
```

---

## 7. Run the API 啟動 API

```powershell
pnpm dev
```

成功會看到：
```
✅ Prisma connected to PostgreSQL
✅ JWT plugin registered
🚀 api-gateway listening on http://0.0.0.0:3000
```

### Smoke test 冒煙測試

開另一個 PowerShell 視窗：

```powershell
# 1. Health check
curl http://localhost:3000/health

# 2. Login
curl -X POST http://localhost:3000/auth/login `
  -H "Content-Type: application/json" `
  -d '{"email":"admin@houston-transit.gov","password":"password123"}'

# 把上面回來的 token 存起來，例如：
$TOKEN = "eyJhbGc..."

# 3. List routes (帶 JWT)
curl http://localhost:3000/api/routes -H "Authorization: Bearer $TOKEN"

# 4. Get one route
curl http://localhost:3000/api/routes/<id> -H "Authorization: Bearer $TOKEN"
```

推薦用 [Thunder Client (VS Code)](https://www.thunderclient.com/) 或 [Insomnia](https://insomnia.rest/) 取代 curl，比較好操作。

---

## 8. Conventional Commits 清單

建議**逐步 commit**，模擬真實工程節奏（不要一次大 dump）。直接複製貼上：

```bash
# 1
git add pnpm-workspace.yaml package.json tsconfig.base.json .gitignore
git commit -m "chore: init monorepo with pnpm workspace"

# 2
git add docker-compose.yml .env.example
git commit -m "chore(docker): add docker-compose for local PostgreSQL"

# 3
git add packages/shared
git commit -m "feat(shared): add common types, zod schemas, errors, and pino logger"

# 4
git add packages/api-gateway/package.json packages/api-gateway/tsconfig.json packages/api-gateway/vitest.config.ts packages/api-gateway/src/config.ts packages/api-gateway/src/server.ts packages/api-gateway/src/routes/health.ts
git commit -m "feat(api): initialize Fastify server with health check"

# 5
git add packages/api-gateway/prisma/schema.prisma
git commit -m "feat(db): add Tenant, User, Route, IncentiveProgram Prisma schema"

# 6
git add packages/api-gateway/src/plugins/prisma.ts
git commit -m "feat(db): wire PrismaClient via Fastify plugin"

# 7
git add packages/api-gateway/prisma/seed.ts
git commit -m "feat(db): add seed script for tenant, users, routes, incentive program"

# 8
git add packages/api-gateway/src/plugins/jwt.ts packages/api-gateway/src/middleware/auth.ts
git commit -m "feat(auth): add JWT plugin and multi-tenant auth middleware"

# 9
git add packages/api-gateway/src/routes/auth.ts
git commit -m "feat(auth): implement POST /auth/login with bcrypt verification"

# 10
git add packages/api-gateway/src/routes/routes.ts
git commit -m "feat(routes): implement Routes CRUD with enforced tenant isolation"

# 11
git add packages/api-gateway/tests
git commit -m "test(auth): add unit tests for requireTenant and requireRole"

# 12
git add packages/agent-service packages/ingestion
git commit -m "chore(workspace): scaffold agent-service and ingestion placeholders"

# 13
git add docs/adr/001-fastify-over-express.md
git commit -m "docs(adr): add ADR-001 Fastify over Express"

# 14
git add README.md SETUP.md
git commit -m "docs: add README with architecture evolution story and SETUP guide"
```

---

## Troubleshooting 疑難排解

| 症狀 | 可能原因 | 解法 |
|------|---------|------|
| `pnpm install` 卡住 | 防火牆/proxy | 改用 `npmmirror` registry |
| `prisma migrate` 連不上 DB | docker 沒起 | `pnpm db:up` 再試 |
| `JWT_SECRET must be at least 16 chars` | `.env` 沒設或太短 | 跑那段 `node -e ...` 產 32 字元 |
| `Cannot find module '@maas/shared'` | pnpm workspace symlink 沒建好 | 在 root 跑 `pnpm install` 一次 |
| `EADDRINUSE: 0.0.0.0:3000` | port 被佔 | 改 `.env` 的 `API_PORT=3001` |
| Prisma client 型別錯誤 | client 沒生成 | `pnpm --filter api-gateway prisma generate` |

---

## What's next? 下一步

Day 2 預告：
- MongoDB 整合（事件儲存）
- Redis 整合（快取 + rate limiting）
- 改 ingestion package：開始接 mock 交通資料
- 寫 ADR-002（為什麼三資料庫分工）

當你完成 Day 1 並 push 到 GitHub，告訴我 repo URL，我可以幫你檢視 + 進入 Day 2。
