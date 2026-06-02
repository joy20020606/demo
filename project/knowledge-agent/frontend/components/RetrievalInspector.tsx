"use client";

import { RetrievedBlock } from "@/lib/api";

function Score({ label, value }: { label: string; value: number | null }) {
  if (value === null || value === undefined) return null;
  return (
    <span className="rounded bg-slate-100 px-2 py-0.5 text-xs text-slate-600">
      {label} {value.toFixed(3)}
    </span>
  );
}

export function RetrievalInspector({ blocks }: { blocks: RetrievedBlock[] }) {
  if (!blocks.length) return null;
  return (
    <div className="space-y-3 rounded border bg-white p-4">
      <div className="font-medium">檢索檢視（Top-{blocks.length}）</div>
      {blocks.map((b, i) => (
        <div key={b.chunk_id} className="rounded border p-3">
          <div className="mb-1 text-sm font-medium">
            [{i + 1}] 《{b.title}》{b.author ? ` — ${b.author}` : ""}
          </div>
          <div className="mb-2 flex flex-wrap gap-1">
            <Score label="vec" value={b.vector_score} />
            <Score label="kw" value={b.keyword_score} />
            <Score label="rrf" value={b.rrf_score} />
            <Score label="rerank" value={b.rerank_score} />
          </div>
          <p className="line-clamp-3 text-xs text-slate-600">{b.content}</p>
        </div>
      ))}
    </div>
  );
}
