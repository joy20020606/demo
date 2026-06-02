"""Dense vector retrieval over pgvector (cosine distance, HNSW index)."""

from sqlalchemy import bindparam, text
from sqlalchemy.orm import Session

from app.llm import embed_query
from app.schemas import RetrievedBlock

# 1 - cosine_distance => cosine similarity in [0, 1-ish]
_SQL = text(
    """
    SELECT c.id::text AS chunk_id, c.document_id::text AS document_id, c.content,
           c.theory_tag, c.page_no,
           d.title, d.author, d.source_ref,
           1 - (c.embedding <=> :qvec) AS vector_score
    FROM chunks c
    JOIN documents d ON d.id = c.document_id
    WHERE c.embedding IS NOT NULL
    ORDER BY c.embedding <=> :qvec
    LIMIT :k
    """
).bindparams(bindparam("qvec"))


def vector_search(session: Session, query: str, k: int) -> list[RetrievedBlock]:
    qvec = embed_query(query)
    rows = session.execute(_SQL, {"qvec": str(qvec), "k": k}).mappings().all()
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
            vector_score=float(r["vector_score"]),
        )
        for r in rows
    ]
