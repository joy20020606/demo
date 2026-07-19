from fastapi import APIRouter

from app.api.v1 import customers, health, plans, subscriptions

api_v1_router = APIRouter(prefix="/api/v1")
api_v1_router.include_router(health.router)
api_v1_router.include_router(customers.router)
api_v1_router.include_router(plans.router)
api_v1_router.include_router(subscriptions.router)
