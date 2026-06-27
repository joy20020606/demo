from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy import text

from app import __version__
from app.api.v1 import api_v1_router
from app.core.config import get_settings
from app.db.session import engine


@asynccontextmanager
async def lifespan(app: FastAPI):
    settings = get_settings()
    async with engine.connect() as conn:
        await conn.execute(text("SELECT 1"))
    print(f"[{settings.APP_NAME}] DB connection verified at startup.")
    yield
    await engine.dispose()


def create_app() -> FastAPI:
    settings = get_settings()

    app = FastAPI(
        title=settings.APP_NAME,
        version=__version__,
        description="Telco BSS Lite — Python full-stack demo.",
        debug=settings.DEBUG,
        lifespan=lifespan,
    )

    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.CORS_ORIGINS,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    app.include_router(api_v1_router)

    @app.get("/", tags=["root"])
    async def root() -> dict[str, str]:
        return {
            "app": settings.APP_NAME,
            "version": __version__,
            "docs": "/docs",
            "api": "/api/v1",
        }

    return app


app = create_app()
