from pathlib import Path

import pytest
from pydantic import ValidationError

from gw.config import SiteConfig, load_site_config

SITES = Path(__file__).resolve().parents[2] / "sites"


def base_config() -> dict:
    return {
        "site_id": "site_x",
        "protocol": "modbus_tcp",
        "host": "device",
        "points": [
            {"name": "chw_supply_temp", "address": 0, "scale": 0.1, "unit": "degC"},
            {"name": "chiller_power", "address": 9, "scale": 0.001, "unit": "kW", "type": "uint32"},
        ],
    }


@pytest.mark.parametrize("site", ["site_a", "site_b"])
def test_shipped_site_configs_are_valid(site):
    config = load_site_config(SITES / f"{site}.yaml")
    assert config.site_id == site


def test_both_sites_expose_the_same_point_names():
    names_a = {p.name for p in load_site_config(SITES / "site_a.yaml").points}
    names_b = {p.name for p in load_site_config(SITES / "site_b.yaml").points}
    assert names_a == names_b


def test_typo_in_key_is_rejected():
    data = base_config()
    data["points"][0]["scael"] = 0.1
    with pytest.raises(ValidationError, match="scael"):
        SiteConfig.model_validate(data)


def test_missing_scale_is_rejected():
    data = base_config()
    del data["points"][0]["scale"]
    with pytest.raises(ValidationError, match="scale"):
        SiteConfig.model_validate(data)


def test_zero_scale_is_rejected():
    data = base_config()
    data["points"][0]["scale"] = 0
    with pytest.raises(ValidationError):
        SiteConfig.model_validate(data)


def test_duplicate_point_name_is_rejected():
    data = base_config()
    data["points"][1]["name"] = "chw_supply_temp"
    with pytest.raises(ValidationError, match="duplicate point name"):
        SiteConfig.model_validate(data)


def test_uint32_overlapping_next_point_is_rejected():
    data = base_config()
    data["points"].append({"name": "pump_power", "address": 10, "scale": 0.1, "unit": "kW"})
    with pytest.raises(ValidationError, match="address 10"):
        SiteConfig.model_validate(data)


def test_unknown_protocol_is_rejected():
    data = base_config()
    data["protocol"] = "bacnet_ip"
    with pytest.raises(ValidationError):
        SiteConfig.model_validate(data)


def test_site_id_must_be_safe_for_topics():
    data = base_config()
    data["site_id"] = "site/+"
    with pytest.raises(ValidationError, match="site_id"):
        SiteConfig.model_validate(data)
