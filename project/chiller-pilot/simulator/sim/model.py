import math
from dataclasses import dataclass

DESIGN_CHW_SETPOINT = 7.0
DESIGN_CHW_RETURN = 12.5
DESIGN_CW_TEMP = 30.0
CHW_SETPOINT_RANGE = (4.0, 15.0)
FAN_SPEED_RANGE = (20.0, 100.0)


@dataclass(frozen=True)
class ModelParams:
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


@dataclass(frozen=True)
class PlantState:
    outdoor_temp: float
    cooling_load_kw: float
    cw_supply_temp: float
    chw_return_temp: float
    chiller_power_kw: float
    chw_pump_power_kw: float
    ct_fan_power_kw: float

    @property
    def total_power_kw(self) -> float:
        return self.chiller_power_kw + self.chw_pump_power_kw + self.ct_fan_power_kw


def clamp(value: float, low: float, high: float) -> float:
    return max(low, min(high, value))


def outdoor_temp(params: ModelParams, hour: float) -> float:
    return params.outdoor_mean_degc + params.outdoor_swing_degc * math.sin(
        2 * math.pi * (hour - 9) / 24
    )


def occupancy(hour: float) -> float:
    if not 7 <= hour <= 21:
        return 0.0
    return math.sin(math.pi * (hour - 7) / 14)


def load_ratio(params: ModelParams, hour: float) -> float:
    weather = 0.03 * (outdoor_temp(params, hour) - params.outdoor_mean_degc)
    return clamp(0.25 + 0.45 * occupancy(hour) + weather, 0.15, 1.0)


def compute(
    params: ModelParams, hour: float, chw_setpoint: float, fan_speed_pct: float
) -> PlantState:
    setpoint = clamp(chw_setpoint, *CHW_SETPOINT_RANGE)
    fan = clamp(fan_speed_pct, *FAN_SPEED_RANGE) / 100

    outdoor = outdoor_temp(params, hour)
    ratio = load_ratio(params, hour)
    load_kw = params.design_load_kw * ratio

    approach = params.approach_min_degc + params.approach_gain_degc * (1 / fan - 1)
    cw_temp = outdoor + approach

    cop = params.cop_ref
    cop *= 1 + params.cop_per_chw_degc * (setpoint - DESIGN_CHW_SETPOINT)
    cop *= 1 - params.cop_per_cw_degc * (cw_temp - DESIGN_CW_TEMP)
    cop = max(cop, 1.0)

    design_delta = DESIGN_CHW_RETURN - DESIGN_CHW_SETPOINT
    delta = max(DESIGN_CHW_RETURN - setpoint, 1.5)
    flow_ratio = clamp(ratio * design_delta / delta, 0.3, 1.2)

    return PlantState(
        outdoor_temp=outdoor,
        cooling_load_kw=load_kw,
        cw_supply_temp=cw_temp,
        chw_return_temp=DESIGN_CHW_RETURN,
        chiller_power_kw=load_kw / cop,
        chw_pump_power_kw=params.pump_rated_kw * flow_ratio**3,
        ct_fan_power_kw=params.fan_rated_kw * fan**3,
    )
