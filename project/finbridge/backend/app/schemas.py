import uuid
from datetime import datetime
from decimal import Decimal

from pydantic import BaseModel, ConfigDict, Field

from app.db.models import (
    ConnectorRunStatus,
    DeadLetterStatus,
    InstrumentType,
    ReconciliationBreakStatus,
    ReconciliationBreakType,
    TransactionType,
)


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


class ConnectorRunOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    tenant_id: uuid.UUID
    connector: str
    status: ConnectorRunStatus
    started_at: datetime
    finished_at: datetime | None
    messages_total: int
    messages_ok: int
    messages_dead: int
    error: str | None


class DeadLetterOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    tenant_id: uuid.UUID
    connector: str
    source_message_id: str | None
    raw_payload: str
    error_type: str
    error_detail: str
    retry_count: int
    status: DeadLetterStatus
    created_at: datetime


class ReconciliationBreakOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: uuid.UUID
    tenant_id: uuid.UUID
    instrument_id: uuid.UUID | None
    account_id: str
    symbol: str
    break_type: ReconciliationBreakType
    expected: Decimal
    actual: Decimal
    delta: Decimal
    status: ReconciliationBreakStatus
    detected_at: datetime
