"use client";

import { useState } from "react";
import { CitationCard } from "@/components/CitationCard";
import { RagConfigPanel } from "@/components/RagConfigPanel";
import { RetrievalInspector } from "@/components/RetrievalInspector";
import { chat, ChatResponse, defaultConfig, RagConfig } from "@/lib/api";

export default function ChatPage() {
  const [query, setQuery] = useState("");
  const [config, setConfig] = useState<RagConfig>(defaultConfig);
  const [resp, setResp] = useState<ChatResponse | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  async function ask() {
    if (!query.trim()) return;
    setBusy(true);
    setError("");
    try {
      setResp(await chat(query, config));
    } catch (e) {
      setError(String(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="grid gap-6 lg:grid-cols-[1fr_320px]">
      <div className="space-y-4">
        <h1 className="text-2xl font-bold">問答</h1>
        <div className="flex gap-2">
          <input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && ask()}
            placeholder="輸入問題…"
            className="flex-1 rounded border px-3 py-2"
          />
          <button onClick={ask} disabled={busy}
            className="rounded bg-accent px-4 py-2 text-white disabled:opacity-50">
            {busy ? "查詢中…" : "送出"}
          </button>
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}

        {resp && (
          <div className="space-y-4">
            <div className="rounded border bg-white p-4">
              <div className="mb-2 text-xs text-slate-500">
                延遲 {resp.latency_ms}ms
                {resp.expanded_queries.length > 1 &&
                  `　· 擴增查詢 ${resp.expanded_queries.length}`}
              </div>
              <p className="whitespace-pre-wrap leading-relaxed">{resp.answer}</p>
            </div>

            {resp.citations.length > 0 && (
              <div className="space-y-2">
                <div className="text-sm font-medium">引用來源</div>
                {resp.citations.map((c) => (
                  <CitationCard key={c.n} citation={c} />
                ))}
              </div>
            )}

            <RetrievalInspector blocks={resp.blocks} />
          </div>
        )}
      </div>

      <RagConfigPanel config={config} onChange={setConfig} />
    </div>
  );
}
