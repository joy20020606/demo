from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    database_url: str = Field(
        default="postgresql+psycopg://postgres:postgres@localhost:5432/knowledge_agent",
        description="SQLAlchemy psycopg3 connection string",
    )

    openai_api_key: str = Field(default="", description="Embeddings provider")
    embedding_model: str = "text-embedding-3-small"
    embedding_dim: int = 1536

    anthropic_api_key: str = Field(default="", description="Generation / agent LLM")
    anthropic_model: str = "claude-sonnet-4-6"

    # RAG tunables (overridable per-request via RagConfig)
    top_k: int = 20
    final_k: int = 3
    similarity_threshold: float = 0.2
    chunk_size: int = 800
    chunk_overlap: int = 120

    # Feature flags
    use_rerank: bool = False  # local cross-encoder pulls torch; off by default
    rerank_model: str = "BAAI/bge-reranker-base"
    use_hyde: bool = False
    use_multi_query: bool = False

    # LangSmith tracing (optional)
    langchain_tracing_v2: bool = False
    langchain_api_key: str = ""

    cors_origins: str = "http://localhost:3000"

    @property
    def cors_origin_list(self) -> list[str]:
        return [o.strip() for o in self.cors_origins.split(",") if o.strip()]


@lru_cache
def get_settings() -> Settings:
    return Settings()
