# SETUP — Step-by-Step

> 從零跑起本專案的手把手指南。預期 20 分鐘內看到 `/health` 回 `{"status":"ok"}`。

---

## 0. Prerequisites 前置檢查

在 PowerShell 或 Windows Terminal 確認版本：

```powershell
node --version    # >= v20.0.0
pnpm --version    # >= 9.0.0
docker --version  # 確認 Docker Desktop 在跑
git --version
```

沒有 pnpm 的話：

```powershell
corepack enable
corepack prepare pnpm@latest --activate
pnpm --version
```

---

## 1. 進入專案

```powershell
cd C:\Users\asus5\Documents\00_demo\project\mini-sme-crm
```

---

## 2. Init git

```powershell
git init
git branch -m main
```

> Conventional Commits 計畫見 §8。

---

## 3. 安裝套件

```powershell
pnpm install
```

這會：
- 解析 `pnpm-workspace.yaml`
- 安裝 root 與所有 `packages/*` 的 deps
- 自動建立 workspace 之間的 symlink（讓 `@sme-crm/shared` 等 import 可直接生效）

常見錯誤：

| 錯誤 | 解法 |
|------|------|
| `ERR_PNPM_PEER_DEP_ISSUES` | 加 `--strict-peer-dependencies=false` |
| `EACCES` 權限問題 | PowerShell 以系統管理員身分執行 |
| 公司 proxy 擋 registry | `pnpm config set registry https://registry.npmmirror.com` |

---

## 4. 環境變數

⚠️ **重要**：每個需要 `.env` 的 package 都在**自己資料夾**放，不是 root。
（Prisma、Fastify 都從各自的 `process.cwd()` 找 `.env`）

```powershell
copy .env.example packages\db\.env
copy .env.example packages\api\.env
copy .env.example packages\web\.env.local
```

打開 `packages\api\.env` 改：
- `JWT_SECRET` 改成隨機 32 字元：
  ```powershell
  node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
  ```
- `ANTHROPIC_API_KEY` 填入你的 key（先沒 key 不影響 health check）

`packages\db\.env` 只需要 `DATABASE_URL`，預設值即可。

---

## 5. 起 PostgreSQL

```powershell
pnpm db:up
docker ps   # 應該看到 sme-crm-postgres
```

要看 log：
```powershell
docker logs -f sme-crm-postgres
```

要停掉：
```powershell
pnpm db:down
```

---

## 6. Migrate + Seed

```powershell
pnpm db:generate
pnpm db:migrate
```

問你 migration 名稱時輸入：`init`

塞範例資料：
```powershell
pnpm db:seed
```

成功會印出：
```
✅ Seed complete
   - 3 customers, 2 deals, 1 contact log
```

---

## 7. 啟動 API

```powershell
pnpm dev:api
```

成功會看到：
```
🚀 api listening on http://0.0.0.0:3001
```

冒煙測試（另一個 PowerShell）：

```powershell
curl http://localhost:3001/health
# → {"status":"ok","service":"mini-sme-crm-api","timestamp":"..."}
```

---

## 8. Conventional Commits 計畫

逐步 commit，模擬真實工程節奏（不要一次大 dump）。

```powershell
# 1. monorepo 骨架
git add package.json pnpm-workspace.yaml tsconfig.base.json .gitignore .env.example
git commit -m "chore: init monorepo with pnpm workspace"

# 2. docker
git add docker-compose.yml
git commit -m "chore(docker): add postgres for local development"

# 3. shared package
git add packages/shared
git commit -m "feat(shared): add Customer/Deal Zod schemas and AppError classes"

# 4. db package
git add packages/db
git commit -m "feat(db): add Prisma schema, client, and seed script"

# 5. ai package
git add packages/ai
git commit -m "feat(ai): add Claude API wrappers for summarize + score-deal"

# 6. api package
git add packages/api
git commit -m "feat(api): initialize Fastify server with health check"

# 7. web package
git add packages/web
git commit -m "feat(web): scaffold Next.js 14 App Router with Tailwind"

# 8. 文件
git add README.md SETUP.md PROGRESS.md
git commit -m "docs: add README, SETUP guide, and progress tracker"
```

---

## Troubleshooting

| 症狀 | 可能原因 | 解法 |
|------|---------|------|
| `pnpm install` 卡住 | proxy / registry 問題 | 換 npmmirror |
| `prisma migrate` 連不上 DB | Docker 沒起 | `pnpm db:up` 再試 |
| `Cannot find module '@sme-crm/shared'` | symlink 未建好 | 在 root 重跑 `pnpm install` |
| `EADDRINUSE: 0.0.0.0:3001` | port 被佔 | 改 `.env` 的 `API_PORT=3002` |
| Prisma client 找不到 | `prisma generate` 沒跑 | `pnpm db:generate` |
| Next.js build 失敗：找不到 `@sme-crm/shared` | `transpilePackages` 漏設 | 確認 `next.config.mjs` 有列 |

---

## 部署 Deploy

### Railway（API + DB）

1. 到 [railway.app](https://railway.app) → New Project → Deploy from GitHub
2. 選這個 repo，root directory 設 `packages/api`
3. **加 PostgreSQL 服務**，Railway 會自動產生 `DATABASE_URL` 環境變數
4. 加環境變數：`ANTHROPIC_API_KEY`、`JWT_SECRET`、`NODE_ENV=production`
5. Build command：`pnpm install && pnpm --filter @sme-crm/api build`
6. Start command：`node dist/server.js`

### Vercel（Web）

1. 到 [vercel.com](https://vercel.com) → Import Project
2. 選同個 repo，**Root Directory** 設 `packages/web`
3. Framework Preset：Next.js（自動偵測）
4. 環境變數：`NEXT_PUBLIC_API_URL=https://<your-railway-api>.railway.app`
5. 部署完成後，每次 push 到 main 自動觸發

### CI（GitHub Actions）

`.github/workflows/ci.yml` 會在每個 PR 跑 `pnpm typecheck` 和 `pnpm build`，
通過後 Vercel + Railway 自動 deploy。

---

## What's next?

完成 Day 0 後，下一步：

- Day 1：把 `/api/customers` CRUD 寫出來，前端能列表
- Day 2：商機 Kanban 拖拉
- Day 3：接 Claude API 做摘要
- Day 4：CI/CD 設好，push 就部署
