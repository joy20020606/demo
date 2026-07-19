from collections.abc import AsyncIterator
from typing import Annotated

from fastapi import Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.db.session import AsyncSessionFactory, get_db_session
from app.services.customer_service import CustomerService
from app.services.plan_service import PlanService
from app.services.subscription_service import SubscriptionService
from app.services.uow import AsyncUnitOfWork


async def get_uow() -> AsyncIterator[AsyncUnitOfWork]:
    async with AsyncUnitOfWork(AsyncSessionFactory) as uow:
        yield uow


SessionDep = Annotated[AsyncSession, Depends(get_db_session)]
UoWDep = Annotated[AsyncUnitOfWork, Depends(get_uow)]


def _customer_service(uow: UoWDep) -> CustomerService:
    return CustomerService(uow)


def _plan_service(uow: UoWDep) -> PlanService:
    return PlanService(uow)


def _subscription_service(uow: UoWDep) -> SubscriptionService:
    return SubscriptionService(uow)


CustomerServiceDep = Annotated[CustomerService, Depends(_customer_service)]
PlanServiceDep = Annotated[PlanService, Depends(_plan_service)]
SubscriptionServiceDep = Annotated[SubscriptionService, Depends(_subscription_service)]
