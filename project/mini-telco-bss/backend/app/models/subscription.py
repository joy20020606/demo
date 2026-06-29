from __future__ import annotations

from datetime import date
from typing import TYPE_CHECKING
from uuid import UUID

from sqlalchemy import Date, Enum, ForeignKey, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.enums import SubscriptionStatus
from app.db.base import Base, TimestampMixin, UUIDPrimaryKeyMixin

if TYPE_CHECKING:
    from app.models.bill import Bill
    from app.models.customer import Customer
    from app.models.plan import Plan
    from app.models.usage import UsageRecord


class Subscription(Base, UUIDPrimaryKeyMixin, TimestampMixin):
    __tablename__ = "subscriptions"

    customer_id: Mapped[UUID] = mapped_column(
        ForeignKey("customers.id", ondelete="CASCADE"), index=True
    )
    plan_id: Mapped[UUID] = mapped_column(ForeignKey("plans.id"), index=True)

    phone_number: Mapped[str] = mapped_column(String(20), unique=True, index=True)
    status: Mapped[SubscriptionStatus] = mapped_column(
        Enum(SubscriptionStatus, native_enum=False, length=20),
        default=SubscriptionStatus.ACTIVE,
    )
    start_date: Mapped[date] = mapped_column(Date)
    end_date: Mapped[date | None] = mapped_column(Date, nullable=True)

    customer: Mapped[Customer] = relationship(back_populates="subscriptions")
    plan: Mapped[Plan] = relationship(back_populates="subscriptions")
    usage_records: Mapped[list[UsageRecord]] = relationship(
        back_populates="subscription", cascade="all, delete-orphan"
    )
    bills: Mapped[list[Bill]] = relationship(
        back_populates="subscription", cascade="all, delete-orphan"
    )
