from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.db.engine import get_session
from app.db.models import Tenant
from app.schemas import TenantCreate, TenantOut

router = APIRouter(prefix="/tenants", tags=["tenants"])


@router.get("", response_model=list[TenantOut])
def list_tenants(session: Session = Depends(get_session)) -> list[Tenant]:
    return list(session.scalars(select(Tenant).order_by(Tenant.created_at)))


@router.post("", response_model=TenantOut, status_code=201)
def create_tenant(payload: TenantCreate, session: Session = Depends(get_session)) -> Tenant:
    exists = session.scalar(select(Tenant).where(Tenant.slug == payload.slug))
    if exists is not None:
        raise HTTPException(status_code=409, detail="slug already exists")
    tenant = Tenant(slug=payload.slug, name=payload.name)
    session.add(tenant)
    session.commit()
    session.refresh(tenant)
    return tenant
