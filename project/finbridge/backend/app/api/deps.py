import uuid

from fastapi import Depends, Header, HTTPException
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.db.engine import get_session
from app.db.models import Tenant


def get_tenant(
    x_tenant: str | None = Header(default=None, alias="X-Tenant"),
    session: Session = Depends(get_session),
) -> uuid.UUID:
    if not x_tenant:
        raise HTTPException(status_code=404, detail="missing X-Tenant header")
    tenant_id = session.scalar(select(Tenant.id).where(Tenant.slug == x_tenant))
    if tenant_id is None:
        raise HTTPException(status_code=404, detail="unknown tenant")
    return tenant_id
