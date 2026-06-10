import uuid
from datetime import UTC, datetime

from fastapi import APIRouter, Depends, HTTPException, Path
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.api.deps import get_tenant
from app.connectors import bank_xml, csv_batch, rest_trading
from app.db.engine import get_session
from app.db.models import ConnectorRun, ConnectorRunStatus, Tenant
from app.schemas import ConnectorRunOut

router = APIRouter(tags=["connectors"])

_CONNECTORS = {
    rest_trading.name: rest_trading.run,
    bank_xml.name: bank_xml.run,
    csv_batch.name: csv_batch.run,
}


@router.post("/connectors/{name}/run", response_model=ConnectorRunOut)
def run_connector(
    name: str = Path(...),
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> ConnectorRun:
    runner = _CONNECTORS.get(name)
    if runner is None:
        raise HTTPException(status_code=404, detail="unknown connector")

    tenant_slug = session.scalar(select(Tenant.slug).where(Tenant.id == tenant_id))

    run = ConnectorRun(
        tenant_id=tenant_id,
        connector=name,
        status=ConnectorRunStatus.running,
    )
    session.add(run)
    session.flush()

    try:
        result = runner(session, tenant_id, tenant_slug)
        run.messages_total = result.messages_total
        run.messages_ok = result.messages_ok
        run.messages_dead = result.messages_dead
        if result.messages_dead == 0:
            run.status = ConnectorRunStatus.success
        elif result.messages_ok > 0:
            run.status = ConnectorRunStatus.partial
        else:
            run.status = ConnectorRunStatus.failed
    except Exception as exc:  # noqa: BLE001
        run.status = ConnectorRunStatus.failed
        run.error = str(exc)
        run.finished_at = datetime.now(UTC)
        session.commit()
        session.refresh(run)
        return run

    run.finished_at = datetime.now(UTC)
    session.commit()
    session.refresh(run)
    return run


@router.get("/connector-runs", response_model=list[ConnectorRunOut])
def list_connector_runs(
    tenant_id: uuid.UUID = Depends(get_tenant),
    session: Session = Depends(get_session),
) -> list[ConnectorRun]:
    stmt = (
        select(ConnectorRun)
        .where(ConnectorRun.tenant_id == tenant_id)
        .order_by(ConnectorRun.started_at.desc())
    )
    return list(session.scalars(stmt))
