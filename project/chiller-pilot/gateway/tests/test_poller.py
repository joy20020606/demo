import json
from datetime import UTC, datetime, timedelta, timezone

from gw.poller import build_payload, poll_once
from gw.readers import DeviceUnavailable, Reading


class FakeReader:
    def __init__(self, readings=None, error=None):
        self.readings = readings or []
        self.error = error

    async def read_all(self):
        if self.error:
            raise self.error
        return self.readings

    async def close(self):
        pass


class Outbox:
    def __init__(self):
        self.messages = []

    async def publish(self, topic, payload):
        self.messages.append((topic, json.loads(payload)))


FIXED = datetime(2026, 10, 14, 2, 0, 0, tzinfo=UTC)


async def test_publishes_one_batch_to_the_site_topic():
    outbox = Outbox()
    reader = FakeReader([Reading("chw_supply_temp", 7.2), Reading("chiller_power", 285.0)])

    count = await poll_once("site_a", reader, outbox.publish, now=lambda: FIXED)

    assert count == 2
    assert outbox.messages == [
        (
            "site/site_a/telemetry",
            {
                "ts": "2026-10-14T02:00:00Z",
                "readings": [
                    {"name": "chw_supply_temp", "value": 7.2},
                    {"name": "chiller_power", "value": 285.0},
                ],
            },
        )
    ]


async def test_payload_does_not_carry_site_id():
    outbox = Outbox()
    await poll_once("site_a", FakeReader([Reading("x", 1.0)]), outbox.publish)
    assert "site_id" not in outbox.messages[0][1]


async def test_device_failure_publishes_nothing_and_does_not_raise():
    outbox = Outbox()
    reader = FakeReader(error=DeviceUnavailable("timeout"))

    count = await poll_once("site_a", reader, outbox.publish)

    assert count == 0
    assert outbox.messages == []


async def test_empty_read_publishes_nothing():
    outbox = Outbox()
    assert await poll_once("site_a", FakeReader([]), outbox.publish) == 0
    assert outbox.messages == []


def test_timestamp_is_always_utc():
    taipei = timezone(timedelta(hours=8))
    payload = json.loads(build_payload(datetime(2026, 10, 14, 10, 0, 0, tzinfo=taipei), []))
    assert payload["ts"] == "2026-10-14T02:00:00Z"
