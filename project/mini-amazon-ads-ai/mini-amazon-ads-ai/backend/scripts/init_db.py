"""初始化資料庫:建立 PostgreSQL 表 + ClickHouse 表"""
import asyncio
from app.core.database import engine, Base
from app.core.clickhouse import get_clickhouse_client
from app.models import Tenant, User, Campaign, Recommendation  # noqa: F401


async def init_postgres():
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.drop_all)
        await conn.run_sync(Base.metadata.create_all)
    print("✅ PostgreSQL 表已建立")


def init_clickhouse():
    ch = get_clickhouse_client()

    ch.command("DROP TABLE IF EXISTS ad_metrics_hourly")
    ch.command("""
        CREATE TABLE ad_metrics_hourly (
            tenant_id UUID,
            campaign_id UUID,
            asin String,
            hour DateTime,
            impressions UInt64,
            clicks UInt32,
            spend Decimal(10, 2),
            sales Decimal(10, 2),
            orders UInt32
        )
        ENGINE = MergeTree()
        PARTITION BY toYYYYMM(hour)
        ORDER BY (tenant_id, campaign_id, hour)
    """)
    print("✅ ClickHouse 表已建立")


async def main():
    await init_postgres()
    init_clickhouse()


if __name__ == "__main__":
    asyncio.run(main())