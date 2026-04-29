import clickhouse_connect
from app.core.config import settings


def get_clickhouse_client():
    """ClickHouse 客戶端(同步,但操作很快)"""
    return clickhouse_connect.get_client(
        host=settings.CLICKHOUSE_HOST,
        port=settings.CLICKHOUSE_PORT,
        username=settings.CLICKHOUSE_USER,
        password=settings.CLICKHOUSE_PASSWORD,
        database=settings.CLICKHOUSE_DB,
    )
