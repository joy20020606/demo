import pytest

from sim.model import compute, load_ratio
from sim.profile import load_profile

SITE_A = load_profile("profiles/site_a.yaml").model.to_params()
SITE_B = load_profile("profiles/site_b.yaml").model.to_params()

SETPOINTS = [x / 10 for x in range(60, 101, 5)]
FAN_SPEEDS = list(range(30, 101, 5))


def best_settings(params, hour):
    return min(
        (compute(params, hour, sp, fan).total_power_kw, sp, fan)
        for sp in SETPOINTS
        for fan in FAN_SPEEDS
    )


def test_higher_chw_setpoint_lowers_chiller_power():
    low = compute(SITE_A, 14, 6.0, 100)
    high = compute(SITE_A, 14, 9.0, 100)
    assert high.chiller_power_kw < low.chiller_power_kw


def test_higher_chw_setpoint_raises_pump_power():
    low = compute(SITE_A, 14, 6.0, 100)
    high = compute(SITE_A, 14, 9.0, 100)
    assert high.chw_pump_power_kw > low.chw_pump_power_kw


def test_fan_power_follows_cube_law():
    full = compute(SITE_A, 14, 7.0, 100)
    half = compute(SITE_A, 14, 7.0, 50)
    assert half.ct_fan_power_kw == pytest.approx(full.ct_fan_power_kw / 8)


def test_slower_fan_warms_condenser_water_and_costs_chiller_power():
    fast = compute(SITE_A, 14, 7.0, 100)
    slow = compute(SITE_A, 14, 7.0, 50)
    assert slow.cw_supply_temp > fast.cw_supply_temp
    assert slow.chiller_power_kw > fast.chiller_power_kw


def test_total_power_has_interior_minimum_over_fan_speed():
    totals = [compute(SITE_A, 14, 7.0, fan).total_power_kw for fan in FAN_SPEEDS]
    best = totals.index(min(totals))
    assert 0 < best < len(totals) - 1


def test_load_is_higher_in_the_afternoon_than_at_night():
    assert load_ratio(SITE_A, 14) > load_ratio(SITE_A, 3)


def test_optimum_moves_with_load():
    _, night_setpoint, night_fan = best_settings(SITE_A, 3)
    _, day_setpoint, day_fan = best_settings(SITE_A, 14)
    assert night_setpoint > day_setpoint
    assert night_fan < day_fan


def test_sites_have_different_optimum():
    assert best_settings(SITE_A, 9)[1:] != best_settings(SITE_B, 9)[1:]


def test_out_of_range_setpoints_are_clamped_by_the_device():
    assert compute(SITE_A, 14, 99, 100) == compute(SITE_A, 14, 15, 100)
    assert compute(SITE_A, 14, 7, 0) == compute(SITE_A, 14, 7, 20)
