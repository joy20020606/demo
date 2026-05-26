# Mini MaaS Ops — Agentic AI Platform for Mobility Operations

> v2 — Architecture Evolution from `mini-amazon-ads-ai`
> v2 — 從 mini-amazon-ads-ai 演進而來的架構升級

A multi-tenant SaaS that helps city transit authorities and corporate commute managers run AI-driven incentive programs to shape commuter behavior — promoting carpooling, off-peak travel, and route diversification.

多租戶 SaaS 平台，協助城市交通局與企業通勤管理員，透過 AI 自主分析、給出行為引導建議（鼓勵共乘、錯峰出勤、改道採用）。

---

## Architecture Evolution Story 架構演進故事

| 維度 | v1 (`mini-amazon-ads-ai`) | v2 (`mini-maas-ops`) |
|------|--------------------------|----------------------|
| Domain | E-commerce ads optimization | MaaS operations |
| Backend | FastAPI (Python) | **Fastify (Node.js + TypeScript)** |
| ORM | SQLAlchemy | **Prisma** |
| Storage | PostgreSQL + ClickHouse | PostgreSQL + MongoDB + Redis + ClickHouse |
| Architecture | Modular monolith | **Microservices (monorepo)** |
| Messaging | N/A | **RabbitMQ** (event-driven) |
| Cloud | Vercel + Railway | + **AWS S3 / DynamoDB** |
| AI Pattern | Tool Use Loop (same) | Tool Use Loop + Multi-agent orchestration |
| Frontend | Next.js + Recharts | Next.js + Recharts + **react-leaflet (maps)** |

**保留核心**：Agent Loop、Tool Use、Human-in-the-loop、多租戶 JWT、混合 OLTP/OLAP。
**升級重點**：語言、ORM、微服務、訊息佇列、AWS 整合。

---

## Quick Start 快速啟動

```bash
# 1. Install dependencies
pnpm install

# 2. Start PostgreSQL (Docker)
pnpm db:up

# 3. Run migrations
pnpm db:migrate

# 4. Seed sample data (1 tenant, 1 admin user, 3 routes)
pnpm db:seed

# 5. Run the API
pnpm dev

# 6. Smoke test
curl http://localhost:3000/health
# → {"status":"ok"}
```

完整步驟請看 [SETUP.md](./SETUP.md)。

---

## Monorepo Layout

```
mini-maas-ops/
├── packages/
│   ├── api-gateway/      # Fastify + JWT + Prisma (Day 1)
│   ├── agent-service/    # Claude Agent Loop (Day 3)
│   ├── ingestion/        # Traffic data ingestion (Day 2)
│   └── shared/           # Common types, schemas, errors
└── docs/
    └── adr/              # Architecture Decision Records
```

---

## Documentation

- [SETUP.md](./SETUP.md) — 手把手安裝指南
- [ADR-001: Fastify over Express](./docs/adr/001-fastify-over-express.md)

---

## License

MIT
