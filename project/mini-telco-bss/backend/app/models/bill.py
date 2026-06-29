from __future__ import annotations

from datetime import date, datetime
from decimal import Decimal
from typing import TYPE_CHECKING
from uuid import UUID

from sqlalchemy import Date, DateTime, Enum, ForeignKey, Numeric, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.enums import BillStatus
from app.db.base import Base, TimestampMixin, UUIDPrimaryKeyMixin

if TYPE_CHECKING:
    from app.models.subscription import Subscription


class Bill(Base, UUIDPrimaryKeyMixin, TimestampMixin):
    __tablename__ = "bills"
    __table_args__ = (
        UniqueConstraint("subscription_id", "billing_period", name="uq_bill_subscription_period"),
    )

    subscription_id: Mapped[UUID] = mapped_column(
        ForeignKey("subscriptions.id", ondelete="CASCADE"), index=True
    )
    billing_period: Mapped[str] = mapped_column(String(7), index=True)

    monthly_fee: Mapped[Decimal] = mapped_column(Numeric(10, 2), default=Decimal("0"))
    overage_charge: Mapped[Decimal] = mapped_column(Numeric(10, 2), default=Decimal("0"))
    total_amount: Mapped[Decimal] = mapped_column(Numeric(10, 2))

    status: Mapped[BillStatus] = mapped_column(
        Enum(BillStatus, native_enum=False, length=20), default=BillStatus.ISSUED
    )
    issued_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    due_date: Mapped[date] = mapped_column(Date)
    paid_at: Mapped[datetime | None] = mapped_column(DateTime(timezone=True), nullable=True)

    subscription: Mapped[Subscription] = relationship(back_populates="bills")
