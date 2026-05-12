import os
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.api import auth, campaigns, ai

app = FastAPI(
    title="Mini Amazon Ads AI",
    version="0.1.0",
    description="多租戶 SaaS · FastAPI + ClickHouse + Claude Agent",
)

# CORS:從環境變數讀取 allow origins(支援多個,逗號分隔)
cors_origins_env = os.getenv("CORS_ORIGINS", "http://localhost:3000")
cors_origins = [o.strip() for o in cors_origins_env.split(",")]

# CORS:讓 Next.js 前端可以呼叫
app.add_middleware(
    CORSMiddleware,
    allow_origins=cors_origins ,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(auth.router)
app.include_router(campaigns.router)
app.include_router(ai.router)


@app.get("/")
async def root():
    return {"status": "ok", "service": "mini-amazon-ads-ai"}


@app.get("/health")
async def health():
    return {"status": "healthy"}
