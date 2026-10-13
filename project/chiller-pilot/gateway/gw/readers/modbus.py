import logging

from pymodbus.client import AsyncModbusTcpClient
from pymodbus.exceptions import ModbusException

from gw.config import Point, SiteConfig
from gw.readers.base import DeviceUnavailable, PointReader, Reading

log = logging.getLogger("gw.modbus")


def decode(words: list[int], scale: float) -> float:
    raw = 0
    for word in words:
        raw = (raw << 16) | word
    return round(raw * scale, 6)


class ModbusReader(PointReader):
    def __init__(self, config: SiteConfig):
        self.config = config
        self.client = AsyncModbusTcpClient(config.host, port=config.port, timeout=3, retries=1)

    async def read_all(self) -> list[Reading]:
        if not self.client.connected and not await self.client.connect():
            raise DeviceUnavailable(f"cannot connect to {self.config.host}:{self.config.port}")

        readings = []
        for point in self.config.points:
            value = await self.read_point(point)
            if value is not None:
                readings.append(Reading(point.name, value))
        return readings

    async def read_point(self, point: Point) -> float | None:
        try:
            result = await self.client.read_holding_registers(
                point.address, count=point.words, device_id=self.config.device_id
            )
        except ModbusException as exc:
            self.client.close()
            raise DeviceUnavailable(str(exc)) from exc
        if result.isError():
            log.warning("%s: device rejected read at address %s", point.name, point.address)
            return None
        return decode(list(result.registers), point.scale)

    async def close(self) -> None:
        self.client.close()
