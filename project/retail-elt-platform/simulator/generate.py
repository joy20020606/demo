"""Source 系統模擬器:每次呼叫 generate_orders() 灌 N 筆假訂單到 raw schema。

第一次跑會自動 seed customers 與 products,後續每天只灌訂單。
production 上線後這支會被換成「接真實 Shopify webhook / Kafka」——它就是 demo 用的資料源 stub。
"""

from __future__ import annotations

import os
import random
import uuid
from datetime import date, datetime, timedelta

import psycopg
from faker import Faker

fake = Faker("zh_TW")

NUM_CUSTOMERS = 100
NUM_PRODUCTS = 50
CITIES = ["台北", "新北", "台中", "台南", "高雄", "桃園", "新竹", "台東"]
CATEGORIES = ["3C", "居家", "服飾", "美妝", "食品", "書籍"]
STATUS_WEIGHTS = [("paid", 70), ("shipped", 25), ("returned", 5)]


def _dsn() -> str:
    return os.environ.get(
        "WAREHOUSE_DSN",
        "host=postgres port=5432 dbname=warehouse user=warehouse password=warehouse",
    )


def _seed_dimensions(conn: psycopg.Connection) -> None:
    """第一次呼叫時把 customers / products 灌好;之後 ON CONFLICT 跳過。"""
    with conn.cursor() as cur:
        cur.execute("SELECT COUNT(*) FROM raw.customers")
        existing_customers = cur.fetchone()[0]
        if existing_customers >= NUM_CUSTOMERS:
            return

        for _ in range(NUM_CUSTOMERS):
            cur.execute(
                """
                INSERT INTO raw.customers (customer_id, name, email, city, created_at, updated_at)
                VALUES (%s, %s, %s, %s, %s, %s)
                ON CONFLICT (customer_id) DO NOTHING
                """,
                (
                    uuid.uuid4(),
                    fake.name(),
                    fake.email(),
                    random.choice(CITIES),
                    fake.date_time_between(start_date="-2y"),
                    datetime.now(),
                ),
            )

        for _ in range(NUM_PRODUCTS):
            cur.execute(
                """
                INSERT INTO raw.products (product_id, name, category, list_price_cents, is_active, created_at)
                VALUES (%s, %s, %s, %s, %s, %s)
                ON CONFLICT (product_id) DO NOTHING
                """,
                (
                    uuid.uuid4(),
                    fake.catch_phrase(),
                    random.choice(CATEGORIES),
                    random.randint(10000, 200000),
                    random.random() > 0.05,
                    fake.date_time_between(start_date="-1y"),
                ),
            )
    conn.commit()


def _load_dimension_ids(conn: psycopg.Connection) -> tuple[list, list]:
    with conn.cursor() as cur:
        cur.execute("SELECT customer_id FROM raw.customers")
        customers = [row[0] for row in cur.fetchall()]
        cur.execute("SELECT product_id FROM raw.products WHERE is_active = TRUE")
        products = [row[0] for row in cur.fetchall()]
    return customers, products


def _pick_status() -> str:
    statuses, weights = zip(*STATUS_WEIGHTS)
    return random.choices(statuses, weights=weights, k=1)[0]


def generate_orders(target_date: date, count: int = 100) -> int:
    """灌 N 筆訂單,回傳實際插入筆數。

    target_date: 訂單的 created_at 會落在這天的 00:00 ~ 24:00 之間
    count: 要產生幾筆
    """
    with psycopg.connect(_dsn()) as conn:
        _seed_dimensions(conn)
        customers, products = _load_dimension_ids(conn)

        if not customers or not products:
            raise RuntimeError("Dimensions are empty — seed failed?")

        day_start = datetime.combine(target_date, datetime.min.time())
        day_end = day_start + timedelta(days=1)

        inserted = 0
        with conn.cursor() as cur:
            for _ in range(count):
                cur.execute(
                    """
                    INSERT INTO raw.orders (order_id, customer_id, product_id, amount_cents, status, created_at)
                    VALUES (%s, %s, %s, %s, %s, %s)
                    """,
                    (
                        uuid.uuid4(),
                        random.choice(customers),
                        random.choice(products),
                        random.randint(10000, 500000),
                        _pick_status(),
                        fake.date_time_between(start_date=day_start, end_date=day_end),
                    ),
                )
                inserted += 1
        conn.commit()

    print(f"[simulator] inserted {inserted} orders for {target_date}")
    return inserted


if __name__ == "__main__":
    generate_orders(date.today(), count=100)
