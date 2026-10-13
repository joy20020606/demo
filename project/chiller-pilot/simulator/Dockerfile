FROM python:3.12-slim

WORKDIR /app

COPY pyproject.toml ./
COPY sim ./sim
RUN pip install --no-cache-dir .

COPY profiles ./profiles

ENV PYTHONUNBUFFERED=1 \
    SIM_HOST=0.0.0.0 \
    SIM_PORT=5020

EXPOSE 5020

CMD ["python", "-m", "sim"]
