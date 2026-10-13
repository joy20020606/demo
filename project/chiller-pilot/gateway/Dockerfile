FROM python:3.12-slim

WORKDIR /app

COPY pyproject.toml ./
COPY gw ./gw
RUN pip install --no-cache-dir .

ENV PYTHONUNBUFFERED=1 \
    GW_SITE_CONFIG=/app/sites/site.yaml

CMD ["python", "-m", "gw"]
