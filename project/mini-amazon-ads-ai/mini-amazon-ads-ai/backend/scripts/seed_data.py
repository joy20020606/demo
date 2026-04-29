"""灌測試資料:1 個 tenant + 1 個 user + 3 個 campaign + 30 天廣告數據"""
import asyncio
import random
import uuid
from datetime import datetime, timedelta
from decimal import Decimal

from app.core.database import SessionLocal
from app.core.clickhouse import get_clickhouse_client
from app.core.security import hash_password
from app.models import Tenant, User, Campaign


async def seed_postgres():
    async with SessionLocal() as db:
        tenant = Tenant(name="Joy's Brand")
        db.add(tenant)
        await db.flush()

        user = User(
            tenant_id=tenant.id,
            email="joy@test.com",
            password_hash=hash_password("password123"),
        )
        db.add(user)

        campaigns = [
            Campaign(
                tenant_id=tenant.id,
                name="Wireless Earbuds - SP",
                type="SP",
                state="enabled",
                daily_budget=Decimal("100.00"),
                target_acos=Decimal("25.00"),
            ),
            Campaign(
                tenant_id=tenant.id,
                name="Bluetooth Speaker - SP",
                type="SP",
                state="enabled",
                daily_budget=Decimal("80.00"),
                target_acos=Decimal("30.00"),
            ),
            Campaign(
                tenant_id=tenant.id,
                name="Phone Case - SD",
                type="SD",
                state="enabled",
                daily_budget=Decimal("50.00"),
                target_acos=Decimal("20.00"),
            ),
        ]
        for c in campaigns:
            db.add(c)
        await db.flush()

        await db.commit()

        print(f"✅ Tenant: {tenant.id}")
        print(f"✅ User:   joy@test.com / password123")
        for c in campaigns:
            print(f"✅ Campaign: {c.name} ({c.id})")

        return str(tenant.id), [(str(c.id), c.name, float(c.target_acos)) for c in campaigns]


def seed_clickhouse(tenant_id: str, campaigns: list):
    """為每個 campaign 灌 30 天 × 24 小時 = 720 筆數據"""
    ch = get_clickhouse_client()
    rows = []

    now = datetime.utcnow().replace(minute=0, second=0, microsecond=0)
    for cid, name, target_acos in campaigns:
        # 不同 campaign 表現不同:第一個健康、第二個 ACOS 飆高、第三個一般
        if "Earbuds" in name:
            base_acos = target_acos * 0.9   # 表現好
        elif "Speaker" in name:
            base_acos = target_acos * 1.8   # ACOS 飆高 → AI 應該抓出來
        else:
            base_acos = target_acos * 1.1

        for h in range(30 * 24):
            hour = now - timedelta(hours=h)
            impressions = random.randint(500, 3000)
            ctr = random.uniform(0.005, 0.02)
            clicks = max(1, int(impressions * ctr))
            cpc = random.uniform(0.5, 2.0)
            spend = round(clicks * cpc, 2)

            # 後 7 天讓 Speaker 變更糟,模擬「最近異常」
            if "Speaker" in name and h < 7 * 24:
                acos = base_acos * random.uniform(1.3, 1.8)
            else:
                acos = base_acos * random.uniform(0.8, 1.2)

            sales = round(spend / (acos / 100), 2) if acos > 0 else 0
            orders = max(0, int(sales / 30))

            rows.append([
                tenant_id, cid, "B0TESTASIN", hour,
                impressions, clicks, spend, sales, orders
            ])

    ch.insert(
        "ad_metrics_hourly",
        rows,
        column_names=["tenant_id", "campaign_id", "asin", "hour",
                      "impressions", "clicks", "spend", "sales", "orders"],
    )
    print(f"✅ ClickHouse 灌入 {len(rows)} 筆廣告數據")


async def main():
    tenant_id, campaigns = await seed_postgres()
    seed_clickhouse(tenant_id, campaigns)
    print("\n🎉 種子資料完成!")
    print("   登入:joy@test.com / password123")


if __name__ == "__main__":
    asyncio.run(main())
