from dataclasses import dataclass
from typing import Protocol


@dataclass(frozen=True)
class Reading:
    name: str
    value: float


class DeviceUnavailable(Exception):
    pass


class PointReader(Protocol):
    async def read_all(self) -> list[Reading]: ...

    async def close(self) -> None: ...
