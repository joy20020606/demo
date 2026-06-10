import uuid

from fastapi import APIRouter, Depends, HTTPException, Path, Query
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.api.deps import get_tenant
from app.db.engine import get_session
from app.db.models import DeadLetter, DeadLetterStatus
from app.resilience.deadletter import replay
from app.schemas import DeadLetterOut

router = APIRouter(tags=["dead-letter"])


@router.get("/dead-letter", response_model=list[DeadLetterOut])
def list_dead_letters(
    status: DeadLetterStatus | None = Query(default=None),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[DeadLetter]:
    stmt = select(DeadLetter).where(DeadLetter.tenant_id == tenant_id)
    if status is not None:
        stmt = stmt.where(DeadLetter.status == status)
    stmt = stmt.order_by(DeadLetter.created_at.desc())
    return list(session.scalars(stmt))


@router.post("/dead-letter/{id}/replay", response_model=DeadLetterOut)
def replay_dead_letter(
    id: uuid.UUID = Path(...),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> DeadLetter:
    dead_letter = session.get(DeadLetter, id)
    if dead_letter is None or dead_letter.tenant_id != tenant_id:
        raise HTTPException(status_code=404, detail="dead letter not found")

    result = replay(session, id)
    session.commit()
    session.refresh(result)
    return result
