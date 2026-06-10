import csv
import json
import uuid
from pathlib import Path

from sqlalchemy import select
from sqlalchemy.orm import Session

from app.connectors.base import ConnectorResult, upsert_transaction
from app.db.models import Tenant
from app.logging_config import get_logger
from app.normalize import normalize_trade
from app.resilience.deadletter import quarantine

name = "csv_batch"
SOURCE = "csv_batch"
DATA_DIR = Path(__file__).resolve().parent.parent / "mock_upstreams" / "data" / "csv_batch"
_TENANT_FILES = {
    "globex": "trades_globex.csv",
}

logger = get_logger("finbridge.connector.csv_batch")


def _fetch(filename: str) -> list[dict]:
    with (DATA_DIR / filename).open(newline="") as fh:
        return list(csv.DictReader(fh))


def run(session: Session, tenant_id: uuid.UUID, tenant_slug: str) -> ConnectorResult:
    if not tenant_slug:
        tenant_slug = session.scalar(select(Tenant.slug).where(Tenant.id == tenant_id))
    filename = _TENANT_FILES.get(tenant_slug)
    if filename is None:
        return ConnectorResult(messages_total=0, messages_ok=0, messages_dead=0)
    rows = _fetch(filename)
    total = len(rows)
    ok = 0
    dead = 0
    for row in rows:
        try:
            txn = normalize_trade(row)
            upsert_transaction(session, tenant_id, tenant_slug, SOURCE, txn)
            ok += 1
        except Exception as exc:  # noqa: BLE001
            quarantine(
                session,
                tenant_id,
                SOURCE,
                row.get("message_id"),
                json.dumps(row),
                exc,
            )
            dead += 1
    return ConnectorResult(messages_total=total, messages_ok=ok, messages_dead=dead)
