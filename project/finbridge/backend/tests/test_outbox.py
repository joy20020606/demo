import uuid
from datetime import datetime
from decimal import Decimal

from sqlalchemy import func, select

from app.config import get_settings
from app.connectors.base import upsert_transaction
from app.db.engine import SessionLocal
from app.db.models import Outbox, OutboxStatus, Tenant, Transaction, TransactionType
from app.normalize import CanonicalTxn

SOURCE = "test_outbox"


def _new_tenant(session) -> Tenant:
    slug = f"outbox-{uuid.uuid4().hex[:12]}"
    tenant = Tenant(slug=slug, name="Outbox Test")
    session.add(tenant)
    session.flush()
    return tenant


def _make_txns(prefix: str) -> list[CanonicalTxn]:
    return [
        CanonicalTxn(
            source_message_id=f"{prefix}-{i}",
            account_id="ACCT-1",
            symbol="AAPL",
            txn_type=TransactionType.buy,
            quantity=Decimal("10"),
            price=Decimal("150.25"),
            amount=Decimal("1502.50"),
            currency="USD",
            trade_date=datetime(2026, 1, i + 1),
        )
        for i in range(3)
    ]


def _ingest(session, tenant) -> int:
    inserted = 0
    for txn in _make_txns(tenant.slug):
        if upsert_transaction(session, tenant.id, tenant.slug, SOURCE, txn):
            inserted += 1
    return inserted


def _count_txns(session, tenant_id) -> int:
    return session.scalar(
        select(func.count())
        .select_from(Transaction)
        .where(Transaction.tenant_id == tenant_id, Transaction.source == SOURCE)
    )


def _count_outbox(session, tenant_id) -> int:
    return session.scalar(
        select(func.count()).select_from(Outbox).where(Outbox.tenant_id == tenant_id)
    )


def test_new_transactions_create_pending_outbox_rows():
    assert get_settings().kafka_enabled is False
    session = SessionLocal()
    try:
        tenant = _new_tenant(session)

        new_count = _ingest(session, tenant)
        session.commit()

        assert new_count > 0
        assert _count_txns(session, tenant.id) == new_count
        assert _count_outbox(session, tenant.id) == new_count

        pending = session.scalar(
            select(func.count())
            .select_from(Outbox)
            .where(
                Outbox.tenant_id == tenant.id,
                Outbox.status == OutboxStatus.pending,
            )
        )
        assert pending == new_count
    finally:
        session.rollback()
        session.close()


def test_idempotent_rerun_adds_no_new_outbox_rows():
    assert get_settings().kafka_enabled is False
    session = SessionLocal()
    try:
        tenant = _new_tenant(session)

        first = _ingest(session, tenant)
        session.commit()
        outbox_after_first = _count_outbox(session, tenant.id)
        assert first > 0
        assert outbox_after_first == first

        second = _ingest(session, tenant)
        session.commit()
        outbox_after_second = _count_outbox(session, tenant.id)

        assert second == 0
        assert outbox_after_second == outbox_after_first
    finally:
        session.rollback()
        session.close()
