import uuid
from decimal import Decimal

import httpx
from sqlalchemy import select
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.orm import Session

from app.config import get_settings
from app.db.models import (
    Instrument,
    ReconciliationBreak,
    ReconciliationBreakType,
    Tenant,
    Transaction,
    TransactionType,
)

TOLERANCE = Decimal("1e-9")


def _fetch_declared(slug: str) -> list[dict]:
    base_url = get_settings().mock_upstreams_url
    resp = httpx.get(f"{base_url}/positions/declared", params={"tenant": slug}, timeout=10.0)
    resp.raise_for_status()
    return resp.json()


def _aggregate_positions(
    session: Session, tenant_id: uuid.UUID
) -> dict[tuple[str, str], tuple[Decimal, uuid.UUID]]:
    stmt = (
        select(
            Transaction.account_id,
            Instrument.symbol,
            Transaction.instrument_id,
            Transaction.txn_type,
            Transaction.quantity,
        )
        .join(Instrument, Transaction.instrument_id == Instrument.id)
        .where(
            Transaction.tenant_id == tenant_id,
            Transaction.txn_type.in_([TransactionType.buy, TransactionType.sell]),
        )
    )
    aggregated: dict[tuple[str, str], tuple[Decimal, uuid.UUID]] = {}
    for account_id, symbol, instrument_id, txn_type, quantity in session.execute(stmt):
        signed = quantity if txn_type == TransactionType.buy else -quantity
        key = (account_id, symbol)
        net, _ = aggregated.get(key, (Decimal(0), instrument_id))
        aggregated[key] = (net + signed, instrument_id)
    return aggregated


def _upsert_break(
    session: Session,
    tenant_id: uuid.UUID,
    instrument_id: uuid.UUID | None,
    account_id: str,
    symbol: str,
    break_type: ReconciliationBreakType,
    expected: Decimal,
    actual: Decimal,
    delta: Decimal,
) -> None:
    stmt = (
        insert(ReconciliationBreak)
        .values(
            tenant_id=tenant_id,
            instrument_id=instrument_id,
            account_id=account_id,
            symbol=symbol,
            break_type=break_type,
            expected=expected,
            actual=actual,
            delta=delta,
        )
        .on_conflict_do_update(
            index_elements=["tenant_id", "account_id", "symbol", "break_type"],
            set_={
                "instrument_id": instrument_id,
                "expected": expected,
                "actual": actual,
                "delta": delta,
            },
        )
    )
    session.execute(stmt)


def run_reconciliation(session: Session, tenant_id: uuid.UUID) -> int:
    tenant_slug = session.scalar(select(Tenant.slug).where(Tenant.id == tenant_id))
    declared_rows = _fetch_declared(tenant_slug)
    declared: dict[tuple[str, str], Decimal] = {
        (row["account_id"], row["symbol"]): Decimal(str(row["quantity"]))
        for row in declared_rows
    }
    aggregated = _aggregate_positions(session, tenant_id)

    breaks = 0
    for key, expected in declared.items():
        account_id, symbol = key
        if key in aggregated:
            actual, instrument_id = aggregated[key]
            delta = actual - expected
            if abs(delta) > TOLERANCE:
                _upsert_break(
                    session,
                    tenant_id,
                    instrument_id,
                    account_id,
                    symbol,
                    ReconciliationBreakType.qty_mismatch,
                    expected,
                    actual,
                    delta,
                )
                breaks += 1
        else:
            _upsert_break(
                session,
                tenant_id,
                None,
                account_id,
                symbol,
                ReconciliationBreakType.missing_position,
                expected,
                Decimal(0),
                Decimal(0) - expected,
            )
            breaks += 1

    for key, (actual, instrument_id) in aggregated.items():
        if key not in declared:
            account_id, symbol = key
            _upsert_break(
                session,
                tenant_id,
                instrument_id,
                account_id,
                symbol,
                ReconciliationBreakType.extra_position,
                Decimal(0),
                actual,
                actual - Decimal(0),
            )
            breaks += 1

    session.commit()
    return breaks
