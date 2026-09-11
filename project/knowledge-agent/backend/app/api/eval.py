import json
import logging
from pathlib import Path

from fastapi import APIRouter, BackgroundTasks

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/eval", tags=["eval"])

_EMPTY: dict = {"runs": [], "n_questions": 0}
_RESULTS = Path(__file__).resolve().parent.parent / "eval" / "results.json"


@router.get("/results")
def get_results() -> dict:
    if not _RESULTS.exists():
        return _EMPTY
    try:
        text = _RESULTS.read_text(encoding="utf-8").strip()
        if not text:
            return _EMPTY
        return json.loads(text)
    except json.JSONDecodeError as exc:
        logger.warning("results.json is corrupt (%s); returning empty payload", exc)
        return _EMPTY


@router.post("/run")
def run_eval(background: BackgroundTasks) -> dict:
    from app.eval.run_ragas import run

    background.add_task(run)
    return {"status": "started"}
