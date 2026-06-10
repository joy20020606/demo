from decimal import Decimal
from pathlib import Path

from app.db.models import TransactionType
from app.normalize import parse_camt053

DATA_PATH = (
    Path(__file__).resolve().parent.parent
    / "app"
    / "mock_upstreams"
    / "data"
    / "bank_xml"
    / "camt053_acme.xml"
)


def _by_ref(entries):
    return {e.source_message_id: e for e in entries}


def test_dividend_entry_maps_correctly():
    entries = parse_camt053(DATA_PATH.read_bytes())
    by_ref = _by_ref(entries)

    div = by_ref["ACME-NTRY-0001"]
    assert div.amount == Decimal("1850.00")
    assert div.currency == "USD"
    assert div.txn_type == TransactionType.dividend


def test_custody_fee_entry_maps_to_fee():
    entries = parse_camt053(DATA_PATH.read_bytes())
    by_ref = _by_ref(entries)

    fee = by_ref["ACME-NTRY-0002"]
    assert fee.amount == Decimal("320.75")
    assert fee.currency == "USD"
    assert fee.txn_type == TransactionType.fee


def test_all_entries_parsed():
    entries = parse_camt053(DATA_PATH.read_bytes())
    assert len(entries) == 4
