import random
import socket

import pytest
from pymodbus.client import AsyncModbusTcpClient

from sim.profile import load_profile
from sim.registers import decode, encode
from sim.server import Simulator


def free_port() -> int:
    with socket.socket() as s:
        s.bind(("127.0.0.1", 0))
        return s.getsockname()[1]


@pytest.fixture(params=["site_a", "site_b"])
async def plant(request):
    profile = load_profile(f"profiles/{request.param}.yaml").model_copy(update={"noise": 0})
    simulator = Simulator(profile, random.Random(1))
    port = free_port()
    await simulator.start("127.0.0.1", port)
    client = AsyncModbusTcpClient("127.0.0.1", port=port)
    await client.connect()
    yield simulator, client
    client.close()
    await simulator.stop()


async def read(client, profile, name):
    register = profile.register(name)
    result = await client.read_holding_registers(
        register.address, count=register.words, device_id=profile.device_id
    )
    assert not result.isError()
    return decode(list(result.registers), register.scale)


async def write(client, profile, name, value):
    register = profile.register(name)
    result = await client.write_registers(
        register.address,
        encode(value, register.scale, register.words),
        device_id=profile.device_id,
    )
    assert not result.isError()


async def test_client_reads_scaled_values(plant):
    simulator, client = plant
    profile = simulator.profile
    assert await read(client, profile, "chw_setpoint") == pytest.approx(
        profile.defaults.chw_setpoint
    )
    assert await read(client, profile, "total_power") > 0
    assert await read(client, profile, "chw_supply_temp") == pytest.approx(
        profile.defaults.chw_setpoint, abs=0.1
    )


async def test_writing_setpoints_changes_power(plant):
    simulator, client = plant
    profile = simulator.profile
    before_chiller = await read(client, profile, "chiller_power")
    before_fan = await read(client, profile, "ct_fan_power")

    await write(client, profile, "chw_setpoint", 9.0)
    await write(client, profile, "ct_fan_speed", 50)
    await simulator.tick(0)

    assert await read(client, profile, "ct_fan_power") == pytest.approx(before_fan / 8, rel=0.02)
    assert await read(client, profile, "chiller_power") != pytest.approx(before_chiller)


async def test_supply_temp_follows_setpoint_with_lag(plant):
    simulator, client = plant
    profile = simulator.profile
    start = await read(client, profile, "chw_supply_temp")

    await write(client, profile, "chw_setpoint", 9.0)
    await simulator.tick(1)
    after_one = await read(client, profile, "chw_supply_temp")
    for _ in range(60):
        await simulator.tick(1)
    settled = await read(client, profile, "chw_supply_temp")

    assert start < after_one < 9.0
    assert settled == pytest.approx(9.0, abs=0.1)


async def test_undefined_address_returns_modbus_error(plant):
    simulator, client = plant
    result = await client.read_holding_registers(5000, count=1, device_id=1)
    assert result.isError()


async def test_scale_error_fault_reports_ten_times_the_value(plant):
    simulator, client = plant
    profile = simulator.profile
    normal = await read(client, profile, "chw_supply_temp")

    await client.write_coil(profile.faults.scale_error, True, device_id=profile.device_id)
    await simulator.tick(0)

    assert await read(client, profile, "chw_supply_temp") == pytest.approx(normal * 10, rel=0.01)


async def test_sensor_stuck_fault_freezes_the_reading(plant):
    simulator, client = plant
    profile = simulator.profile
    frozen = await read(client, profile, "chw_supply_temp")

    await client.write_coil(profile.faults.sensor_stuck, True, device_id=profile.device_id)
    await write(client, profile, "chw_setpoint", 9.5)
    for _ in range(30):
        await simulator.tick(1)

    assert await read(client, profile, "chw_supply_temp") == pytest.approx(frozen)
    assert simulator.chw_supply_temp == pytest.approx(9.5, abs=0.1)
