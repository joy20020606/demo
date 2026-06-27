from __future__ import annotations

from types import TracebackType

from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker


class AsyncUnitOfWork:
    """Unit of Work.

    Mirrors EF Core's DbContext lifecycle: open a session, run multiple
    repository operations, then commit-or-rollback as one transaction.
    Concrete repositories are attached in `__aenter__` (see Phase 3).
    """

    session: AsyncSession

    def __init__(self, session_factory: async_sessionmaker[AsyncSession]) -> None:
        self._session_factory = session_factory

    async def __aenter__(self) -> AsyncUnitOfWork:
        self.session = self._session_factory()
        await self._register_repositories()
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

    async def _register_repositories(self) -> None:
        """Hook for subclasses / future phases to attach concrete repositories."""
        return None
