"use client";

import { useEffect, useState } from "react";
import { MetricBar } from "@/components/MetricBar";
import { EvalResults, getEvalResults, runEval } from "@/lib/api";

const METRICS = ["faithfulness", "answer_relevancy", "context_precision", "context_recall"];

export default function EvalPage() {
  const [results, setResults] = useState<EvalResults | null>(null);
  const [busy, setBusy] = useState(false);

  const refresh = () => getEvalResults().then(setResults).catch(() => {});
  useEffect(() => { refresh(); }, []);

  async function trigger() {
    setBusy(true);
    try {
      await runEval();
      setTimeout(refresh, 3000);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">RAGAS 評估</h1>
        <button onClick={trigger} disabled={busy}
          className="rounded bg-accent px-4 py-2 text-white disabled:opacity-50">
          {busy ? "執行中…" : "Run eval"}
        </button>
      </div>

      <p className="text-sm text-slate-600">
        各組態跑同一份 golden set（{results?.n_questions ?? 0} 題），比較指標隨
        hybrid / rerank / HyDE 的變化與延遲成本。
      </p>

      <div className="grid gap-4 md:grid-cols-2">
        {results?.runs.map((run) => (
          <div key={run.variant} className="space-y-3 rounded border bg-white p-4">
            <div className="flex items-center justify-between">
              <div className="font-medium">{run.variant}</div>
              <div className="text-xs text-slate-500">{run.avg_latency_ms}ms/題</div>
            </div>
            {run.metrics.error ? (
              <p className="text-xs text-red-600">{run.metrics.error}</p>
            ) : (
              METRICS.map((m) =>
                run.metrics[m] !== undefined ? (
                  <MetricBar key={m} label={m} value={run.metrics[m]} />
                ) : null
              )
            )}
          </div>
        ))}
        {!results?.runs.length && (
          <p className="text-sm text-slate-500">尚無評估結果，按 Run eval 開始。</p>
        )}
      </div>
    </div>
  );
}
