"""Idempotent schema bootstrap: tables only.

Run via:  python -m app.db.init_db
"""

from app.db.engine import engine
from app.db.models import Base


def init_db() -> None:
    Base.metadata.create_all(engine)
    print("DB initialised: tenants, instruments, positions, transactions tables.")


if __name__ == "__main__":
    init_db()
