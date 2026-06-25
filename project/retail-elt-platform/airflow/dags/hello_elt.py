"""Week 1 Day 1:hello world DAG。

目的:證明整條鏈通了。
  1. simulator 灌 100 筆假訂單進 raw.orders
  2. dbt run --select stg_orders
  3. print 一行成功訊息

之後 Week 2 會擴成 daily_elt(完整 stg → marts → metrics + tests)。
"""

from __future__ import annotations

import sys
from datetime import date, datetime

from airflow.decorators import dag, task


@dag(
    dag_id="hello_elt",
    description="Week 1 Day 1: 證明 simulator → dbt → analytics 整條鏈通了",
    schedule=None,  # 手動觸發
    start_date=datetime(2026, 6, 1),
    catchup=False,
    tags=["week1", "demo"],
)
def hello_elt():

    @task
    def simulate_orders() -> int:
        """灌 100 筆假訂單到 raw.orders。"""
        sys.path.insert(0, "/opt/simulator")
        from generate import generate_orders  # noqa: WPS433

        return generate_orders(date.today(), count=100)

    @task.bash
    def dbt_run() -> str:
        return (
            "/home/airflow/dbt_venv/bin/dbt run "
            "--project-dir /opt/dbt "
            "--profiles-dir /opt/dbt "
            "--select stg_orders"
        )

    @task
    def hello(inserted_count: int) -> None:
        print(f"Hello ELT! 灌了 {inserted_count} 筆訂單,stg_orders 也跑完了。")

    n = simulate_orders()
    n >> dbt_run() >> hello(n)


dag = hello_elt()
