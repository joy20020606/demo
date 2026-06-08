from fastapi import APIRouter, Depends, File, Form, HTTPException, UploadFile
from sqlalchemy import func
from sqlalchemy.orm import Session

from app.db.engine import get_session
from app.db.models import Chunk, Document
from app.ingestion.pipeline import DuplicateDocumentError, ingest
from app.schemas import DocumentOut, IngestResponse

router = APIRouter(prefix="/documents", tags=["documents"])


@router.post("", response_model=IngestResponse)
async def upload_document(
    file: UploadFile = File(...),
    method: str = Form("fixed"),
    session: Session = Depends(get_session),
) -> IngestResponse:
    data = await file.read()
    if not data:
        raise HTTPException(400, "Empty file.")
    try:
        doc, n = ingest(session, file.filename or "upload", data, method=method)
    except DuplicateDocumentError as e:
        # 409 Conflict — semantically correct for "this resource already exists"
        raise HTTPException(409, str(e)) from e
    except ValueError as e:
        raise HTTPException(422, str(e)) from e
    return IngestResponse(
        document=DocumentOut(
            id=str(doc.id),
            title=doc.title,
            author=doc.author,
            source_type=doc.source_type,
            source_ref=doc.source_ref,
            chunk_count=n,
        ),
        chunks_created=n,
        chunk_method=method,
    )


@router.get("", response_model=list[DocumentOut])
def list_documents(session: Session = Depends(get_session)) -> list[DocumentOut]:
    rows = (
        session.query(Document, func.count(Chunk.id))
        .outerjoin(Chunk, Chunk.document_id == Document.id)
        .group_by(Document.id)
        .all()
    )
    return [
        DocumentOut(
            id=str(d.id),
            title=d.title,
            author=d.author,
            source_type=d.source_type,
            source_ref=d.source_ref,
            chunk_count=n,
        )
        for d, n in rows
    ]
