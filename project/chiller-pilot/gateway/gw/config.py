import re
from pathlib import Path
from typing import Literal

import yaml
from pydantic import BaseModel, ConfigDict, Field, field_validator, model_validator
from pydantic_settings import BaseSettings, SettingsConfigDict

WORDS = {"uint16": 1, "uint32": 2}


class Point(BaseModel):
    model_config = ConfigDict(extra="forbid")

    name: str
    address: int = Field(ge=0, le=65535)
    scale: float = Field(gt=0)
    unit: str
    type: Literal["uint16", "uint32"] = "uint16"

    @field_validator("name")
    @classmethod
    def name_is_snake_case(cls, value: str) -> str:
        if not re.fullmatch(r"[a-z][a-z0-9_]*", value):
            raise ValueError("point name must be lower snake_case")
        return value

    @property
    def words(self) -> int:
        return WORDS[self.type]


class SiteConfig(BaseModel):
    model_config = ConfigDict(extra="forbid")

    site_id: str
    protocol: Literal["modbus_tcp"]
    host: str
    port: int = Field(default=502, ge=1, le=65535)
    device_id: int = Field(default=1, ge=0, le=255)
    poll_interval_seconds: float = Field(default=10, gt=0)
    points: list[Point] = Field(min_length=1)

    @field_validator("site_id")
    @classmethod
    def site_id_is_topic_safe(cls, value: str) -> str:
        if not re.fullmatch(r"[a-z0-9_]+", value):
            raise ValueError("site_id may only contain a-z, 0-9 and underscore")
        return value

    @model_validator(mode="after")
    def points_do_not_collide(self) -> "SiteConfig":
        names: set[str] = set()
        used: dict[int, str] = {}
        for point in self.points:
            if point.name in names:
                raise ValueError(f"duplicate point name: {point.name}")
            names.add(point.name)
            for address in range(point.address, point.address + point.words):
                if address in used:
                    raise ValueError(
                        f"address {address} is used by both {used[address]} and {point.name}"
                    )
                used[address] = point.name
        return self


def load_site_config(path: str | Path) -> SiteConfig:
    with open(path, encoding="utf-8") as f:
        return SiteConfig.model_validate(yaml.safe_load(f))


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="GW_")

    site_config: str = "../sites/site_a.yaml"
    mqtt_host: str = "127.0.0.1"
    mqtt_port: int = 1883
    mqtt_username: str | None = None
    mqtt_password: str | None = None
    device_host: str | None = None
    device_port: int | None = None
