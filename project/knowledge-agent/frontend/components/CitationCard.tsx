"use client";

import { Citation } from "@/lib/api";

export function CitationCard({ citation }: { citation: Citation }) {
  return (
    <div className="rounded border bg-white p-3 text-sm">
      <div className="font-medium text-accent">[{citation.n}] {citation.source}</div>
      <div className="text-slate-600">
        {citation.author && <span>作者：{citation.author}　</span>}
        {citation.theory && <span>理論：{citation.theory}　</span>}
        {citation.page_no && <span>頁：{citation.page_no}</span>}
      </div>
    </div>
  );
}
