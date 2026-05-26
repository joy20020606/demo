# 進度紀錄 (Progress)

> 最後更新：2026-05-23
> 狀態：Day 2 程式碼生完，**尚未** 安裝套件 / 啟動驗證。

---

## 回來時做什麼

### 一、確認環境
```powershell
cd C:\Users\asus5\Documents\00_demo\project\mini-maas-ops
node --version    # >= v20
pnpm --version
docker --version  # 確認 Docker Desktop 開著
```

### 二、安裝 Day 2 新套件 + 啟動

```powershell
pnpm install                                                  # mongoose / ioredis / pg 等新套件
copy packages\ingestion\.env.example packages\ingestion\.env  # 建 ingestion 的 .env
pnpm db:up                                                    # 起 postgres + mongodb + redis
docker ps                                                     # 應看到 3 個 container
pnpm dev                                                      # 終端機 A：api-gateway
# 另開終端機 B：
pnpm ingest                                                   # ingestion 微服務
```

### 三、驗證 Day 2 通了

```powershell
# 登入拿 token
$res = Invoke-RestMethod -Method Post -Uri http://localhost:3000/auth/login `
  -ContentType "application/json" `
  -Body '{"email":"admin@houston-transit.gov","password":"password123"}'
$token = $res.data.token

# 查事件（讓 ingestion 跑個 30 秒以上再查，才有資料）
Invoke-RestMethod http://localhost:3000/api/events `
  -Headers @{ Authorization = "Bearer $token" }

# 查路線並觀察快取 HIT/MISS
Invoke-RestMethod http://localhost:3000/api/routes -Headers @{ Authorization = "Bearer $token" }
# 第一次：meta.cache = "MISS"
# 60 秒內再查：meta.cache = "HIT"
```

---

## 7 天整體進度

| Day | 主題 | 狀態 |
|-----|------|------|
| 1 | Node.js 骨架 + JWT 多租戶 | ✅ 完成（已跑通） |
| 2 | 三資料庫整合 + ingestion 微服務 | 🔄 程式碼完成，待跑通 |
| 3 | Claude Agent Loop + Tool Use | ⬜ |
| 4 | 微服務拆分 + RabbitMQ | ⬜ |
| 5 | AWS 部署 + CI/CD + 可觀測性 | ⬜ |
| 6 | Next.js 前端 + 地圖視覺化 | ⬜ |
| 7 | Swagger + UML + Demo 影片 | ⬜ |

**側邊任務**：AWS Free Tier 速通（S3 + DynamoDB 各 1 小時，Day 5 前做）

---

## 重要參考文件

| 路徑 | 用途 |
|------|------|
| `README.md` | 專案介紹 + 架構演進故事 |
| `SETUP.md` | 安裝步驟 + commits 計畫 |
| `docs/adr/001-fastify-over-express.md` | 為什麼用 Fastify |
| `docs/adr/002-polyglot-persistence.md` | 為什麼三資料庫分工 |

**個人學習筆記**（在你私人筆記區，不在 repo 內）：
- `DAY1_PLAN.md` / `DAY2_PLAN.md`
- `DAY1_CODE_WALKTHROUGH.md` / `DAY2_CODE_WALKTHROUGH.md`（含面試話術）

---

## 已完成的關鍵成果

- ✅ Fastify + TypeScript + Prisma + PostgreSQL（多租戶 JWT）
- ✅ MongoDB（Mongoose）儲存通勤事件
- ✅ Redis 做 Cache-Aside 快取 + INCR/EXPIRE 限流
- ✅ ingestion 獨立微服務（PG 讀路線 → 寫事件到 Mongo）
- ✅ 5 個 endpoints：`/health`、`/auth/login`、`/api/routes` × 3、`/api/events`
- ✅ 2 個 ADR、1 個單元測試檔
