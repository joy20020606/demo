from sqlalchemy import func, select

from app.connectors import bank_xml
from app.db.engine import SessionLocal
from app.db.models import Tenant, Transaction


def _ensure_tenant(session, slug: str, name: str) -> Tenant:
    tenant = session.scalar(select(Tenant).where(Tenant.slug == slug))
    if tenant is None:
        tenant = Tenant(slug=slug, name=name)
        session.add(tenant)
        session.flush()
    return tenant


def _count(session, tenant_id) -> int:
    return session.scalar(
        select(func.count())
        .select_from(Transaction)
        .where(
            Transaction.tenant_id == tenant_id,
            Transaction.source == bank_xml.SOURCE,
        )
    )


def test_connector_run_is_idempotent():
    session = SessionLocal()
    try:
        tenant = _ensure_tenant(session, "acme", "Acme Capital")
        session.commit()

        bank_xml.run(session, tenant.id, "acme")
        session.commit()
        count_after_first = _count(session, tenant.id)
        assert count_after_first > 0

        bank_xml.run(session, tenant.id, "acme")
        session.commit()
        count_after_second = _count(session, tenant.id)

        assert count_after_second == count_after_first
    finally:
        session.close()
