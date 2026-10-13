import json
import logging
from collections.abc import Awaitable, Callable
from datetime import UTC, datetime

from gw.readers import DeviceUnavailable, PointReader, Reading

log = logging.getLogger("gw.poller")

Publish = Callable[[str, str], Awaitable[None]]


def telemetry_topic(site_id: str) -> str:
    return f"site/{site_id}/telemetry"


def build_payload(ts: datetime, readings: list[Reading]) -> str:
    return json.dumps(
        {
            "ts": ts.astimezone(UTC).isoformat(timespec="seconds").replace("+00:00", "Z"),
            "readings": [{"name": r.name, "value": r.value} for r in readings],
        }
    )


async def poll_once(
    site_id: str,
    reader: PointReader,
    publish: Publish,
    now: Callable[[], datetime] = lambda: datetime.now(UTC),
) -> int:
    try:
        readings = await reader.read_all()
    except DeviceUnavailable as exc:
        log.warning("device unavailable: %s", exc)
        return 0
    if not readings:
        return 0
    await publish(telemetry_topic(site_id), build_payload(now(), readings))
    return len(readings)
