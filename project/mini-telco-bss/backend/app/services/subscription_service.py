from collections.abc import Sequence
from datetime import date
from uuid import UUID

from fastapi import HTTPException, status

from app.core.enums import CustomerStatus, SubscriptionStatus
from app.models.subscription import Subscription
from app.schemas.subscription import SubscriptionCreate
from app.services.uow import AsyncUnitOfWork


class SubscriptionService:
    def __init__(self, uow: AsyncUnitOfWork) -> None:
        self.uow = uow

    async def get(self, subscription_id: UUID) -> Subscription:
        sub = await self.uow.subscriptions.get_with_relations(subscription_id)
        if sub is None:
            raise HTTPException(status.HTTP_404_NOT_FOUND, "Subscription not found")
        return sub

    async def list_page(
        self, limit: int, offset: int
    ) -> tuple[Sequence[Subscription], int]:
        return await self.uow.subscriptions.list_page(limit=limit, offset=offset)

    async def create(self, payload: SubscriptionCreate) -> Subscription:
        customer = await self.uow.customers.get(payload.customer_id)
        if customer is None:
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "Customer not found")
        if customer.status != CustomerStatus.ACTIVE:
            raise HTTPException(
                status.HTTP_400_BAD_REQUEST,
                f"Customer status is {customer.status.value}, cannot create subscription",
            )

        plan = await self.uow.plans.get(payload.plan_id)
        if plan is None:
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "Plan not found")
        if not plan.is_active:
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "Plan is not active")

        conflicting = await self.uow.subscriptions.get_by_phone(payload.phone_number)
        if conflicting is not None:
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                f"Phone number {payload.phone_number} already used",
            )

        sub = Subscription(
            customer_id=payload.customer_id,
            plan_id=payload.plan_id,
            phone_number=payload.phone_number,
            start_date=payload.start_date,
            status=SubscriptionStatus.ACTIVE,
        )
        await self.uow.subscriptions.add(sub)
        await self.uow.commit()
        return sub

    async def terminate(
        self, subscription_id: UUID, end_date: date | None = None
    ) -> Subscription:
        sub = await self.uow.subscriptions.get(subscription_id)
        if sub is None:
            raise HTTPException(status.HTTP_404_NOT_FOUND, "Subscription not found")
        if sub.status == SubscriptionStatus.TERMINATED:
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "Already terminated")

        sub.status = SubscriptionStatus.TERMINATED
        sub.end_date = end_date or date.today()
        await self.uow.commit()
        return sub
