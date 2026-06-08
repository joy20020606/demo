from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy import text

from app.api import instruments, positions, tenants, transactions
from app.config import get_settings
from app.db.engine import engine
from app.db.init_db import init_db

_s = get_settings()


@asynccontextmanager
async def lifespan(app: FastAPI):
    try:
        init_db()
    except Exception as exc:  # noqa: BLE001 — surface but don't crash on cold DB
        print(f"[startup] init_db skipped: {exc}")
    yield


app = FastAPI(title="FinBridge API", version="0.1.0", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=_s.cors_origin_list,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(tenants.router)
app.include_router(instruments.router)
app.include_router(positions.router)
app.include_router(transactions.router)


@app.get("/health")
def health() -> dict:
    db_ok = True
    try:
        with engine.connect() as conn:
            conn.execute(text("SELECT 1"))
    except Exception:  # noqa: BLE001
        db_ok = False
    return {"status": "ok", "db": db_ok}
