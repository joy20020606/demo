import json
import time

from sqlalchemy import func, select
from sqlalchemy.orm import Session

from app.config import Settings, get_settings
from app.db.engine import SessionLocal
from app.db.models import Outbox, OutboxStatus


def build_producer(settings: Settings):
    from confluent_kafka import Producer

    conf = {"bootstrap.servers": settings.kafka_bootstrap}
    if settings.kafka_security_protocol:
        conf["security.protocol"] = settings.kafka_security_protocol
        conf["sasl.mechanism"] = settings.kafka_sasl_mechanism
        conf["sasl.username"] = settings.kafka_sasl_username
        conf["sasl.password"] = settings.kafka_sasl_password
    return Producer(conf)


def publish_pending(session: Session, settings: Settings, producer) -> int:
    if not settings.kafka_enabled:
        return 0

    rows = list(
        session.scalars(
            select(Outbox)
            .where(Outbox.status == OutboxStatus.pending)
            .order_by(Outbox.created_at)
        )
    )
    if not rows:
        return 0

    for row in rows:
        value = json.dumps(
            {
                "tenant_id": str(row.tenant_id),
                "event_type": row.event_type,
                "payload": row.payload,
            }
        ).encode()
        producer.produce(row.topic, key=str(row.tenant_id).encode(), value=value)

    producer.flush()

    for row in rows:
        row.status = OutboxStatus.published
        row.published_at = func.now()

    session.commit()
    return len(rows)


def main() -> None:
    settings = get_settings()
    if not settings.kafka_enabled:
        return
    producer = build_producer(settings)
    while True:
        session = SessionLocal()
        try:
            count = publish_pending(session, settings, producer)
            if count:
                print(f"published {count} outbox rows", flush=True)
        finally:
            session.close()
        time.sleep(2)


if __name__ == "__main__":
    main()
