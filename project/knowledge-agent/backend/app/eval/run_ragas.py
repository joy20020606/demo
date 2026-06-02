"""RAGAS evaluation across RAG config variants.

For each variant (vector-only, hybrid, hybrid+rerank, hybrid+rerank+HyDE) we run
the golden set through the live pipeline, then score with RAGAS:
faithfulness, answer_relevancy, context_precision, context_recall.

The per-variant table is the demo's headline artifact: the interviewer sees
the metrics climb as retrieval gets smarter, alongside latency.

Run:  python -m app.eval.run_ragas
"""

import json
import time
from pathlib import Path

from app.agent.graph import run_agent
from app.db.engine import SessionLocal
from app.eval.dataset import GOLDEN_SET
from app.schemas import RagConfig

RESULTS_PATH = Path(__file__).parent / "results.json"

VARIANTS: dict[str, RagConfig] = {
    "vector-only": RagConfig(use_hybrid=False, use_rerank=False),
    "hybrid": RagConfig(use_hybrid=True, use_rerank=False),
    "hybrid+rerank": RagConfig(use_hybrid=True, use_rerank=True),
    "hybrid+rerank+hyde": RagConfig(use_hybrid=True, use_rerank=True, use_hyde=True),
}


def _collect(config: RagConfig) -> tuple[list[dict], float]:
    samples: list[dict] = []
    t0 = time.perf_counter()
    with SessionLocal() as session:
        for item in GOLDEN_SET:
            resp = run_agent(session, item.question, config)
            samples.append(
                {
                    "user_input": item.question,
                    "retrieved_contexts": [b.content for b in resp.blocks] or [""],
                    "response": resp.answer,
                    "reference": item.ground_truth,
                }
            )
    avg_latency = (time.perf_counter() - t0) * 1000 / max(len(GOLDEN_SET), 1)
    return samples, avg_latency


def _score(samples: list[dict]) -> dict:
    from ragas import EvaluationDataset, evaluate
    from ragas.metrics import (
        answer_relevancy,
        context_precision,
        context_recall,
        faithfulness,
    )

    dataset = EvaluationDataset.from_list(samples)
    result = evaluate(
        dataset=dataset,
        metrics=[faithfulness, answer_relevancy, context_precision, context_recall],
    )
    df = result.to_pandas()
    return {
        m: float(df[m].mean())
        for m in ["faithfulness", "answer_relevancy", "context_precision", "context_recall"]
        if m in df
    }


def run() -> dict:
    runs = []
    for name, config in VARIANTS.items():
        samples, avg_latency = _collect(config)
        try:
            metrics = _score(samples)
        except Exception as e:  # noqa: BLE001 — keep the dashboard alive if RAGAS deps absent
            metrics = {"error": str(e)}
        runs.append({"variant": name, "metrics": metrics, "avg_latency_ms": round(avg_latency)})

    output = {"runs": runs, "n_questions": len(GOLDEN_SET)}
    RESULTS_PATH.write_text(json.dumps(output, ensure_ascii=False, indent=2), encoding="utf-8")
    return output


if __name__ == "__main__":
    print(json.dumps(run(), ensure_ascii=False, indent=2))
