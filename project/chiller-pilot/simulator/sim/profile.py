from pathlib import Path
from typing import Literal

import yaml
from pydantic import BaseModel, ConfigDict, Field, model_validator

from sim.model import ModelParams

PointName = Literal[
    "chw_supply_temp",
    "chw_return_temp",
    "cw_supply_temp",
    "outdoor_temp",
    "cooling_load",
    "chiller_power",
    "chw_pump_power",
    "ct_fan_power",
    "total_power",
    "chw_setpoint",
    "ct_fan_speed",
]

WRITABLE_POINTS = {"chw_setpoint", "ct_fan_speed"}


class Register(BaseModel):
    model_config = ConfigDict(extra="forbid")

    name: PointName
    address: int = Field(ge=0, le=65535)
    scale: float = Field(gt=0)
    words: Literal[1, 2] = 1


class Defaults(BaseModel):
    model_config = ConfigDict(extra="forbid")

    chw_setpoint: float
    ct_fan_speed: float


class Faults(BaseModel):
    model_config = ConfigDict(extra="forbid")

    sensor_stuck: int = 0
    scale_error: int = 1


class PlantConfig(BaseModel):
    model_config = ConfigDict(extra="forbid")

    design_load_kw: float
    cop_ref: float
    cop_per_chw_degc: float
    cop_per_cw_degc: float
    pump_rated_kw: float
    fan_rated_kw: float
    approach_min_degc: float
    approach_gain_degc: float
    outdoor_mean_degc: float
    outdoor_swing_degc: float

    def to_params(self) -> ModelParams:
        return ModelParams(**self.model_dump())


class Profile(BaseModel):
    model_config = ConfigDict(extra="forbid")

    site_id: str
    device_id: int = Field(default=1, ge=1, le=247)
    time_scale: float = Field(default=60, gt=0)
    start_hour: float = Field(default=8, ge=0, lt=24)
    noise: float = Field(default=0.01, ge=0)
    model: PlantConfig
    defaults: Defaults
    registers: list[Register]
    faults: Faults = Faults()

    @model_validator(mode="after")
    def check_registers(self) -> "Profile":
        names = [r.name for r in self.registers]
        if len(names) != len(set(names)):
            raise ValueError("duplicate register name")
        missing = WRITABLE_POINTS - set(names)
        if missing:
            raise ValueError(f"missing writable registers: {sorted(missing)}")
        used: dict[int, str] = {}
        for register in self.registers:
            for address in range(register.address, register.address + register.words):
                if address in used:
                    raise ValueError(
                        f"address {address} used by both {used[address]} and {register.name}"
                    )
                used[address] = register.name
        return self

    def register(self, name: str) -> Register:
        return next(r for r in self.registers if r.name == name)


def load_profile(path: str | Path) -> Profile:
    with open(path, encoding="utf-8") as f:
        return Profile.model_validate(yaml.safe_load(f))
