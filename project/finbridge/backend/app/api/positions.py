import uuid

from fastapi import APIRouter, Depends, Query
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.api.deps import get_tenant
from app.db.engine import get_session
from app.db.models import Instrument, InstrumentType, Position
from app.schemas import PositionOut

router = APIRouter(prefix="/positions", tags=["positions"])


@router.get("", response_model=list[PositionOut])
def list_positions(
    account_id: str | None = Query(default=None),
    instrument_type: InstrumentType | None = Query(default=None),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[Position]:
    stmt = select(Position).where(Position.tenant_id == tenant_id)
    if account_id is not None:
        stmt = stmt.where(Position.account_id == account_id)
    if instrument_type is not None:
        stmt = stmt.join(Instrument, Position.instrument_id == Instrument.id).where(
            Instrument.instrument_type == instrument_type
        )
    return list(session.scalars(stmt.order_by(Position.account_id)))
