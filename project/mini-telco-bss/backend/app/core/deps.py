from collections.abc import AsyncIterator
from typing import Annotated

from fastapi import Depends
from sqlalchemy.ext.asyncio import AsyncSession

from app.db.session import AsyncSessionFactory, get_db_session
from app.services.uow import AsyncUnitOfWork


async def get_uow() -> AsyncIterator[AsyncUnitOfWork]:
    async with AsyncUnitOfWork(AsyncSessionFactory) as uow:
        yield uow


SessionDep = Annotated[AsyncSession, Depends(get_db_session)]
UoWDep = Annotated[AsyncUnitOfWork, Depends(get_uow)]
