import hashlib
import uuid
from dataclasses import dataclass
from typing import Protocol

from sqlalchemy import select
from sqlalchemy.dialects.postgresql import insert
from sqlalchemy.orm import Session

from app.db.models import Instrument, InstrumentType, Transaction
from app.normalize import CanonicalTxn


@dataclass
class ConnectorResult:
    messages_total: int
    messages_ok: int
    messages_dead: int


class Connector(Protocol):
    name: str

    def run(self, session: Session, tenant_id: uuid.UUID, tenant_slug: str) -> ConnectorResult: ...


def content_hash(tenant_slug: str, source: str, source_message_id: str) -> str:
    raw = f"{tenant_slug}|{source}|{source_message_id}"
    return hashlib.sha256(raw.encode()).hexdigest()


def _infer_instrument_type(symbol: str) -> InstrumentType:
    return InstrumentType.equity


def get_or_create_instrument(
    session: Session, tenant_id: uuid.UUID, symbol: str, currency: str
) -> Instrument:
    instrument = session.scalar(
        select(Instrument).where(
            Instrument.tenant_id == tenant_id, Instrument.symbol == symbol
        )
    )
    if instrument is not None:
        return instrument
    instrument = Instrument(
        tenant_id=tenant_id,
        symbol=symbol,
        isin=None,
        instrument_type=_infer_instrument_type(symbol),
        currency=currency,
        name=symbol,
    )
    session.add(instrument)
    session.flush()
    return instrument


def upsert_transaction(
    session: Session,
    tenant_id: uuid.UUID,
    tenant_slug: str,
    source: str,
    txn: CanonicalTxn,
) -> bool:
    instrument_id = None
    if txn.symbol is not None:
        instrument = get_or_create_instrument(
            session, tenant_id, txn.symbol, txn.currency
        )
        instrument_id = instrument.id

    stmt = (
        insert(Transaction)
        .values(
            tenant_id=tenant_id,
            instrument_id=instrument_id,
            account_id=txn.account_id,
            txn_type=txn.txn_type,
            quantity=txn.quantity,
            price=txn.price,
            amount=txn.amount,
            currency=txn.currency,
            trade_date=txn.trade_date,
            source=source,
            source_message_id=txn.source_message_id,
            content_hash=content_hash(tenant_slug, source, txn.source_message_id),
        )
        .on_conflict_do_nothing(
            index_elements=["tenant_id", "source", "source_message_id"]
        )
        .returning(Transaction.id)
    )
    inserted_id = session.execute(stmt).scalar_one_or_none()
    return inserted_id is not None
