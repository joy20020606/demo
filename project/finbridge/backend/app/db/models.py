import enum
import uuid
from datetime import datetime
from decimal import Decimal
from typing import Any

from sqlalchemy import (
    Enum,
    ForeignKey,
    Index,
    Integer,
    Numeric,
    String,
    Text,
    UniqueConstraint,
    func,
)
from sqlalchemy.dialects.postgresql import JSONB, UUID
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship


class Base(DeclarativeBase):
    pass


class InstrumentType(enum.StrEnum):
    equity = "equity"
    fund = "fund"
    bond = "bond"
    derivative = "derivative"


class TransactionType(enum.StrEnum):
    buy = "buy"
    sell = "sell"
    dividend = "dividend"
    fee = "fee"
    transfer = "transfer"


class ConnectorRunStatus(enum.StrEnum):
    running = "running"
    success = "success"
    partial = "partial"
    failed = "failed"


class DeadLetterStatus(enum.StrEnum):
    pending = "pending"
    replayed = "replayed"
    resolved = "resolved"


class ReconciliationBreakType(enum.StrEnum):
    qty_mismatch = "qty_mismatch"
    missing_position = "missing_position"
    extra_position = "extra_position"


class ReconciliationBreakStatus(enum.StrEnum):
    open = "open"
    acknowledged = "acknowledged"


class OutboxStatus(enum.StrEnum):
    pending = "pending"
    published = "published"


class Tenant(Base):
    __tablename__ = "tenants"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    slug: Mapped[str] = mapped_column(String(64), unique=True, index=True, nullable=False)
    name: Mapped[str] = mapped_column(String(256), nullable=False)
    created_at: Mapped[datetime] = mapped_column(server_default=func.now())

    instruments: Mapped[list["Instrument"]] = relationship(
        back_populates="tenant", cascade="all, delete-orphan"
    )
    positions: Mapped[list["Position"]] = relationship(
        back_populates="tenant", cascade="all, delete-orphan"
    )
    transactions: Mapped[list["Transaction"]] = relationship(
        back_populates="tenant", cascade="all, delete-orphan"
    )


class Instrument(Base):
    __tablename__ = "instruments"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    symbol: Mapped[str] = mapped_column(String(64), nullable=False)
    isin: Mapped[str | None] = mapped_column(String(12))
    instrument_type: Mapped[InstrumentType] = mapped_column(
        Enum(InstrumentType, name="instrument_type"), nullable=False
    )
    currency: Mapped[str] = mapped_column(String(3), nullable=False)
    name: Mapped[str] = mapped_column(String(256), nullable=False)

    tenant: Mapped["Tenant"] = relationship(back_populates="instruments")
    positions: Mapped[list["Position"]] = relationship(
        back_populates="instrument", cascade="all, delete-orphan"
    )
    transactions: Mapped[list["Transaction"]] = relationship(back_populates="instrument")

    __table_args__ = (UniqueConstraint("tenant_id", "symbol", name="uq_instrument_tenant_symbol"),)


class Position(Base):
    __tablename__ = "positions"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    instrument_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("instruments.id", ondelete="CASCADE"), nullable=False
    )
    account_id: Mapped[str] = mapped_column(String(64), nullable=False)
    quantity: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    avg_cost: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    source: Mapped[str] = mapped_column(String(64), nullable=False)
    as_of: Mapped[datetime] = mapped_column(nullable=False)
    updated_at: Mapped[datetime] = mapped_column(
        server_default=func.now(), onupdate=func.now()
    )

    tenant: Mapped["Tenant"] = relationship(back_populates="positions")
    instrument: Mapped["Instrument"] = relationship(back_populates="positions")

    __table_args__ = (
        UniqueConstraint(
            "tenant_id",
            "account_id",
            "instrument_id",
            "source",
            name="uq_position_tenant_account_instrument_source",
        ),
    )


class Transaction(Base):
    __tablename__ = "transactions"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    instrument_id: Mapped[uuid.UUID | None] = mapped_column(
        UUID(as_uuid=True), ForeignKey("instruments.id", ondelete="SET NULL")
    )
    account_id: Mapped[str] = mapped_column(String(64), nullable=False)
    txn_type: Mapped[TransactionType] = mapped_column(
        Enum(TransactionType, name="transaction_type"), nullable=False
    )
    quantity: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    price: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    amount: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    currency: Mapped[str] = mapped_column(String(3), nullable=False)
    trade_date: Mapped[datetime] = mapped_column(nullable=False)
    source: Mapped[str] = mapped_column(String(64), nullable=False)
    source_message_id: Mapped[str] = mapped_column(String(128), nullable=False)
    content_hash: Mapped[str] = mapped_column(String(64), nullable=False)
    created_at: Mapped[datetime] = mapped_column(server_default=func.now())

    tenant: Mapped["Tenant"] = relationship(back_populates="transactions")
    instrument: Mapped["Instrument | None"] = relationship(back_populates="transactions")

    __table_args__ = (
        UniqueConstraint(
            "tenant_id",
            "source",
            "source_message_id",
            name="uq_transaction_tenant_source_message",
        ),
        UniqueConstraint("content_hash", name="uq_transaction_content_hash"),
    )


