from datetime import datetime
from decimal import Decimal
from uuid import UUID

from pydantic import BaseModel, ConfigDict, EmailStr, Field

from app.core.enums import CustomerStatus, CustomerType


class CustomerBase(BaseModel):
    customer_type: CustomerType
    name: str = Field(min_length=1, max_length=100)
    email: EmailStr
    phone: str = Field(min_length=8, max_length=20)
    national_id: str = Field(min_length=8, max_length=20)
    credit_limit: Decimal = Field(default=Decimal("0"), ge=0)


class CustomerCreate(CustomerBase):
    code: str = Field(min_length=4, max_length=20)


class CustomerUpdate(BaseModel):
    name: str | None = Field(default=None, min_length=1, max_length=100)
    email: EmailStr | None = None
    phone: str | None = Field(default=None, min_length=8, max_length=20)
    status: CustomerStatus | None = None
    credit_limit: Decimal | None = Field(default=None, ge=0)


class CustomerRead(CustomerBase):
    model_config = ConfigDict(from_attributes=True)

    id: UUID
    code: str
    status: CustomerStatus
    created_at: datetime
    updated_at: datetime


class CustomerSearchFilters(BaseModel):
    status: CustomerStatus | None = None
    customer_type: CustomerType | None = None
    q: str | None = Field(default=None, description="name / code / email substring")
