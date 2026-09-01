import { API_BASE_URL } from "@/config";

export type RagConfig = {
  top_k?: number;
  final_k?: number;
  similarity_threshold?: number;
  use_hybrid?: boolean;
  use_rerank?: boolean;
  use_hyde?: boolean;
  use_multi_query?: boolean;
};

export type RetrievedBlock = {
  chunk_id: string;
  document_id: string;
  content: string;
  title: string;
  author?: string | null;
  source_ref?: string | null;
  theory_tag?: string | null;
  page_no?: number | null;
  vector_score?: number | null;
  keyword_score?: number | null;
  rrf_score?: number | null;
  rerank_score?: number | null;
};

export type Citation = {
  n: number;
  chunk_id: string;
  source: string;
  author?: string | null;
  theory?: string | null;
  page_no?: number | null;
};

export type ChatResponse = {
  answer: string;
  citations: Citation[];
  blocks: RetrievedBlock[];
  expanded_queries: string[];
  latency_ms?: number | null;
};

export async function askKnowledgeAgent(
  query: string,
  config?: RagConfig,
  signal?: AbortSignal,
): Promise<ChatResponse> {
  const res = await fetch(`${API_BASE_URL}/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ query, config }),
    signal,
  });
  if (!res.ok) {
    const body = await res.text().catch(() => "");
    throw new Error(
      `Chat request failed (${res.status}): ${body || res.statusText}`,
    );
  }
  return (await res.json()) as ChatResponse;
}
