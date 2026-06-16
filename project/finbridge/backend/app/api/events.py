import uuid

from fastapi import APIRouter, Depends, Query
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.api.deps import get_tenant
from app.db.engine import get_session
from app.db.models import NotificationLog, Outbox, OutboxStatus
from app.schemas import NotificationLogOut, OutboxOut

router = APIRouter(tags=["events"])


@router.get("/events/log", response_model=list[NotificationLogOut])
def list_event_log(
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[NotificationLog]:
    stmt = (
        select(NotificationLog)
        .where(NotificationLog.tenant_id == tenant_id)
        .order_by(NotificationLog.received_at.desc())
    )
    return list(session.scalars(stmt))


@router.get("/outbox", response_model=list[OutboxOut])
def list_outbox(
    status: OutboxStatus | None = Query(default=None),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[Outbox]:
    stmt = select(Outbox).where(Outbox.tenant_id == tenant_id)
    if status is not None:
        stmt = stmt.where(Outbox.status == status)
    stmt = stmt.order_by(Outbox.created_at.desc())
    return list(session.scalars(stmt))
