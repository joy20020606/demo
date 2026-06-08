"""load → chunk → (theory-tag) → embed → upsert."""

import hashlib
import json
import logging

from sqlalchemy.orm import Session

logger = logging.getLogger(__name__)


class DuplicateDocumentError(ValueError):
    """Raised when an identical document (same SHA256) has already been ingested."""

    def __init__(self, existing_title: str):
        super().__init__(f"Document already ingested: {existing_title!r}")
        self.existing_title = existing_title

from app.db.models import Chunk as ChunkRow
from app.db.models import Document
from app.ingestion.chunker import chunk as chunk_text
from app.ingestion.loader import load
from app.llm import complete, embed_texts

_META_SYSTEM = (
    "You extract bibliographic metadata from the first page of a document. "
    'Return strict JSON: {"title": str, "author": str|null, '
    '"source_type": "paper"|"book"|"research", "theory": str|null}. '
    "theory = the central theory/concept/framework the work is about, if identifiable."
)


def _extract_metadata(first_page_text: str, fallback_title: str) -> dict:
    try:
        raw = complete(_META_SYSTEM, first_page_text[:3000], max_tokens=300, temperature=0.0)
        raw = raw[raw.find("{") : raw.rfind("}") + 1]
        data = json.loads(raw)
    except Exception as exc:  # noqa: BLE001 — degrade gracefully but never silently
        logger.warning(
            "metadata extraction failed (%s: %s); falling back to filename/defaults",
            type(exc).__name__,
            exc,
        )
        data = {}
    theory = data.get("theory")
    return {
        "title": (data.get("title") or fallback_title)[:512],
        "author": (data.get("author") or None) and data["author"][:256],
        "source_type": (data.get("source_type") or "paper")[:32],
        "theory": theory[:500] if theory else None,
    }


def ingest(session: Session, filename: str, data: bytes, method: str = "fixed") -> tuple[Document, int]:
    # Dedup by content SHA256 — same bytes = same document, regardless of filename.
    content_hash = hashlib.sha256(data).hexdigest()
    existing = session.query(Document).filter_by(content_hash=content_hash).first()
    if existing:
        raise DuplicateDocumentError(existing.title)

    pages = load(filename, data)
    if not pages:
        raise ValueError("No extractable text in upload.")

    meta = _extract_metadata(pages[0].text, fallback_title=filename)

    doc = Document(
        title=meta["title"],
        author=meta["author"],
        source_type=meta["source_type"],
        source_ref=filename,
        content_hash=content_hash,
    )
    session.add(doc)
    session.flush()  # assign doc.id

    chunks = chunk_text(pages, method=method)
    if not chunks:
        raise ValueError("Chunking produced no content.")

    embeddings = embed_texts([c.content for c in chunks])

    rows = [
        ChunkRow(
            document_id=doc.id,
            content=c.content,
            chunk_index=c.index,
            chunk_method=c.method,
            theory_tag=meta["theory"],
            page_no=c.page_no,
            token_count=c.token_count,
            embedding=emb,
        )
        for c, emb in zip(chunks, embeddings)
    ]
    session.add_all(rows)
    session.commit()
    return doc, len(rows)
