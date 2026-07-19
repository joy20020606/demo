from collections.abc import Sequence

from sqlalchemy import select

from app.models.plan import Plan
from app.repositories.base import BaseRepository


class PlanRepository(BaseRepository[Plan]):
    model = Plan

    async def get_by_code(self, code: str) -> Plan | None:
        stmt = select(Plan).where(Plan.code == code)
        result = await self.session.execute(stmt)
        return result.scalar_one_or_none()

    async def list_active(self) -> Sequence[Plan]:
        stmt = select(Plan).where(Plan.is_active.is_(True)).order_by(Plan.code)
        result = await self.session.execute(stmt)
        return result.scalars().all()
