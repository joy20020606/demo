import uuid

from sqlalchemy import select
from sqlalchemy.orm import Session

from app.connectors.base import upsert_transaction
from app.db.models import DeadLetter, DeadLetterStatus, Tenant
from app.logging_config import get_logger
from app.normalize import normalize_dead_letter_payload

logger = get_logger("finbridge.deadletter")


def quarantine(
    session: Session,
    tenant_id: uuid.UUID,
    connector: str,
    source_message_id: str | None,
    raw_payload: str,
    exc: Exception,
) -> DeadLetter:
    dead_letter = DeadLetter(
        tenant_id=tenant_id,
        connector=connector,
        source_message_id=source_message_id,
        raw_payload=raw_payload,
        error_type=type(exc).__name__,
        error_detail=str(exc),
    )
    session.add(dead_letter)
    session.flush()
    logger.error(
        "message quarantined",
        extra={
            "tenant": str(tenant_id),
            "connector": connector,
            "source_message_id": source_message_id,
            "error_type": type(exc).__name__,
            "dead_letter_id": str(dead_letter.id),
        },
    )
    return dead_letter


def replay(session: Session, dead_letter_id: uuid.UUID) -> DeadLetter:
    dead_letter = session.get(DeadLetter, dead_letter_id)
    if dead_letter is None:
        raise ValueError("dead letter not found")

    tenant_slug = session.scalar(
        select(Tenant.slug).where(Tenant.id == dead_letter.tenant_id)
    )
    try:
        txn = normalize_dead_letter_payload(dead_letter.connector, dead_letter.raw_payload)
        upsert_transaction(
            session, dead_letter.tenant_id, tenant_slug, dead_letter.connector, txn
        )
        dead_letter.status = DeadLetterStatus.replayed
        logger.info(
            "dead letter replayed",
            extra={
                "tenant": str(dead_letter.tenant_id),
                "connector": dead_letter.connector,
                "source_message_id": dead_letter.source_message_id,
                "dead_letter_id": str(dead_letter.id),
            },
        )
    except Exception as exc:  # noqa: BLE001
        dead_letter.retry_count += 1
        dead_letter.status = DeadLetterStatus.pending
        logger.error(
            "dead letter replay failed",
            extra={
                "tenant": str(dead_letter.tenant_id),
                "connector": dead_letter.connector,
                "source_message_id": dead_letter.source_message_id,
                "dead_letter_id": str(dead_letter.id),
                "error_type": type(exc).__name__,
            },
        )
    session.flush()
    return dead_letter
