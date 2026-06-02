from pydantic import BaseModel, Field

from app.config import get_settings

_s = get_settings()


class RagConfig(BaseModel):
    """Per-request tunables, surfaced live in the frontend RagConfigPanel."""

    top_k: int = _s.top_k
    final_k: int = _s.final_k
    similarity_threshold: float = _s.similarity_threshold
    use_hybrid: bool = True
    use_rerank: bool = _s.use_rerank
    use_hyde: bool = _s.use_hyde
    use_multi_query: bool = _s.use_multi_query


class RetrievedBlock(BaseModel):
    chunk_id: str
    document_id: str
    content: str
    title: str
    author: str | None = None
    source_ref: str | None = None
    theory_tag: str | None = None
    page_no: int | None = None
    # per-stage provenance scores (for the RetrievalInspector)
    vector_score: float | None = None
    keyword_score: float | None = None
    rrf_score: float | None = None
    rerank_score: float | None = None


class Citation(BaseModel):
    n: int
    chunk_id: str
    source: str
    author: str | None = None
    theory: str | None = None
    page_no: int | None = None


class ChatRequest(BaseModel):
    query: str = Field(min_length=1)
    config: RagConfig = RagConfig()


class ChatResponse(BaseModel):
    answer: str
    citations: list[Citation]
    blocks: list[RetrievedBlock]
    expanded_queries: list[str] = []
    latency_ms: int | None = None


class DocumentOut(BaseModel):
    id: str
    title: str
    author: str | None = None
    source_type: str | None = None
    source_ref: str | None = None
    chunk_count: int = 0


class IngestResponse(BaseModel):
    document: DocumentOut
    chunks_created: int
    chunk_method: str
