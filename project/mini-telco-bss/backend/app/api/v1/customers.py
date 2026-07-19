from uuid import UUID

from fastapi import APIRouter, Query, status

from app.core.deps import CustomerServiceDep
from app.core.enums import CustomerStatus, CustomerType
from app.schemas.common import Page
from app.schemas.customer import (
    CustomerCreate,
    CustomerRead,
    CustomerSearchFilters,
    CustomerUpdate,
)

router = APIRouter(prefix="/customers", tags=["customers"])


@router.get("", response_model=Page[CustomerRead])
async def list_customers(
    svc: CustomerServiceDep,
    status_: CustomerStatus | None = Query(default=None, alias="status"),
    customer_type: CustomerType | None = None,
    q: str | None = Query(default=None, description="name / code / email substring"),
    limit: int = Query(default=20, ge=1, le=100),
    offset: int = Query(default=0, ge=0),
) -> Page[CustomerRead]:
    filters = CustomerSearchFilters(status=status_, customer_type=customer_type, q=q)
    items, total = await svc.search(filters, limit=limit, offset=offset)
    return Page(
        items=[CustomerRead.model_validate(c) for c in items],
        total=total,
        limit=limit,
        offset=offset,
    )


@router.get("/{customer_id}", response_model=CustomerRead)
async def get_customer(customer_id: UUID, svc: CustomerServiceDep) -> CustomerRead:
    customer = await svc.get(customer_id)
    return CustomerRead.model_validate(customer)


@router.post("", response_model=CustomerRead, status_code=status.HTTP_201_CREATED)
async def create_customer(
    payload: CustomerCreate, svc: CustomerServiceDep
) -> CustomerRead:
    customer = await svc.create(payload)
    return CustomerRead.model_validate(customer)


@router.patch("/{customer_id}", response_model=CustomerRead)
async def update_customer(
    customer_id: UUID, payload: CustomerUpdate, svc: CustomerServiceDep
) -> CustomerRead:
    customer = await svc.update(customer_id, payload)
    return CustomerRead.model_validate(customer)
