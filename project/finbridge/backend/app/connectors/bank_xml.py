import uuid
from pathlib import Path

from lxml import etree
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.connectors.base import ConnectorResult, upsert_transaction
from app.db.models import Tenant
from app.normalize import CAMT_NS, normalize_camt_entry
from app.resilience.deadletter import quarantine

name = "bank_xml"
SOURCE = "bank_xml"
DATA_DIR = Path(__file__).resolve().parent.parent / "mock_upstreams" / "data" / "bank_xml"
_TENANT_STATEMENTS = {
    "acme": ("camt053_acme.xml", "ACME-BANK-01"),
}


def run(session: Session, tenant_id: uuid.UUID, tenant_slug: str) -> ConnectorResult:
    if not tenant_slug:
        tenant_slug = session.scalar(select(Tenant.slug).where(Tenant.id == tenant_id))
    statement = _TENANT_STATEMENTS.get(tenant_slug)
    if statement is None:
        return ConnectorResult(messages_total=0, messages_ok=0, messages_dead=0)
    filename, account_default = statement
    root = etree.fromstring((DATA_DIR / filename).read_bytes())
    ntries = root.findall(".//c:Stmt/c:Ntry", CAMT_NS)
    total = len(ntries)
    ok = 0
    dead = 0
    for ntry in ntries:
        ref = ntry.findtext("c:NtryRef", namespaces=CAMT_NS)
        try:
            txn = normalize_camt_entry(ntry, account_default=account_default)
            upsert_transaction(session, tenant_id, tenant_slug, SOURCE, txn)
            ok += 1
        except Exception as exc:  # noqa: BLE001
            quarantine(
                session,
                tenant_id,
                SOURCE,
                ref,
                etree.tostring(ntry, encoding="unicode"),
                exc,
            )
            dead += 1
    return ConnectorResult(messages_total=total, messages_ok=ok, messages_dead=dead)
