from collections.abc import Sequence
from uuid import UUID

from sqlalchemy import select

from app.models.bill import Bill
from app.repositories.base import BaseRepository


class BillRepository(BaseRepository[Bill]):
    model = Bill

    async def list_by_subscription(self, subscription_id: UUID) -> Sequence[Bill]:
        stmt = (
            select(Bill)
            .where(Bill.subscription_id == subscription_id)
            .order_by(Bill.billing_period.desc())
        )
        return (await self.session.execute(stmt)).scalars().all()

    async def get_for_period(
        self, subscription_id: UUID, billing_period: str
    ) -> Bill | None:
        stmt = select(Bill).where(
            Bill.subscription_id == subscription_id,
            Bill.billing_period == billing_period,
        )
        return (await self.session.execute(stmt)).scalar_one_or_none()
