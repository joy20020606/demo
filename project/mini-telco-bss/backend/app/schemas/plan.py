from datetime import datetime
from decimal import Decimal
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field

from app.core.enums import PlanType


class PlanBase(BaseModel):
    name: str = Field(min_length=1, max_length=100)
    plan_type: PlanType
    monthly_fee: Decimal = Field(ge=0)
    included_minutes: int = Field(default=0, ge=0)
    included_sms: int = Field(default=0, ge=0)
    included_data_mb: int = Field(default=0, ge=0)
    overage_minute_rate: Decimal = Field(default=Decimal("0"), ge=0)
    overage_sms_rate: Decimal = Field(default=Decimal("0"), ge=0)
    overage_mb_rate: Decimal = Field(default=Decimal("0"), ge=0)
    is_active: bool = True


class PlanCreate(PlanBase):
    code: str = Field(min_length=4, max_length=30)


class PlanRead(PlanBase):
    model_config = ConfigDict(from_attributes=True)

    id: UUID
    code: str
    created_at: datetime
    updated_at: datetime
