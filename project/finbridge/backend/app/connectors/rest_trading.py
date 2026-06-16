import json
import uuid

import httpx
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.config import get_settings
from app.connectors.base import ConnectorResult, upsert_transaction
from app.db.models import Tenant
from app.normalize import normalize_trade
from app.resilience.deadletter import quarantine
from app.resilience.retry import fetch_retry

name = "trading_rest"
SOURCE = "trading_rest"


@fetch_retry
def _fetch(slug: str) -> list[dict]:
    base_url = get_settings().mock_upstreams_url
    resp = httpx.get(f"{base_url}/trades", params={"tenant": slug}, timeout=10.0)
    resp.raise_for_status()
    return resp.json()


def run(session: Session, tenant_id: uuid.UUID, tenant_slug: str) -> ConnectorResult:
    if not tenant_slug:
        tenant_slug = session.scalar(select(Tenant.slug).where(Tenant.id == tenant_id))
    trades = _fetch(tenant_slug)
    total = len(trades)
    ok = 0
    dead = 0
    for row in trades:
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
