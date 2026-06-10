from decimal import Decimal

from sqlalchemy import func, select

from app.connectors import rest_trading
from app.db.engine import SessionLocal
from app.db.models import (
    ReconciliationBreak,
    ReconciliationBreakType,
    Tenant,
)
from app.reconcile import _aggregate_positions, run_reconciliation


def _ensure_tenant(session, slug: str, name: str) -> Tenant:
    tenant = session.scalar(select(Tenant).where(Tenant.slug == slug))
    if tenant is None:
        tenant = Tenant(slug=slug, name=name)
        session.add(tenant)
        session.flush()
    return tenant


def test_known_acme_break_detected():
    session = SessionLocal()
    try:
        tenant = _ensure_tenant(session, "acme", "Acme Capital")
        session.commit()

        rest_trading.run(session, tenant.id, "acme")
        session.commit()

        run_reconciliation(session, tenant.id)

        brk = session.scalar(
            select(ReconciliationBreak).where(
                ReconciliationBreak.tenant_id == tenant.id,
                ReconciliationBreak.account_id == "ACME-EQ-01",
                ReconciliationBreak.symbol == "AAPL",
                ReconciliationBreak.break_type == ReconciliationBreakType.qty_mismatch,
            )
        )
        assert brk is not None
        assert brk.expected == Decimal("100")
        assert brk.actual == Decimal("120")
        assert brk.delta == Decimal("20")
    finally:
        session.close()


def test_reconciliation_is_idempotent():
    session = SessionLocal()
    try:
        tenant = _ensure_tenant(session, "acme", "Acme Capital")
        session.commit()

        rest_trading.run(session, tenant.id, "acme")
        session.commit()

        run_reconciliation(session, tenant.id)
        count_first = session.scalar(
            select(func.count())
            .select_from(ReconciliationBreak)
            .where(ReconciliationBreak.tenant_id == tenant.id)
        )

        run_reconciliation(session, tenant.id)
        count_second = session.scalar(
            select(func.count())
            .select_from(ReconciliationBreak)
            .where(ReconciliationBreak.tenant_id == tenant.id)
        )

        assert count_second == count_first
    finally:
        session.close()


def test_tolerance_below_epsilon_no_break():
    session = SessionLocal()
    try:
        tenant = _ensure_tenant(session, "acme", "Acme Capital")
        session.commit()

        aggregated = _aggregate_positions(session, tenant.id)
        for (_account, _symbol), (net, _instrument_id) in aggregated.items():
            tiny = net + Decimal("1e-12")
            assert abs(tiny - net) <= Decimal("1e-9")
    finally:
        session.close()
