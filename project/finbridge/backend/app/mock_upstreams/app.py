from fastapi import FastAPI, Query

app = FastAPI(title="FinBridge Mock Trading Platform", version="0.1.0")

_TRADES: dict[str, list[dict]] = {
    "acme": [
        {
            "message_id": "ACME-TRD-0001",
            "account_id": "ACME-EQ-01",
            "symbol": "AAPL",
            "side": "buy",
            "quantity": "100",
            "price": "187.45",
            "currency": "USD",
            "trade_date": "2026-05-04",
        },
        {
            "message_id": "ACME-TRD-0002",
            "account_id": "ACME-EQ-01",
            "symbol": "AAPL",
            "side": "buy",
            "quantity": "50",
            "price": "190.10",
            "currency": "USD",
            "trade_date": "2026-05-06",
        },
        {
            "message_id": "ACME-TRD-0003",
            "account_id": "ACME-EQ-01",
            "symbol": "AAPL",
            "side": "sell",
            "quantity": "30",
            "price": "192.80",
            "currency": "USD",
            "trade_date": "2026-05-11",
        },
        {
            "message_id": "ACME-TRD-0004",
            "account_id": "ACME-EQ-01",
            "symbol": "MSFT",
            "side": "buy",
            "quantity": "75",
            "price": "421.30",
            "currency": "USD",
            "trade_date": "2026-05-07",
        },
        {
            "message_id": "ACME-TRD-0005",
            "account_id": "ACME-FX-02",
            "symbol": "TSLA",
            "side": "buy",
            "quantity": "40",
            "price": "245.66",
            "currency": "USD",
            "trade_date": "2026-05-12",
        },
    ],
    "globex": [
        {
            "message_id": "GLBX-TRD-1001",
            "account_id": "GLBX-EQ-01",
            "symbol": "NVDA",
            "side": "buy",
            "quantity": "20",
            "price": "1180.20",
            "currency": "USD",
            "trade_date": "2026-05-05",
        },
        {
            "message_id": "GLBX-TRD-1002",
            "account_id": "GLBX-EQ-01",
            "symbol": "NVDA",
            "side": "sell",
            "quantity": "5",
            "price": "1205.00",
            "currency": "USD",
            "trade_date": "2026-05-09",
        },
        {
            "message_id": "GLBX-TRD-1003",
            "account_id": "GLBX-BD-03",
            "symbol": "AMZN",
            "side": "buy",
            "quantity": "60",
            "price": "182.94",
            "currency": "USD",
            "trade_date": "2026-05-10",
        },
    ],
}

_DECLARED_POSITIONS: dict[str, list[dict]] = {
    "acme": [
        {"account_id": "ACME-EQ-01", "symbol": "AAPL", "quantity": "100"},
        {"account_id": "ACME-EQ-01", "symbol": "MSFT", "quantity": "75"},
        {"account_id": "ACME-FX-02", "symbol": "TSLA", "quantity": "40"},
    ],
    "globex": [
        {"account_id": "GLBX-EQ-01", "symbol": "NVDA", "quantity": "15"},
        {"account_id": "GLBX-BD-03", "symbol": "AMZN", "quantity": "60"},
    ],
}


@app.get("/health")
def health() -> dict:
    return {"status": "ok"}


@app.get("/trades")
def trades(tenant: str = Query(default="acme")) -> list[dict]:
    return _TRADES.get(tenant.lower(), [])


@app.get("/positions/declared")
def positions_declared(tenant: str = Query(default="acme")) -> list[dict]:
    return _DECLARED_POSITIONS.get(tenant.lower(), [])
