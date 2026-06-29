from __future__ import annotations

from decimal import Decimal
from typing import TYPE_CHECKING

from sqlalchemy import Boolean, Enum, Numeric, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.enums import PlanType
from app.db.base import Base, TimestampMixin, UUIDPrimaryKeyMixin

if TYPE_CHECKING:
    from app.models.subscription import Subscription


class Plan(Base, UUIDPrimaryKeyMixin, TimestampMixin):
    __tablename__ = "plans"

    code: Mapped[str] = mapped_column(String(30), unique=True, index=True)
    name: Mapped[str] = mapped_column(String(100))
    plan_type: Mapped[PlanType] = mapped_column(Enum(PlanType, native_enum=False, length=20))

    monthly_fee: Mapped[Decimal] = mapped_column(Numeric(10, 2), default=Decimal("0"))

    included_minutes: Mapped[int] = mapped_column(default=0)
    included_sms: Mapped[int] = mapped_column(default=0)
    included_data_mb: Mapped[int] = mapped_column(default=0)

    overage_minute_rate: Mapped[Decimal] = mapped_column(Numeric(8, 4), default=Decimal("0"))
    overage_sms_rate: Mapped[Decimal] = mapped_column(Numeric(8, 4), default=Decimal("0"))
    overage_mb_rate: Mapped[Decimal] = mapped_column(Numeric(8, 4), default=Decimal("0"))

    is_active: Mapped[bool] = mapped_column(Boolean, default=True)

    subscriptions: Mapped[list[Subscription]] = relationship(back_populates="plan")
