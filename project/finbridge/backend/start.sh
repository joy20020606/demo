#!/bin/sh
if [ "$SERVICE_ROLE" = "mock" ]; then
  exec uvicorn app.mock_upstreams.app:app --host 0.0.0.0 --port "${PORT:-9001}"
fi
python -m app.db.init_db && python -m app.seed
exec uvicorn app.main:app --host 0.0.0.0 --port "${PORT:-8000}"
