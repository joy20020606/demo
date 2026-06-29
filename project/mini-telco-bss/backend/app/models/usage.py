from __future__ import annotations

from datetime import datetime
from typing import TYPE_CHECKING
from uuid import UUID

from sqlalchemy import DateTime, Enum, ForeignKey, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.enums import UsageType
from app.db.base import Base, TimestampMixin, UUIDPrimaryKeyMixin

if TYPE_CHECKING:
    from app.models.subscription import Subscription


class UsageRecord(Base, UUIDPrimaryKeyMixin, TimestampMixin):
    __tablename__ = "usage_records"

    subscription_id: Mapped[UUID] = mapped_column(
        ForeignKey("subscriptions.id", ondelete="CASCADE"), index=True
    )
    usage_type: Mapped[UsageType] = mapped_column(Enum(UsageType, native_enum=False, length=10))
    quantity: Mapped[int] = mapped_column()
    recorded_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    billing_period: Mapped[str] = mapped_column(String(7), index=True)

    subscription: Mapped[Subscription] = relationship(back_populates="usage_records")
