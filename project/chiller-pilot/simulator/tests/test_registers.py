import pytest
from pydantic import ValidationError

from sim.profile import Profile, load_profile
from sim.registers import decode, encode


def test_single_word_round_trip():
    assert encode(7.2, 0.1, 1) == [72]
    assert decode([72], 0.1) == pytest.approx(7.2)


def test_double_word_is_high_word_first():
    assert encode(285.5, 0.001, 2) == [4, 23356]
    assert decode([4, 23356], 0.001) == pytest.approx(285.5)


def test_values_are_clipped_to_register_size():
    assert encode(99999, 0.1, 1) == [0xFFFF]
    assert encode(-5, 0.1, 1) == [0]


def test_both_profiles_load():
    assert load_profile("profiles/site_a.yaml").site_id == "site_a"
    assert load_profile("profiles/site_b.yaml").site_id == "site_b"


def test_overlapping_addresses_are_rejected():
    data = load_profile("profiles/site_b.yaml").model_dump()
    data["registers"][5]["address"] = 201
    with pytest.raises(ValidationError, match="address 201"):
        Profile.model_validate(data)


def test_unknown_keys_are_rejected():
    data = load_profile("profiles/site_a.yaml").model_dump()
    data["registers"][0]["scael"] = 0.1
    with pytest.raises(ValidationError):
        Profile.model_validate(data)
