import uuid
from datetime import datetime
from decimal import Decimal

from pydantic import BaseModel, ConfigDict, Field

from app.db.models import InstrumentType, TransactionType


class TenantCreate(BaseModel):
    slug: str = Field(min_length=1, max_length=64)
    name: str = Field(min_length=1, max_length=256)


class TenantOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    slug: str
    name: str
    created_at: datetime


class InstrumentOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    tenant_id: uuid.UUID
    symbol: str
    isin: str | None
    instrument_type: InstrumentType
    currency: str
    name: str


class PositionOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    tenant_id: uuid.UUID
    instrument_id: uuid.UUID
    account_id: str
    quantity: Decimal
    avg_cost: Decimal
    source: str
    as_of: datetime
    updated_at: datetime


class TransactionOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    tenant_id: uuid.UUID
    instrument_id: uuid.UUID | None
    account_id: str
    txn_type: TransactionType
    quantity: Decimal
    price: Decimal
    amount: Decimal
    currency: str
    trade_date: datetime
    source: str
    source_message_id: str
    content_hash: str
