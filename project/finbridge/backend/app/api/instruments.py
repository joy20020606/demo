import uuid

from fastapi import APIRouter, Depends, Query
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.api.deps import get_tenant
from app.db.engine import get_session
from app.db.models import Instrument, InstrumentType
from app.schemas import InstrumentOut

router = APIRouter(prefix="/instruments", tags=["instruments"])


@router.get("", response_model=list[InstrumentOut])
def list_instruments(
    instrument_type: InstrumentType | None = Query(default=None),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[Instrument]:
    stmt = select(Instrument).where(Instrument.tenant_id == tenant_id)
    if instrument_type is not None:
        stmt = stmt.where(Instrument.instrument_type == instrument_type)
    return list(session.scalars(stmt.order_by(Instrument.symbol)))
