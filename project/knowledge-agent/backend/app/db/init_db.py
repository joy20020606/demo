"""Idempotent schema bootstrap: pgvector extension + tables + indexes.

Run via:  python -m app.db.init_db
"""

from sqlalchemy import text

from app.db.engine import engine
from app.db.models import Base


def init_db() -> None:
    with engine.begin() as conn:
        conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
    Base.metadata.create_all(engine)
    print("DB initialised: pgvector extension, documents/chunks tables, HNSW + GIN indexes.")


if __name__ == "__main__":
    init_db()
