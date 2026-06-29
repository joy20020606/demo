from __future__ import annotations

from typing import Any

from sqlalchemy import Boolean, String, UniqueConstraint
from sqlalchemy.dialects.postgresql import JSONB
from sqlalchemy.orm import Mapped, mapped_column

from app.db.base import Base, TimestampMixin, UUIDPrimaryKeyMixin


class WebhookEvent(Base, UUIDPrimaryKeyMixin, TimestampMixin):
    __tablename__ = "webhook_events"
    __table_args__ = (
        UniqueConstraint("source", "event_id", name="uq_webhook_source_event"),
    )

    source: Mapped[str] = mapped_column(String(50), index=True)
    event_id: Mapped[str] = mapped_column(String(100), index=True)
    payload: Mapped[dict[str, Any]] = mapped_column(JSONB, default=dict)
    processed: Mapped[bool] = mapped_column(Boolean, default=False)
    error: Mapped[str | None] = mapped_column(String(500), nullable=True)
