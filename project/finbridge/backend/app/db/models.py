import enum
import uuid
from datetime import datetime
from decimal import Decimal

from sqlalchemy import (
    Enum,
    ForeignKey,
    Numeric,
    String,
    UniqueConstraint,
    func,
)
from sqlalchemy.dialects.postgresql import UUID
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
