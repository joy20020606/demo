"""Cross-encoder re-ranking (optional).

A cross-encoder scores (query, passage) jointly — far more accurate than the
bi-encoder retrieval scores, at higher cost. Gated behind `use_rerank` because
sentence-transformers pulls torch (~1-2GB image). This toggle IS the
precision↔cost tradeoff the eval dashboard visualises.
"""

from functools import lru_cache

from app.config import get_settings
from app.schemas import RetrievedBlock

_s = get_settings()


@lru_cache
def _model():
    from sentence_transformers import CrossEncoder  # imported lazily; heavy dep

    return CrossEncoder(_s.rerank_model)


def rerank(query: str, blocks: list[RetrievedBlock], top_n: int) -> list[RetrievedBlock]:
    if not blocks:
        return []
    model = _model()
    scores = model.predict([(query, b.content) for b in blocks])
    for b, s in zip(blocks, scores):
        b.rerank_score = float(s)
    ranked = sorted(blocks, key=lambda b: b.rerank_score or 0.0, reverse=True)
    return ranked[:top_n]
