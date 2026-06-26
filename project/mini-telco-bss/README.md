# mini-telco-bss

A production-grade **Telco BSS (Business Support System) Lite** — customer, plan, subscription, usage, billing, notifications. Built to showcase **clean layered architecture and classic OO design patterns in Python**, the same way they are practiced in mature .NET / C# codebases.

> Target role: Python Full-Stack Engineer @ a Telco-grade IT team.
> Why this stack: most "FastAPI demos" jam business logic into endpoints. This one separates concerns the way EF Core + ASP.NET Core would — Repository, Unit of Work, Service Layer, DI — so a senior reviewer can read the codebase top-down in 10 minutes.

---

## Highlights

- **9 GoF / DDD-ish patterns** wired into real features (see [Design Patterns Index](#design-patterns-index))
- **Async end-to-end**: FastAPI + SQLAlchemy 2.0 async + asyncpg
- **Production-grade extras** that telcos actually care about: RBAC, audit log, rate limiting, API versioning, Prometheus `/metrics`, health checks
- **Real third-party integration shape**: HMAC-signed webhook receiver with idempotency keys
- **End-to-end testability**: pytest + httpx + testcontainers (real Postgres, not mocks at boundaries)
- **Deployed**: Railway (backend + Postgres) + Vercel (frontend)

## Tech Stack

| Layer | Choice | Why (vs. the obvious alternative) |
|---|---|---|
| Language | Python 3.12 | Modern type hints, `Mapped[str]` works cleanly |
| API | FastAPI 0.115+ | Pydantic v2 + auto OpenAPI; closest feel to ASP.NET Core minimal APIs |
| ORM | SQLAlchemy 2.0 (async) | Mature, transaction control matches EF Core's `DbContext.SaveChanges` |
| Migrations | Alembic | The Python `dotnet ef migrations` |
| DB | PostgreSQL 16 | Telco-grade, JSONB for flexible plan rules |
| Auth | JWT (python-jose) + bcrypt | Stateless, scales horizontally |
| Frontend | React 19 + Vite + TypeScript + Tailwind + TanStack Query + Recharts | Telco job lists React/Angular/Vue equally; React = fastest to ship a real-looking dashboard |
| Tests | pytest + pytest-asyncio + httpx + testcontainers-postgres | Real DB in CI, not mocks (avoids "passes-in-CI-breaks-in-prod") |
| Deploy | Railway (BE+DB) + Vercel (FE) | Single-command deploys; same flow author uses for all demos |

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│  React 19 + TanStack Query  ──HTTPS──►  FastAPI (/api/v1/*)      │
└──────────────────────────────────────────────────────────────────┘
                                            │
                  ┌─────────────────────────┼──────────────────────┐
                  │                         │                      │
              Routers                  Middlewares           Background
           (api/v1/*.py)         (auth, audit, rate-limit)    (webhooks)
                  │
                  ▼
            ┌──────────────┐
            │  Service     │  ← business logic, orchestrates UoW
            └──────┬───────┘
                   │
            ┌──────▼───────┐
            │  UnitOfWork  │  ← transaction boundary (begin/commit/rollback)
            └──────┬───────┘
                   │
            ┌──────▼───────┐
            │  Repository  │  ← SQLAlchemy queries, hides ORM from services
            └──────┬───────┘
                   │
              PostgreSQL
```

---

## Design Patterns Index

> Every pattern below maps to a real file. Open the link, read 30 lines, see how it works.

| # | Pattern | C# / .NET analog | Where it lives | Why used here |
|---|---|---|---|---|
| 1 | **Repository** | `IUserRepository` + EF `DbSet` | _to be filled_ | Hide SQLAlchemy from services; mockable in unit tests |
| 2 | **Unit of Work** | `DbContext.SaveChanges()` | _to be filled_ | Wrap multi-table writes (subscribe → bill → notify) in one transaction |
| 3 | **Service Layer** | `UserService : IUserService` | _to be filled_ | Business rules live here, not in routers |
| 4 | **Dependency Injection** | `builder.Services.AddScoped<>` | _to be filled_ | FastAPI `Depends`; everything constructor-injected |
| 5 | **DTO / Pydantic Schemas** | `record UserDto` + `[Required]` | _to be filled_ | Request/response contracts decoupled from ORM models |
| 6 | **Strategy** | `IPricingStrategy` | _to be filled_ | Three billing models (monthly / prepaid / unlimited) swap behind one interface |
| 7 | **Factory** | `INotifierFactory` | _to be filled_ | Pick SMS vs Email at runtime by channel string |
| 8 | **Decorator** | C# attributes + Castle DynamicProxy | _to be filled_ | Python native: `@retry`, `@audit_log`, `@cache` — *cleaner than the .NET equivalent* |
| 9 | **Specification** | `ISpecification<Customer>` | _to be filled_ | Composable WHERE clauses for advanced search |

---

## Getting Started

```bash
# 1. Backend (Postgres + FastAPI)
docker compose up -d db
cd backend
pip install -e ".[dev]"
alembic upgrade head
python scripts/seed.py
uvicorn app.main:app --reload

# 2. Frontend
cd frontend
pnpm install
pnpm dev

# 3. Open
# Backend docs:  http://localhost:8000/docs
# Frontend:      http://localhost:5173
```

Detailed setup: [docs/getting-started.md](docs/getting-started.md) (TBD)

---

## Project Structure

```
mini-telco-bss/
├── backend/
│   ├── app/
│   │   ├── api/v1/           # FastAPI routers
│   │   ├── core/             # config, security, deps
│   │   ├── db/               # SQLAlchemy session, base
│   │   ├── models/           # ORM models
│   │   ├── schemas/          # Pydantic DTOs
│   │   ├── repositories/     # Repository pattern
│   │   ├── services/         # Service layer + UoW
│   │   ├── patterns/         # Strategy / Factory / Decorator
│   │   └── main.py
│   ├── alembic/              # Migrations
│   ├── tests/                # pytest
│   └── scripts/              # seed, ops helpers
├── frontend/                 # React + Vite + TS
├── docs/                     # design notes, screenshots
├── infra/                    # Railway / deploy configs
└── docker-compose.yml
```

---

## Production-Grade Extras (the "what would you add for prod" answer)

- `GET /health` — DB + cache liveness
- `GET /metrics` — Prometheus exposition
- `/api/v1/*` — versioned from day one; old clients won't break on v2 cutover
- **RBAC**: `admin` / `agent` / `readonly` roles enforced via FastAPI deps
- **Audit log**: every write op recorded (who, what, when) — telco compliance baseline
- **Rate limiting**: `slowapi` per-IP per-route
- **Webhook security**: HMAC-SHA256 signature + replay-protection via idempotency key

---

## Screenshots

_Filled in Phase 9._

---

## License

MIT
