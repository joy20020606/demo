from collections.abc import Sequence
from uuid import UUID

from fastapi import HTTPException, status

from app.models.plan import Plan
from app.schemas.plan import PlanCreate
from app.services.uow import AsyncUnitOfWork


class PlanService:
    def __init__(self, uow: AsyncUnitOfWork) -> None:
        self.uow = uow

    async def get(self, plan_id: UUID) -> Plan:
        plan = await self.uow.plans.get(plan_id)
        if plan is None:
            raise HTTPException(status.HTTP_404_NOT_FOUND, "Plan not found")
        return plan

    async def list_active(self) -> Sequence[Plan]:
        return await self.uow.plans.list_active()

    async def create(self, payload: PlanCreate) -> Plan:
        existing = await self.uow.plans.get_by_code(payload.code)
        if existing is not None:
            raise HTTPException(
                status.HTTP_409_CONFLICT, f"Plan code '{payload.code}' already exists"
            )
        plan = Plan(**payload.model_dump())
        await self.uow.plans.add(plan)
        await self.uow.commit()
        return plan
