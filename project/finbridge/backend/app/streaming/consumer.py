import json
import uuid

from sqlalchemy.orm import Session

from app.config import Settings, get_settings
from app.db.engine import SessionLocal
from app.db.models import NotificationLog


def build_consumer(settings: Settings):
    from confluent_kafka import Consumer

    conf = {
        "bootstrap.servers": settings.kafka_bootstrap,
        "group.id": "finbridge-notifier",
        "auto.offset.reset": "earliest",
    }
    if settings.kafka_security_protocol:
        conf["security.protocol"] = settings.kafka_security_protocol
        conf["sasl.mechanism"] = settings.kafka_sasl_mechanism
        conf["sasl.username"] = settings.kafka_sasl_username
        conf["sasl.password"] = settings.kafka_sasl_password
    return Consumer(conf)


def consume_batch(
    session: Session,
    settings: Settings,
    consumer,
    max_messages: int,
    timeout: float,
) -> int:
    if not settings.kafka_enabled:
        return 0

    count = 0
    for _ in range(max_messages):
        msg = consumer.poll(timeout)
        if msg is None:
            break
        if msg.error():
            continue

        data = json.loads(msg.value())
        payload = data.get("payload", {})
        event_type = data.get("event_type", "")

        tenant_raw = data.get("tenant_id")
        if tenant_raw is None and msg.key() is not None:
            tenant_raw = msg.key().decode()
        tenant_id = uuid.UUID(tenant_raw)

        session.add(
            NotificationLog(
                tenant_id=tenant_id,
                event_type=event_type,
                payload=payload,
            )
        )
        count += 1

    if count:
        session.commit()
    return count


def main() -> None:
    settings = get_settings()
    if not settings.kafka_enabled:
        return
    consumer = build_consumer(settings)
    consumer.subscribe([settings.kafka_topic])
    try:
        while True:
            session = SessionLocal()
            try:
                count = consume_batch(session, settings, consumer, max_messages=100, timeout=1.0)
                if count:
                    print(f"wrote {count} notification_log rows", flush=True)
            finally:
                session.close()
    finally:
        consumer.close()


if __name__ == "__main__":
    main()
