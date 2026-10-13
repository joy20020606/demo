import asyncio
import logging
import math
import random

from pymodbus.server import ModbusTcpServer
from pymodbus.simulator import DataType, SimData, SimDevice

from sim.model import CHW_SETPOINT_RANGE, clamp, compute
from sim.profile import Profile
from sim.registers import decode, encode

log = logging.getLogger("sim")

READ_COILS = 1
READ_HOLDING = 3
WRITE_HOLDING = 16
FAULT_COIL_COUNT = 8
SUPPLY_TEMP_LAG_SECONDS = 300


class Simulator:
    def __init__(self, profile: Profile, rng: random.Random | None = None):
        self.profile = profile
        self.params = profile.model.to_params()
        self.rng = rng or random.Random()
        self.sim_seconds = profile.start_hour * 3600
        self.chw_supply_temp = profile.defaults.chw_setpoint
        self.reported_supply_temp = self.chw_supply_temp
        self.server: ModbusTcpServer | None = None

    @property
    def hour(self) -> float:
        return (self.sim_seconds / 3600) % 24

    def build_device(self) -> SimDevice:
        holding = [
            SimData(address=r.address, count=r.words, values=0, datatype=DataType.REGISTERS)
            for r in sorted(self.profile.registers, key=lambda r: r.address)
        ]
        coils = [SimData(address=0, count=FAULT_COIL_COUNT, values=False, datatype=DataType.BITS)]
        unused_bits = [SimData(address=0, count=1, values=False, datatype=DataType.BITS)]
        unused_registers = [SimData(address=0, count=1, values=0, datatype=DataType.REGISTERS)]
        return SimDevice(
            id=self.profile.device_id,
            simdata=(coils, unused_bits, holding, unused_registers),
        )

    async def start(self, host: str, port: int) -> None:
        self.server = ModbusTcpServer(self.build_device(), address=(host, port))
        await self.server.serve_forever(background=True)
        await self.write_point("chw_setpoint", self.profile.defaults.chw_setpoint)
        await self.write_point("ct_fan_speed", self.profile.defaults.ct_fan_speed)
        await self.tick(0)

    async def stop(self) -> None:
        if self.server:
            await self.server.shutdown()

    async def read_point(self, name: str) -> float:
        register = self.profile.register(name)
        raw = await self.server.async_getValues(
            self.profile.device_id, READ_HOLDING, register.address, register.words
        )
        return decode(raw, register.scale)

    async def write_point(self, name: str, value: float) -> None:
        register = self.profile.register(name)
        await self.server.async_setValues(
            self.profile.device_id,
            WRITE_HOLDING,
            register.address,
            encode(value, register.scale, register.words),
        )

    async def read_fault(self, coil: int) -> bool:
        values = await self.server.async_getValues(self.profile.device_id, READ_COILS, coil, 1)
        return bool(values[0])

    def noisy(self, value: float) -> float:
        return value * (1 + self.rng.gauss(0, self.profile.noise))

    async def tick(self, real_seconds: float) -> None:
        sim_delta = real_seconds * self.profile.time_scale
        self.sim_seconds += sim_delta

        setpoint = await self.read_point("chw_setpoint")
        fan_speed = await self.read_point("ct_fan_speed")
        state = compute(self.params, self.hour, setpoint, fan_speed)

        target = clamp(setpoint, *CHW_SETPOINT_RANGE)
        gain = 1 - math.exp(-sim_delta / SUPPLY_TEMP_LAG_SECONDS)
        self.chw_supply_temp += (target - self.chw_supply_temp) * gain

        if not await self.read_fault(self.profile.faults.sensor_stuck):
            self.reported_supply_temp = self.noisy(self.chw_supply_temp)
        supply_temp = self.reported_supply_temp
        if await self.read_fault(self.profile.faults.scale_error):
            supply_temp *= 10

        chiller = self.noisy(state.chiller_power_kw)
        pump = self.noisy(state.chw_pump_power_kw)
        fan = self.noisy(state.ct_fan_power_kw)
        values = {
            "chw_supply_temp": supply_temp,
            "chw_return_temp": self.noisy(state.chw_return_temp),
            "cw_supply_temp": self.noisy(state.cw_supply_temp),
            "outdoor_temp": self.noisy(state.outdoor_temp),
            "cooling_load": self.noisy(state.cooling_load_kw),
            "chiller_power": chiller,
            "chw_pump_power": pump,
            "ct_fan_power": fan,
            "total_power": chiller + pump + fan,
        }
        for register in self.profile.registers:
            if register.name in values:
                await self.write_point(register.name, values[register.name])

    async def run(self, host: str, port: int, interval: float = 1.0) -> None:
        await self.start(host, port)
        log.info("simulator %s listening on %s:%s", self.profile.site_id, host, port)
        try:
            while True:
                await asyncio.sleep(interval)
                await self.tick(interval)
        finally:
            await self.stop()