class ConnectorRun(Base):
    __tablename__ = "connector_runs"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    connector: Mapped[str] = mapped_column(String(64), nullable=False)
    status: Mapped[ConnectorRunStatus] = mapped_column(
        Enum(ConnectorRunStatus, name="connector_run_status"), nullable=False
    )
    started_at: Mapped[datetime] = mapped_column(server_default=func.now())
    finished_at: Mapped[datetime | None] = mapped_column()
    messages_total: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    messages_ok: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    messages_dead: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    error: Mapped[str | None] = mapped_column(Text)

    tenant: Mapped["Tenant"] = relationship()


class DeadLetter(Base):
    __tablename__ = "dead_letters"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    connector: Mapped[str] = mapped_column(String(64), nullable=False)
    source_message_id: Mapped[str | None] = mapped_column(String(128))
    raw_payload: Mapped[str] = mapped_column(Text, nullable=False)
    error_type: Mapped[str] = mapped_column(String(128), nullable=False)
    error_detail: Mapped[str] = mapped_column(Text, nullable=False)
    retry_count: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    status: Mapped[DeadLetterStatus] = mapped_column(
        Enum(DeadLetterStatus, name="dead_letter_status"),
        nullable=False,
        default=DeadLetterStatus.pending,
    )
    created_at: Mapped[datetime] = mapped_column(server_default=func.now())

    tenant: Mapped["Tenant"] = relationship()


class ReconciliationBreak(Base):
    __tablename__ = "reconciliation_breaks"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    instrument_id: Mapped[uuid.UUID | None] = mapped_column(
        UUID(as_uuid=True), ForeignKey("instruments.id", ondelete="SET NULL")
    )
    account_id: Mapped[str] = mapped_column(String(64), nullable=False)
    symbol: Mapped[str] = mapped_column(String(64), nullable=False)
    break_type: Mapped[ReconciliationBreakType] = mapped_column(
        Enum(ReconciliationBreakType, name="reconciliation_break_type"), nullable=False
    )
    expected: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    actual: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    delta: Mapped[Decimal] = mapped_column(Numeric(28, 8), nullable=False)
    status: Mapped[ReconciliationBreakStatus] = mapped_column(
        Enum(ReconciliationBreakStatus, name="reconciliation_break_status"),
        nullable=False,
        default=ReconciliationBreakStatus.open,
    )
    detected_at: Mapped[datetime] = mapped_column(
        server_default=func.now(), onupdate=func.now()
    )

    tenant: Mapped["Tenant"] = relationship()
    instrument: Mapped["Instrument | None"] = relationship()

    __table_args__ = (
        UniqueConstraint(
            "tenant_id",
            "account_id",
            "symbol",
            "break_type",
            name="uq_recon_break_tenant_account_symbol_type",
        ),
    )


class Outbox(Base):
    __tablename__ = "outbox"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    topic: Mapped[str] = mapped_column(String(128), nullable=False)
    event_type: Mapped[str] = mapped_column(String(64), nullable=False)
    payload: Mapped[dict[str, Any]] = mapped_column(JSONB, nullable=False)
    status: Mapped[OutboxStatus] = mapped_column(
        Enum(OutboxStatus, name="outbox_status"),
        nullable=False,
        default=OutboxStatus.pending,
    )
    created_at: Mapped[datetime] = mapped_column(server_default=func.now())
    published_at: Mapped[datetime | None] = mapped_column()

    tenant: Mapped["Tenant"] = relationship()

    __table_args__ = (Index("ix_outbox_status_created_at", "status", "created_at"),)


class NotificationLog(Base):
    __tablename__ = "notification_log"

    id: Mapped[uuid.UUID] = mapped_column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    tenant_id: Mapped[uuid.UUID] = mapped_column(
        UUID(as_uuid=True), ForeignKey("tenants.id", ondelete="CASCADE"), nullable=False
    )
    event_type: Mapped[str] = mapped_column(String(64), nullable=False)
    payload: Mapped[dict[str, Any]] = mapped_column(JSONB, nullable=False)
    received_at: Mapped[datetime] = mapped_column(server_default=func.now())

    tenant: Mapped["Tenant"] = relationship()
