from __future__ import annotations

from decimal import Decimal
from typing import TYPE_CHECKING

from sqlalchemy import Enum, Numeric, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.core.enums import CustomerStatus, CustomerType
from app.db.base import Base, TimestampMixin, UUIDPrimaryKeyMixin

if TYPE_CHECKING:
    from app.models.subscription import Subscription


class Customer(Base, UUIDPrimaryKeyMixin, TimestampMixin):
    __tablename__ = "customers"

    code: Mapped[str] = mapped_column(String(20), unique=True, index=True)
    customer_type: Mapped[CustomerType] = mapped_column(
        Enum(CustomerType, native_enum=False, length=20)
    )
    name: Mapped[str] = mapped_column(String(100))
    email: Mapped[str] = mapped_column(String(255), index=True)
    phone: Mapped[str] = mapped_column(String(20))
    national_id: Mapped[str] = mapped_column(String(20))
    status: Mapped[CustomerStatus] = mapped_column(
        Enum(CustomerStatus, native_enum=False, length=20), default=CustomerStatus.ACTIVE
    )
    credit_limit: Mapped[Decimal] = mapped_column(Numeric(12, 2), default=Decimal("0"))

    subscriptions: Mapped[list[Subscription]] = relationship(
        back_populates="customer", cascade="all, delete-orphan"
    )
