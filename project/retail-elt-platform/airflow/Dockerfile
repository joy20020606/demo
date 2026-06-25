FROM apache/airflow:2.10.4-python3.11

USER root
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl build-essential \
 && rm -rf /var/lib/apt/lists/*

USER airflow

COPY requirements.txt /tmp/requirements.txt
RUN pip install --no-cache-dir -r /tmp/requirements.txt

# dbt 跟 Airflow 有時候會在 pydantic / jsonschema 版本上打架,
# 所以把 dbt 裝在獨立 venv 裡,DAG 用絕對路徑呼叫它。
RUN python -m venv /home/airflow/dbt_venv \
 && /home/airflow/dbt_venv/bin/pip install --no-cache-dir \
        dbt-core==1.9.0 dbt-postgres==1.9.0

ENV DBT_BIN=/home/airflow/dbt_venv/bin/dbt
