"""Full retrieval orchestration: expand → (hybrid|vector) → fuse → threshold → rerank → top-k.

This is the single entry point the agent's retrieve tool calls.
"""

from sqlalchemy.orm import Session

from app.retrieval.hybrid import hybrid_search
from app.retrieval.query_expand import expand
from app.retrieval.rerank import rerank
from app.retrieval.vectorstore import vector_search
from app.schemas import RagConfig, RetrievedBlock


def _dedupe(blocks: list[RetrievedBlock]) -> list[RetrievedBlock]:
    seen: dict[str, RetrievedBlock] = {}
    for b in blocks:
        if b.chunk_id not in seen:
            seen[b.chunk_id] = b
    return list(seen.values())


def retrieve(session: Session, query: str, config: RagConfig) -> tuple[list[RetrievedBlock], list[str]]:
    queries = expand(query, config.use_hyde, config.use_multi_query)

    pooled: list[RetrievedBlock] = []
    for q in queries:
        if config.use_hybrid:
            pooled.extend(hybrid_search(session, q, config.top_k))
        else:
            pooled.extend(vector_search(session, q, config.top_k))

    blocks = _dedupe(pooled)

    # similarity threshold gate (vector_score present on dense hits)
    blocks = [
        b for b in blocks if b.vector_score is None or b.vector_score >= config.similarity_threshold
    ]

    if config.use_rerank:
        blocks = rerank(query, blocks, config.final_k)
    else:
        key = "rrf_score" if config.use_hybrid else "vector_score"
        blocks = sorted(blocks, key=lambda b: getattr(b, key) or 0.0, reverse=True)[: config.final_k]

    return blocks, queries
