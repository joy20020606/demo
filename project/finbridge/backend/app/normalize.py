import json
from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal

from lxml import etree

from app.db.models import TransactionType

CAMT_NS = {"c": "urn:iso:std:iso:20022:tech:xsd:camt.053.001.08"}
CAMT_URI = CAMT_NS["c"]


@dataclass
class CanonicalTxn:
    source_message_id: str
    account_id: str
    symbol: str | None
    txn_type: TransactionType
    quantity: Decimal
    price: Decimal
    amount: Decimal
    currency: str
    trade_date: datetime


def _parse_date(value: str) -> datetime:
    return datetime.fromisoformat(value)


def normalize_trade(row: dict, account_default: str = "") -> CanonicalTxn:
    quantity = Decimal(str(row["quantity"]))
    price = Decimal(str(row["price"]))
    return CanonicalTxn(
        source_message_id=str(row["message_id"]),
        account_id=str(row.get("account_id") or account_default),
        symbol=str(row["symbol"]),
        txn_type=TransactionType(str(row["side"])),
        quantity=quantity,
        price=price,
        amount=quantity * price,
        currency=str(row.get("currency") or "USD"),
        trade_date=_parse_date(str(row["trade_date"])),
    )


def _classify_bank_entry(ustrd: str, cdt_dbt: str) -> TransactionType:
    text = ustrd.lower()
    if "dividend" in text:
        return TransactionType.dividend
    if "fee" in text or "custody" in text:
        return TransactionType.fee
    if cdt_dbt == "CRDT":
        return TransactionType.transfer
    return TransactionType.fee


def normalize_camt_entry(ntry, account_default: str = "BANK") -> CanonicalTxn:
    ref = ntry.findtext("c:NtryRef", namespaces=CAMT_NS)
    amt_el = ntry.find("c:Amt", CAMT_NS)
    amount = Decimal(amt_el.text)
    currency = amt_el.get("Ccy")
    cdt_dbt = ntry.findtext("c:CdtDbtInd", namespaces=CAMT_NS)
    ustrd = ntry.findtext(
        "c:NtryDtls/c:TxDtls/c:RmtInf/c:Ustrd", default="", namespaces=CAMT_NS
    )
    booking = ntry.findtext("c:BookgDt/c:Dt", namespaces=CAMT_NS)
    return CanonicalTxn(
        source_message_id=ref,
        account_id=account_default,
        symbol=None,
        txn_type=_classify_bank_entry(ustrd or "", cdt_dbt or ""),
        quantity=Decimal("0"),
        price=Decimal("0"),
        amount=amount,
        currency=currency or "USD",
        trade_date=_parse_date(booking),
    )


def parse_camt053(xml_bytes: bytes, account_default: str = "BANK") -> list[CanonicalTxn]:
    root = etree.fromstring(xml_bytes)
    return [
        normalize_camt_entry(ntry, account_default)
        for ntry in root.findall(".//c:Stmt/c:Ntry", CAMT_NS)
    ]


def normalize_dead_letter_payload(connector: str, raw_payload: str) -> CanonicalTxn:
    if connector == "bank_xml":
        ntry = etree.fromstring(raw_payload.encode())
        return normalize_camt_entry(ntry, account_default="ACME-BANK-01")
    return normalize_trade(json.loads(raw_payload))
