from uuid import UUID

from fastapi import APIRouter, Query, status

from app.core.deps import SubscriptionServiceDep
from app.schemas.common import Page
from app.schemas.subscription import (
    SubscriptionCreate,
    SubscriptionRead,
    SubscriptionTerminate,
)

router = APIRouter(prefix="/subscriptions", tags=["subscriptions"])


@router.get("", response_model=Page[SubscriptionRead])
async def list_subscriptions(
    svc: SubscriptionServiceDep,
    limit: int = Query(default=20, ge=1, le=100),
    offset: int = Query(default=0, ge=0),
) -> Page[SubscriptionRead]:
    items, total = await svc.list_page(limit=limit, offset=offset)
    return Page(
        items=[SubscriptionRead.model_validate(s) for s in items],
        total=total,
        limit=limit,
        offset=offset,
    )


@router.get("/{subscription_id}", response_model=SubscriptionRead)
async def get_subscription(
    subscription_id: UUID, svc: SubscriptionServiceDep
) -> SubscriptionRead:
    sub = await svc.get(subscription_id)
    return SubscriptionRead.model_validate(sub)


@router.post("", response_model=SubscriptionRead, status_code=status.HTTP_201_CREATED)
async def create_subscription(
    payload: SubscriptionCreate, svc: SubscriptionServiceDep
) -> SubscriptionRead:
    sub = await svc.create(payload)
    return SubscriptionRead.model_validate(sub)


@router.post("/{subscription_id}/terminate", response_model=SubscriptionRead)
async def terminate_subscription(
    subscription_id: UUID,
    payload: SubscriptionTerminate,
    svc: SubscriptionServiceDep,
) -> SubscriptionRead:
    sub = await svc.terminate(subscription_id, end_date=payload.end_date)
    return SubscriptionRead.model_validate(sub)
