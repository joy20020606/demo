from uuid import UUID

from fastapi import APIRouter, status

from app.core.deps import PlanServiceDep
from app.schemas.plan import PlanCreate, PlanRead

router = APIRouter(prefix="/plans", tags=["plans"])


@router.get("", response_model=list[PlanRead])
async def list_plans(svc: PlanServiceDep) -> list[PlanRead]:
    plans = await svc.list_active()
    return [PlanRead.model_validate(p) for p in plans]


@router.get("/{plan_id}", response_model=PlanRead)
async def get_plan(plan_id: UUID, svc: PlanServiceDep) -> PlanRead:
    plan = await svc.get(plan_id)
    return PlanRead.model_validate(plan)


@router.post("", response_model=PlanRead, status_code=status.HTTP_201_CREATED)
async def create_plan(payload: PlanCreate, svc: PlanServiceDep) -> PlanRead:
    plan = await svc.create(payload)
    return PlanRead.model_validate(plan)
