import socket

import pytest
from pymodbus.server import ModbusTcpServer
from pymodbus.simulator import DataType, SimData, SimDevice

from gw.config import SiteConfig
from gw.readers import DeviceUnavailable, Reading, build_reader
from gw.readers.modbus import decode


def free_port() -> int:
    with socket.socket() as s:
        s.bind(("127.0.0.1", 0))
        return s.getsockname()[1]


def site_config(port: int, points: list[dict]) -> SiteConfig:
    return SiteConfig.model_validate(
        {
            "site_id": "site_x",
            "protocol": "modbus_tcp",
            "host": "127.0.0.1",
            "port": port,
            "points": points,
        }
    )


@pytest.fixture
async def device():
    port = free_port()
    holding = [
        SimData(address=0, values=[72], datatype=DataType.REGISTERS),
        SimData(address=9, values=[4, 23356], datatype=DataType.REGISTERS),
    ]
    unused_bits = [SimData(address=0, values=False, datatype=DataType.BITS)]
    unused_registers = [SimData(address=0, values=0, datatype=DataType.REGISTERS)]
    server = ModbusTcpServer(
        SimDevice(id=1, simdata=(unused_bits, unused_bits, holding, unused_registers)),
        address=("127.0.0.1", port),
    )
    await server.serve_forever(background=True)
    yield port
    await server.shutdown()


def test_decode_single_word():
    assert decode([72], 0.1) == pytest.approx(7.2)


def test_decode_two_words_high_word_first():
    assert decode([4, 23356], 0.001) == pytest.approx(285.5)


async def test_reads_and_scales_points(device):
    config = site_config(
        device,
        [
            {"name": "chw_supply_temp", "address": 0, "scale": 0.1, "unit": "degC"},
            {"name": "chiller_power", "address": 9, "scale": 0.001, "unit": "kW", "type": "uint32"},
        ],
    )
    reader = build_reader(config)
    try:
        assert await reader.read_all() == [
            Reading("chw_supply_temp", 7.2),
            Reading("chiller_power", 285.5),
        ]
    finally:
        await reader.close()


async def test_wrong_scale_in_point_table_is_not_an_error(device):
    config = site_config(
        device, [{"name": "chw_supply_temp", "address": 0, "scale": 1, "unit": "degC"}]
    )
    reader = build_reader(config)
    try:
        assert await reader.read_all() == [Reading("chw_supply_temp", 72.0)]
    finally:
        await reader.close()


async def test_bad_address_skips_that_point_only(device):
    config = site_config(
        device,
        [
            {"name": "missing", "address": 500, "scale": 1, "unit": "x"},
            {"name": "chw_supply_temp", "address": 0, "scale": 0.1, "unit": "degC"},
        ],
    )
    reader = build_reader(config)
    try:
        assert await reader.read_all() == [Reading("chw_supply_temp", 7.2)]
    finally:
        await reader.close()


async def test_unreachable_device_raises_device_unavailable():
    config = site_config(free_port(), [{"name": "x", "address": 0, "scale": 1, "unit": "x"}])
    reader = build_reader(config)
    try:
        with pytest.raises(DeviceUnavailable):
            await reader.read_all()
    finally:
        await reader.close()
