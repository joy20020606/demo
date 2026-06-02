"""Sparse keyword retrieval over Postgres tsvector (ts_rank_cd, GIN index).

Conceptually the BM25 half of hybrid search; tsvector is the zero-infra,
production-friendly choice on Railway.
"""

from sqlalchemy import text
from sqlalchemy.orm import Session

from app.schemas import RetrievedBlock

_SQL = text(
    """
    SELECT c.id::text AS chunk_id, c.document_id::text AS document_id, c.content,
           c.theory_tag, c.page_no,
           d.title, d.author, d.source_ref,
           ts_rank_cd(c.ts, q) AS keyword_score
    FROM chunks c
    JOIN documents d ON d.id = c.document_id,
         websearch_to_tsquery('english', :query) q
    WHERE c.ts @@ q
    ORDER BY keyword_score DESC
    LIMIT :k
    """
)


def keyword_search(session: Session, query: str, k: int) -> list[RetrievedBlock]:
    rows = session.execute(_SQL, {"query": query, "k": k}).mappings().all()
    return [
        RetrievedBlock(
            chunk_id=r["chunk_id"],
            document_id=r["document_id"],
            content=r["content"],
            title=r["title"],
            author=r["author"],
            source_ref=r["source_ref"],
            theory_tag=r["theory_tag"],
            page_no=r["page_no"],
            keyword_score=float(r["keyword_score"]),
        )
        for r in rows
    ]
