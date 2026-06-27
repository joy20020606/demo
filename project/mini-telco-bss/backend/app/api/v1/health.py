from fastapi import APIRouter, status
from sqlalchemy import text

from app.core.deps import SessionDep

router = APIRouter(tags=["health"])


@router.get("/health", status_code=status.HTTP_200_OK)
async def health() -> dict[str, str]:
    return {"status": "ok"}


@router.get("/health/db", status_code=status.HTTP_200_OK)
async def health_db(session: SessionDep) -> dict[str, str]:
    result = await session.execute(text("SELECT 1"))
    value = result.scalar_one()
    return {"status": "ok", "db": "up", "result": str(value)}
