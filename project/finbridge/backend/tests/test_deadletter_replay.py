import json
import uuid

from sqlalchemy import func, select

from app.connectors import csv_batch
from app.db.engine import SessionLocal
from app.db.models import DeadLetter, DeadLetterStatus, Tenant, Transaction


def _make_tenant(session) -> Tenant:
    slug = f"dl-test-{uuid.uuid4().hex[:8]}"
    tenant = Tenant(slug=slug, name="Dead Letter Test")
    session.add(tenant)
    session.flush()
    return tenant


def _txn_count(session, tenant_id, source_message_id) -> int:
    return session.scalar(
        select(func.count())
        .select_from(Transaction)
        .where(
            Transaction.tenant_id == tenant_id,
            Transaction.source == csv_batch.SOURCE,
            Transaction.source_message_id == source_message_id,
        )
    )


def test_bad_record_quarantines_then_replays(monkeypatch):
    session = SessionLocal()
    try:
        tenant = _make_tenant(session)
        session.commit()
        monkeypatch.setitem(csv_batch._TENANT_FILES, tenant.slug, "trades_globex.csv")

        result = csv_batch.run(session, tenant.id, tenant.slug)
        session.commit()

        assert result.messages_dead >= 1

        dead_letter = session.scalar(
            select(DeadLetter).where(
                DeadLetter.tenant_id == tenant.id,
                DeadLetter.connector == csv_batch.SOURCE,
                DeadLetter.status == DeadLetterStatus.pending,
            )
        )
        assert dead_letter is not None
        msg_id = dead_letter.source_message_id
        assert _txn_count(session, tenant.id, msg_id) == 0

        row = json.loads(dead_letter.raw_payload)
        row["quantity"] = "5"
        dead_letter.raw_payload = json.dumps(row)
        session.flush()

        from app.resilience.deadletter import replay

        replay(session, dead_letter.id)
        session.commit()

        session.refresh(dead_letter)
        assert dead_letter.status == DeadLetterStatus.replayed
        assert _txn_count(session, tenant.id, msg_id) == 1
    finally:
        session.close()
