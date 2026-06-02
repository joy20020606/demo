"""Reciprocal Rank Fusion (RRF) of dense + sparse result lists.

RRF score for a doc d:  sum over lists L of  1 / (k + rank_L(d))
with k=60 (the standard constant). Rank-based fusion sidesteps the
incompatible score scales of cosine similarity vs ts_rank_cd.
"""

from sqlalchemy.orm import Session

from app.retrieval.keyword import keyword_search
from app.retrieval.vectorstore import vector_search
from app.schemas import RetrievedBlock

RRF_K = 60


def _fuse(
    vector_hits: list[RetrievedBlock],
    keyword_hits: list[RetrievedBlock],
) -> dict[str, RetrievedBlock]:
    merged: dict[str, RetrievedBlock] = {}

    def add(block: RetrievedBlock, rank: int):
        existing = merged.get(block.chunk_id)
        contribution = 1.0 / (RRF_K + rank)
        if existing is None:
            block.rrf_score = contribution
            merged[block.chunk_id] = block
        else:
            existing.rrf_score = (existing.rrf_score or 0.0) + contribution
            # carry over whichever per-stage score this list provides
            existing.vector_score = existing.vector_score or block.vector_score
            existing.keyword_score = existing.keyword_score or block.keyword_score

    for rank, b in enumerate(vector_hits, start=1):
        add(b, rank)
    for rank, b in enumerate(keyword_hits, start=1):
        add(b, rank)
    return merged


def hybrid_search(session: Session, query: str, k: int) -> list[RetrievedBlock]:
    vector_hits = vector_search(session, query, k)
    keyword_hits = keyword_search(session, query, k)
    merged = _fuse(vector_hits, keyword_hits)
    blocks = sorted(merged.values(), key=lambda b: b.rrf_score or 0.0, reverse=True)
    return blocks[:k]
