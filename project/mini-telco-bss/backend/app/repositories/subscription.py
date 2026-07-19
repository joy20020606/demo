from collections.abc import Sequence
from uuid import UUID

from sqlalchemy import func, select
from sqlalchemy.orm import selectinload

from app.models.subscription import Subscription
from app.repositories.base import BaseRepository


class SubscriptionRepository(BaseRepository[Subscription]):
    model = Subscription

    async def get_by_phone(self, phone_number: str) -> Subscription | None:
        stmt = select(Subscription).where(Subscription.phone_number == phone_number)
        return (await self.session.execute(stmt)).scalar_one_or_none()

    async def get_with_relations(self, id: UUID) -> Subscription | None:
        stmt = (
            select(Subscription)
            .where(Subscription.id == id)
            .options(
                selectinload(Subscription.customer),
                selectinload(Subscription.plan),
            )
        )
        return (await self.session.execute(stmt)).scalar_one_or_none()

    async def list_by_customer(self, customer_id: UUID) -> Sequence[Subscription]:
        stmt = (
            select(Subscription)
            .where(Subscription.customer_id == customer_id)
            .order_by(Subscription.start_date.desc())
        )
        return (await self.session.execute(stmt)).scalars().all()

    async def list_page(
        self, limit: int = 20, offset: int = 0
    ) -> tuple[Sequence[Subscription], int]:
        items_stmt = (
            select(Subscription)
            .order_by(Subscription.phone_number)
            .limit(limit)
            .offset(offset)
        )
        count_stmt = select(func.count()).select_from(Subscription)
        items = (await self.session.execute(items_stmt)).scalars().all()
        total = (await self.session.execute(count_stmt)).scalar_one()
        return items, total
