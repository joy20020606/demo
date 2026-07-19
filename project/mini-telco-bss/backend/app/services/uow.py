from __future__ import annotations

from types import TracebackType

from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker

from app.repositories.bill import BillRepository
from app.repositories.customer import CustomerRepository
from app.repositories.plan import PlanRepository
from app.repositories.subscription import SubscriptionRepository


class AsyncUnitOfWork:
    """Transaction boundary + repository registry.

    Mirrors EF Core's DbContext: open one session, attach repositories
    sharing that session, commit/rollback atomically.
    """

    session: AsyncSession
    customers: CustomerRepository
    plans: PlanRepository
    subscriptions: SubscriptionRepository
    bills: BillRepository

    def __init__(self, session_factory: async_sessionmaker[AsyncSession]) -> None:
        self._session_factory = session_factory

    async def __aenter__(self) -> AsyncUnitOfWork:
        self.session = self._session_factory()
        self.customers = CustomerRepository(self.session)
        self.plans = PlanRepository(self.session)
        self.subscriptions = SubscriptionRepository(self.session)
        self.bills = BillRepository(self.session)
        return self

    async def __aexit__(
        self,
        exc_type: type[BaseException] | None,
        exc: BaseException | None,
        tb: TracebackType | None,
    ) -> None:
        try:
            if exc_type is not None:
                await self.rollback()
        finally:
            await self.session.close()

    async def commit(self) -> None:
        await self.session.commit()

    async def rollback(self) -> None:
        await self.session.rollback()
