import httpx
import pytest

from app.connectors import rest_trading


def test_fetch_retries_exactly_three_times(monkeypatch):
    calls = {"count": 0}

    def fake_get(*args, **kwargs):
        calls["count"] += 1
        raise httpx.TransportError("simulated transient failure")

    monkeypatch.setattr(rest_trading.httpx, "get", fake_get)

    with pytest.raises(httpx.TransportError):
        rest_trading._fetch("acme")

    assert calls["count"] == 3


def test_fetch_does_not_retry_on_validation_error(monkeypatch):
    calls = {"count": 0}

    def fake_get(*args, **kwargs):
        calls["count"] += 1
        raise ValueError("deterministic failure")

    monkeypatch.setattr(rest_trading.httpx, "get", fake_get)

    with pytest.raises(ValueError):
        rest_trading._fetch("acme")

    assert calls["count"] == 1
