from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from app.agent.graph import run_agent
from app.db.engine import get_session
from app.schemas import ChatRequest, ChatResponse

router = APIRouter(prefix="/chat", tags=["chat"])


@router.post("", response_model=ChatResponse)
def chat(req: ChatRequest, session: Session = Depends(get_session)) -> ChatResponse:
    return run_agent(session, req.query, req.config)
