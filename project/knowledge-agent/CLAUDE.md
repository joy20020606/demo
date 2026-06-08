# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

RAG + LangGraph academic-document Q&A system. Upload papers/books → semantic chunking → hybrid retrieval (pgvector + tsvector) + rerank → LangGraph agent answers with `[n]` citations → RAGAS evaluation dashboard. Polyglot: Python/FastAPI backend (`backend/`), TypeScript/Next.js frontend (`frontend/`). Deploys to Railway (backend + Postgres/pgvector) + Vercel (frontend).

## Commands

Backend (run from `backend/`):
```bash
pip install -e ".[dev,eval]"      # add ,rerank for local cross-encoder (pulls torch, +1-2GB)
python -m app.db.init_db          # creates tables, enables pgvector, builds indexes
uvicorn app.main:app --reload     # → http://localhost:8000  (/docs for OpenAPI)
ruff check .                      # lint (CI runs this)
pytest -q                         # all tests
pytest tests/test_rrf.py -q       # single file
python -m app.eval.run_ragas      # RAGAS eval across 4 configs → app/eval/results.json
```

Frontend (run from `frontend/`):
```bash
npm install && npm run dev        # → http://localhost:3000
npm run lint                      # CI runs this
npm run build                     # CI runs this
```

Full stack via Docker: `docker compose up --build` (backend + Postgres). Needs `.env` (copy `.env.example`) with `OPENAI_API_KEY` (embeddings) + `ANTHROPIC_API_KEY` (generation).

## Architecture

**Two-provider LLM split** (`backend/app/llm.py`, `config.py`): OpenAI `text-embedding-3-small` (1536-dim) for embeddings, Claude for generation/agent. Claude has no embedding API — this is deliberate.

**Retrieval pipeline** — single entry point `retrieval/retriever.py::retrieve()`, called by the agent's tool:
```
expand (HyDE + multi-query, query_expand.py)
  → hybrid_search (vectorstore.py ⊕ keyword.py) OR vector_search
  → RRF fusion (hybrid.py)
  → dedupe + similarity_threshold gate
  → rerank (cross-encoder, rerank.py) OR sort by rrf_score/vector_score
  → top final_k
```
RRF is hand-rolled to fuse cosine and `ts_rank_cd` (different scales) by rank, not score. Rerank defaults OFF (`use_rerank`) — it's the "precision↔cost" toggle the eval dashboard exists to quantify.

**Agent** — hand-built LangGraph `StateGraph` (`agent/graph.py`), NOT `create_react_agent` (deprecated). Control flow: `retrieve → generate → validate_citations → (relax + retry once if ungrounded)`. The `relax_node` drops the similarity threshold to 0 and widens `final_k` for the single retry. `agent/citations.py` validates `[n]` markers against retrieved blocks and **strips fabricated citations** — the anti-hallucination guardrail. `agent/tools.py` exposes retrieve/list_sources/lookup_author as both plain callables (driving the graph) and `@tool` LangChain wrappers.

**Database** (`db/models.py`) — Postgres + pgvector. `Chunk.embedding` has an HNSW index (`vector_cosine_ops`, m=16, ef_construction=64); `Chunk.ts` is a **generated** `tsvector` column (`to_tsvector('english', content)`) with a GIN index — serves the BM25-equivalent keyword role with zero extra infra. `Document.content_hash` is SHA256 for upload dedup (ingest returns 409 on duplicate).

**Config flows downward**: `Settings` (`config.py`, env-backed) → defaults for `RagConfig` (`schemas.py`, per-request, overridable live from the frontend `RagConfigPanel`) → threaded through the whole retrieval/agent path. `RetrievedBlock` carries per-stage provenance scores (vector/keyword/rrf/rerank) for the frontend `RetrievalInspector`.

**API** (`api/`): `ingest.py` (upload + chunk + embed), `chat.py` (runs agent), `eval.py` (RAGAS results). Registered in `main.py`; `/health` reports DB + model.

## Conventions & gotchas

- `chunk_method`: `fixed | semantic | propositional` (`ingestion/chunker.py`).
- LLM-extracted fields (`theory_tag`) use `TEXT`, not `VARCHAR(N)` — model output has no length bound and will overflow fixed widths.
- `DATABASE_URL` must be a `postgresql+psycopg://…` (psycopg3) SQLAlchemy URL.
- ruff: line-length 100, `E501` ignored, target py312; rules `E,F,I,UP,B`.
- `init_db()` runs at startup (`lifespan`) but is wrapped to not crash on a cold DB.

## Deployment

- **Backend → Railway** (Dockerfile builder, `backend/railway.json`):
  - `startCommand` MUST wrap `uvicorn ... --port $PORT` in `sh -c`. Exec-form CMD doesn't expand `$PORT` → `"$PORT" is not a valid integer` at boot.
  - Railway gives `DATABASE_URL` as `postgresql://…` — manually rewrite the prefix to `postgresql+psycopg://…`.
  - Postgres needs `CREATE EXTENSION vector;` once via the Data SQL editor before backend deploy.
  - `healthcheckTimeout`: 120s — cold-start (uvicorn + ragas/langchain import + `init_db`) can blow past 30s.
- **Frontend → Vercel** (Next.js auto-detect, root `frontend/`):
  - `NEXT_PUBLIC_API_URL` is **build-time** — when changing it via the dashboard, redeploy with "Use existing Build Cache" **unchecked**, otherwise the old value stays baked in.
- **CORS** (`backend/app/config.py::cors_origins`): comma-separated, exact origin match.
  - ✅ `https://demo-z1s7.vercel.app`
  - ❌ trailing `/`, `http://` instead of `https://`, or with path.
- **Live**: https://demo-z1s7.vercel.app · https://backend-production-6049.up.railway.app/docs

## Debugging

- **Container picked up stale env after editing `.env`** → `docker compose up -d` (recreates), not `docker compose restart` (reuses existing env baked at create time).
- **Shell env overrides `.env`** → docker compose precedence is shell > .env. Unset the conflicting var: `env -u VAR_NAME docker compose up -d`.
- **RAGAS import error `ModuleNotFoundError: langchain_community.chat_models.vertexai`** → pin `langchain-community<0.3.20` (newer versions removed `ChatVertexAI` from there, ragas 0.2.x still imports it).
- **`value too long for character varying(N)` on ingest** → an LLM-extracted field hit a column cap; widen to `TEXT` and add a length cap in `ingestion/pipeline.py::_extract_metadata`.
- **Frontend `TypeError: Failed to fetch`** → check Network tab for the request URL. If it's `localhost:8001/...` in production, `NEXT_PUBLIC_API_URL` wasn't baked in (rebuild without cache). If it's the right URL but blocked, CORS_ORIGINS doesn't match (check trailing slash / preview-deploy subdomain).
