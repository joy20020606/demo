import uuid

from fastapi import APIRouter, Depends, Query
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.api.deps import get_tenant
from app.db.engine import get_session
from app.db.models import ReconciliationBreak, ReconciliationBreakStatus
from app.reconcile import run_reconciliation
from app.schemas import ReconciliationBreakOut

router = APIRouter(tags=["reconciliation"])


@router.post("/reconciliation/run")
def run(
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> dict:
    breaks = run_reconciliation(session, tenant_id)
    return {"breaks": breaks}


@router.get("/reconciliation/breaks", response_model=list[ReconciliationBreakOut])
def list_breaks(
    status: ReconciliationBreakStatus | None = Query(default=None),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[ReconciliationBreak]:
    stmt = select(ReconciliationBreak).where(ReconciliationBreak.tenant_id == tenant_id)
    if status is not None:
        stmt = stmt.where(ReconciliationBreak.status == status)
    stmt = stmt.order_by(ReconciliationBreak.detected_at.desc())
    return list(session.scalars(stmt))
