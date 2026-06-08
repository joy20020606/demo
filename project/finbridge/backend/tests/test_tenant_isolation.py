import uuid

from fastapi.testclient import TestClient
from sqlalchemy import select

from app.db.engine import SessionLocal
from app.db.models import Instrument, InstrumentType, Tenant
from app.main import app

client = TestClient(app)


def _get_or_create_tenant(session, slug: str, name: str) -> Tenant:
    tenant = session.scalar(select(Tenant).where(Tenant.slug == slug))
    if tenant is None:
        tenant = Tenant(slug=slug, name=name)
        session.add(tenant)
        session.flush()
    return tenant


def _ensure_instrument(session, tenant_id, symbol: str) -> None:
    exists = session.scalar(
        select(Instrument).where(
            Instrument.tenant_id == tenant_id, Instrument.symbol == symbol
        )
    )
    if exists is None:
        session.add(
            Instrument(
                tenant_id=tenant_id,
                symbol=symbol,
                isin=None,
                instrument_type=InstrumentType.equity,
                currency="USD",
                name=f"{symbol} Test",
            )
        )


def _seed_isolation() -> str:
    marker = uuid.uuid4().hex[:8].upper()
    acme_symbol = f"ACME{marker}"
    globex_symbol = f"GLBX{marker}"
    session = SessionLocal()
    try:
        acme = _get_or_create_tenant(session, "acme", "Acme Capital")
        globex = _get_or_create_tenant(session, "globex", "Globex Asset Management")
        _ensure_instrument(session, acme.id, acme_symbol)
        _ensure_instrument(session, globex.id, globex_symbol)
        session.commit()
    finally:
        session.close()
    return marker


def test_tenant_isolation_no_cross_leakage():
    marker = _seed_isolation()
    acme_symbol = f"ACME{marker}"
    globex_symbol = f"GLBX{marker}"

    acme_res = client.get("/instruments", headers={"X-Tenant": "acme"})
    assert acme_res.status_code == 200
    acme_symbols = {row["symbol"] for row in acme_res.json()}

    globex_res = client.get("/instruments", headers={"X-Tenant": "globex"})
    assert globex_res.status_code == 200
    globex_symbols = {row["symbol"] for row in globex_res.json()}

    assert acme_symbol in acme_symbols
    assert globex_symbol in globex_symbols
    assert globex_symbol not in acme_symbols
    assert acme_symbol not in globex_symbols
    assert not (globex_symbols & {acme_symbol})


def test_missing_tenant_header_404():
    res = client.get("/instruments")
    assert res.status_code == 404


def test_unknown_tenant_404():
    res = client.get("/instruments", headers={"X-Tenant": "does-not-exist"})
    assert res.status_code == 404
