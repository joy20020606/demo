"""Tools the agent can call. Each takes an open Session (bound per request).

Exposed as plain callables and also wrapped as LangChain tools so the same
functions can drive either a deterministic graph or a tool-calling agent.
"""

from langchain_core.tools import tool
from sqlalchemy import func, text
from sqlalchemy.orm import Session

from app.db.models import Chunk, Document
from app.retrieval.retriever import retrieve
from app.schemas import RagConfig, RetrievedBlock


def retrieve_knowledge(session: Session, query: str, config: RagConfig) -> tuple[list[RetrievedBlock], list[str]]:
    """Hybrid retrieve + rerank over the knowledge base. Returns scored blocks."""
    return retrieve(session, query, config)


def list_sources(session: Session) -> list[dict]:
    """Enumerate ingested documents so the agent knows what it has."""
    rows = (
        session.query(
            Document.title, Document.author, Document.source_type, func.count(Chunk.id)
        )
        .outerjoin(Chunk, Chunk.document_id == Document.id)
        .group_by(Document.id)
        .all()
    )
    return [
        {"title": t, "author": a, "source_type": st, "chunks": n} for t, a, st, n in rows
    ]


def lookup_author(session: Session, name: str) -> list[dict]:
    """Find what a given author/source claims (metadata-filtered chunk lookup)."""
    rows = session.execute(
        text(
            """
            SELECT d.title, d.author, c.content, c.theory_tag
            FROM chunks c JOIN documents d ON d.id = c.document_id
            WHERE d.author ILIKE :name
            LIMIT 5
            """
        ),
        {"name": f"%{name}%"},
    ).mappings().all()
    return [dict(r) for r in rows]


def build_langchain_tools(session: Session, config: RagConfig):
    """LangChain @tool wrappers (bound to this request's session/config)."""

    @tool
    def retrieve_knowledge_tool(query: str) -> str:
        """Search the knowledge base for passages relevant to `query`."""
        blocks, _ = retrieve_knowledge(session, query, config)
        return "\n\n".join(f"[{i+1}] {b.content}" for i, b in enumerate(blocks))

    @tool
    def list_sources_tool() -> str:
        """List all documents currently in the knowledge base."""
        return "\n".join(f"- {s['title']} ({s['author']})" for s in list_sources(session))

    @tool
    def lookup_author_tool(name: str) -> str:
        """Look up what a specific author or source claims."""
        return "\n".join(r["content"] for r in lookup_author(session, name))

    return [retrieve_knowledge_tool, list_sources_tool, lookup_author_tool]
