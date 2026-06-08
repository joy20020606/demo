"""Idempotent demo seed.

Run via:  python -m app.seed
Re-running neither duplicates rows nor errors.
"""

import hashlib
from datetime import UTC, datetime
from decimal import Decimal

from sqlalchemy import select
from sqlalchemy.orm import Session

from app.db.engine import SessionLocal
from app.db.models import (
    Instrument,
    InstrumentType,
    Position,
    Tenant,
    Transaction,
    TransactionType,
)

TENANTS = [
    {"slug": "acme", "name": "Acme Capital"},
    {"slug": "globex", "name": "Globex Asset Management"},
]

INSTRUMENTS = [
    {"symbol": "AAPL", "isin": "US0378331005", "instrument_type": InstrumentType.equity, "currency": "USD", "name": "Apple Inc."},
    {"symbol": "VWRL", "isin": "IE00B3RBWM25", "instrument_type": InstrumentType.fund, "currency": "USD", "name": "Vanguard FTSE All-World"},
    {"symbol": "T2030", "isin": "US912810TM02", "instrument_type": InstrumentType.bond, "currency": "USD", "name": "US Treasury 2030"},
    {"symbol": "ESH6", "isin": None, "instrument_type": InstrumentType.derivative, "currency": "USD", "name": "E-mini S&P 500 Mar26"},
]


def _txn_hash(tenant_slug: str, source: str, source_message_id: str) -> str:
    raw = f"{tenant_slug}|{source}|{source_message_id}"
    return hashlib.sha256(raw.encode()).hexdigest()


def _get_or_create_tenant(session: Session, slug: str, name: str) -> tuple[Tenant, bool]:
    tenant = session.scalar(select(Tenant).where(Tenant.slug == slug))
    if tenant is not None:
        return tenant, False
    tenant = Tenant(slug=slug, name=name)
    session.add(tenant)
    session.flush()
    return tenant, True


def _get_or_create_instrument(
    session: Session, tenant_id, data: dict
) -> tuple[Instrument, bool]:
    instrument = session.scalar(
        select(Instrument).where(
            Instrument.tenant_id == tenant_id, Instrument.symbol == data["symbol"]
        )
    )
    if instrument is not None:
        return instrument, False
    instrument = Instrument(tenant_id=tenant_id, **data)
    session.add(instrument)
    session.flush()
    return instrument, True


def _get_or_create_position(
    session: Session, tenant_id, instrument_id, account_id, source, data: dict
) -> tuple[Position, bool]:
    position = session.scalar(
        select(Position).where(
            Position.tenant_id == tenant_id,
            Position.account_id == account_id,
            Position.instrument_id == instrument_id,
            Position.source == source,
        )
    )
    if position is not None:
        return position, False
    position = Position(
        tenant_id=tenant_id,
        instrument_id=instrument_id,
        account_id=account_id,
        source=source,
        **data,
    )
    session.add(position)
    session.flush()
    return position, True


def _get_or_create_transaction(
    session: Session, tenant_id, tenant_slug, source, source_message_id, data: dict
) -> tuple[Transaction, bool]:
    txn = session.scalar(
        select(Transaction).where(
            Transaction.tenant_id == tenant_id,
            Transaction.source == source,
            Transaction.source_message_id == source_message_id,
        )
    )
    if txn is not None:
        return txn, False
    txn = Transaction(
        tenant_id=tenant_id,
        source=source,
        source_message_id=source_message_id,
        content_hash=_txn_hash(tenant_slug, source, source_message_id),
        **data,
    )
    session.add(txn)
    session.flush()
    return txn, True


def seed() -> None:
    as_of = datetime(2026, 6, 1, tzinfo=UTC)
    counts = {
        "tenants": [0, 0],
        "instruments": [0, 0],
        "positions": [0, 0],
        "transactions": [0, 0],
    }

    def tally(key: str, created: bool) -> None:
        counts[key][0 if created else 1] += 1

    session = SessionLocal()
    try:
        for t in TENANTS:
            tenant, created = _get_or_create_tenant(session, t["slug"], t["name"])
            tally("tenants", created)

            instruments: dict[str, Instrument] = {}
            for data in INSTRUMENTS:
                instrument, created = _get_or_create_instrument(session, tenant.id, data)
                tally("instruments", created)
                instruments[data["symbol"]] = instrument

            account_id = f"{t['slug'].upper()}-001"

            position_specs = [
                ("AAPL", Decimal("150"), Decimal("178.42")),
                ("VWRL", Decimal("320"), Decimal("104.10")),
                ("T2030", Decimal("50"), Decimal("98.75")),
            ]
            for symbol, qty, avg_cost in position_specs:
                _, created = _get_or_create_position(
                    session,
                    tenant.id,
                    instruments[symbol].id,
                    account_id,
                    "broker-csv",
                    {
                        "quantity": qty,
                        "avg_cost": avg_cost,
                        "as_of": as_of,
                    },
                )
                tally("positions", created)

            txn_specs = [
                ("AAPL", TransactionType.buy, Decimal("100"), Decimal("175.00"), Decimal("-17500.00")),
                ("AAPL", TransactionType.buy, Decimal("50"), Decimal("185.26"), Decimal("-9263.00")),
                ("VWRL", TransactionType.buy, Decimal("320"), Decimal("104.10"), Decimal("-33312.00")),
                ("AAPL", TransactionType.dividend, Decimal("0"), Decimal("0"), Decimal("96.00")),
                (None, TransactionType.fee, Decimal("0"), Decimal("0"), Decimal("-12.50")),
            ]
            for idx, (symbol, txn_type, qty, price, amount) in enumerate(txn_specs, start=1):
                instrument_id = instruments[symbol].id if symbol is not None else None
                source_message_id = f"{account_id}-MSG-{idx:04d}"
                _, created = _get_or_create_transaction(
                    session,
                    tenant.id,
                    t["slug"],
                    "broker-feed",
                    source_message_id,
                    {
                        "instrument_id": instrument_id,
                        "account_id": account_id,
                        "txn_type": txn_type,
                        "quantity": qty,
                        "price": price,
                        "amount": amount,
                        "currency": "USD",
                        "trade_date": as_of,
                    },
                )
                tally("transactions", created)

        session.commit()
    finally:
        session.close()

    for key, (inserted, skipped) in counts.items():
        print(f"{key}: inserted={inserted} skipped={skipped}")


if __name__ == "__main__":
    seed()
