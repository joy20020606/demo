from app.config import Settings
from app.streaming.publisher import publish_pending


class _ExplodingProducer:
    def produce(self, *args, **kwargs):
        raise AssertionError("producer must not be used when kafka is disabled")

    def flush(self, *args, **kwargs):
        raise AssertionError("producer must not be used when kafka is disabled")


class _ExplodingSession:
    def scalars(self, *args, **kwargs):
        raise AssertionError("session must not be queried when kafka is disabled")

    def commit(self, *args, **kwargs):
        raise AssertionError("session must not commit when kafka is disabled")


def test_publish_pending_noop_when_disabled():
    settings = Settings(kafka_enabled=False)
    assert settings.kafka_enabled is False

    result = publish_pending(_ExplodingSession(), settings, _ExplodingProducer())

    assert result == 0
