import json
from pathlib import Path

from fastapi import APIRouter, BackgroundTasks

router = APIRouter(prefix="/eval", tags=["eval"])

_RESULTS = Path(__file__).resolve().parent.parent / "eval" / "results.json"


@router.get("/results")
def get_results() -> dict:
    if _RESULTS.exists():
        return json.loads(_RESULTS.read_text(encoding="utf-8"))
    return {"runs": [], "n_questions": 0}


@router.post("/run")
def run_eval(background: BackgroundTasks) -> dict:
    from app.eval.run_ragas import run

    background.add_task(run)
    return {"status": "started"}
