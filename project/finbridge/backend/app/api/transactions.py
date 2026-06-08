import uuid
from datetime import datetime

from fastapi import APIRouter, Depends, Query
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.api.deps import get_tenant
from app.db.engine import get_session
from app.db.models import Transaction, TransactionType
from app.schemas import TransactionOut

router = APIRouter(prefix="/transactions", tags=["transactions"])


@router.get("", response_model=list[TransactionOut])
def list_transactions(
    txn_type: TransactionType | None = Query(default=None),
    date_from: datetime | None = Query(default=None),
    date_to: datetime | None = Query(default=None),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[Transaction]:
    stmt = select(Transaction).where(Transaction.tenant_id == tenant_id)
    if txn_type is not None:
        stmt = stmt.where(Transaction.txn_type == txn_type)
    if date_from is not None:
        stmt = stmt.where(Transaction.trade_date >= date_from)
    if date_to is not None:
        stmt = stmt.where(Transaction.trade_date <= date_to)
    return list(session.scalars(stmt.order_by(Transaction.trade_date)))
