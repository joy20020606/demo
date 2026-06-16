from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    database_url: str = Field(
        default="postgresql+psycopg://postgres:postgres@localhost:5432/finbridge",
        description="SQLAlchemy psycopg3 connection string",
    )

    cors_origins: str = "http://localhost:3000"

    mock_upstreams_url: str = "http://mock_upstreams:9001"

    kafka_enabled: bool = False
    kafka_bootstrap: str = "redpanda:9092"
    kafka_topic: str = "finbridge.events"
    kafka_security_protocol: str = ""
    kafka_sasl_mechanism: str = ""
    kafka_sasl_username: str = ""
    kafka_sasl_password: str = ""

    @property
    def cors_origin_list(self) -> list[str]:
        return [o.strip() for o in self.cors_origins.split(",") if o.strip()]


@lru_cache
def get_settings() -> Settings:
    return Settings()
