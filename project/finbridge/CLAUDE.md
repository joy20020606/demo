# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

FinBridge — multi-tenant financial data integration gateway. Ingest, normalize, and serve financial feeds across tenants with numeric-safe modeling. NO AI/ML, NO pgvector, NO Alembic. Polyglot: Python/FastAPI backend (`backend/`), TypeScript/Next.js frontend (`frontend/`). Deploys to Railway (backend + plain Postgres) + Vercel (frontend).

## Commands

Backend (run via Docker — local Python is 3.14, backend targets 3.12):
```bash
docker compose up -d --build                       # full stack (db + backend)
curl.exe http://localhost:8002/health              # -> {"status":"ok","db":true}
docker compose run --rm backend pytest -q          # tests
docker compose exec backend ruff check .           # lint (CI runs this)
docker compose exec backend python -m app.db.init_db  # create tables
```

Frontend (run from `frontend/`):
```bash
npm install && npm run dev        # → http://localhost:3000
npm run lint                      # CI runs this
npm run build                     # CI runs this
```

## Conventions & gotchas

- **Ports** (avoid clashing with sibling demos): db `5433:5432`, backend `8002:8000`. Health URL is `http://localhost:8002/health`.
- DB image is plain `postgres:16` (NOT pgvector). No `CREATE EXTENSION`. `init_db()` only runs `Base.metadata.create_all`.
- DB name: `finbridge`. `DATABASE_URL=postgresql+psycopg://postgres:postgres@db:5432/finbridge` (psycopg3 URL).
- Money/quantity columns ALWAYS `sqlalchemy.Numeric` (Python `Decimal`), never `Float`.
- SQLAlchemy 2.0 `Mapped[]` style; Pydantic v2; `requires-python ">=3.11,<3.13"`.
- ruff: line-length 100, `E501` ignored, target py312, rules `E,F,I,UP,B`. FastAPI `Depends`/`Query`/`Path`/`Body` are in `flake8-bugbear.extend-immutable-calls` so B008 stays clean.
- No explanatory comments (standing rule: no comments unless asked).

## Deployment

- **Backend → Railway** (Dockerfile builder, `backend/railway.json`): `startCommand` wraps `uvicorn ... --port $PORT` in `sh -c`. Railway gives `DATABASE_URL` as `postgresql://…` — rewrite the prefix to `postgresql+psycopg://…`. `healthcheckTimeout` 120s.
- **Frontend → Vercel** (Next.js auto-detect, root `frontend/`): `NEXT_PUBLIC_API_URL` is build-time — redeploy without build cache when changing it.
- **CORS** (`backend/app/config.py::cors_origins`): comma-separated exact origin match. No trailing `/`, use `https://`, pure domain (no path).
