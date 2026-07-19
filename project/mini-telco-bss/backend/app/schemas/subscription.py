from datetime import date, datetime
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field

from app.core.enums import SubscriptionStatus


class SubscriptionBase(BaseModel):
    customer_id: UUID
    plan_id: UUID
    phone_number: str = Field(min_length=8, max_length=20)
    start_date: date


class SubscriptionCreate(SubscriptionBase):
    pass


class SubscriptionRead(SubscriptionBase):
    model_config = ConfigDict(from_attributes=True)

    id: UUID
    status: SubscriptionStatus
    end_date: date | None
    created_at: datetime
    updated_at: datetime


class SubscriptionTerminate(BaseModel):
    end_date: date | None = None
