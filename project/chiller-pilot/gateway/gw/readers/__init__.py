from gw.config import SiteConfig
from gw.readers.base import DeviceUnavailable, PointReader, Reading
from gw.readers.modbus import ModbusReader

READERS: dict[str, type[PointReader]] = {
    "modbus_tcp": ModbusReader,
}


def build_reader(config: SiteConfig) -> PointReader:
    return READERS[config.protocol](config)


__all__ = ["DeviceUnavailable", "PointReader", "Reading", "build_reader"]
