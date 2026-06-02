export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8000";

export interface RagConfig {
  top_k: number;
  final_k: number;
  similarity_threshold: number;
  use_hybrid: boolean;
  use_rerank: boolean;
  use_hyde: boolean;
  use_multi_query: boolean;
}

export const defaultConfig: RagConfig = {
  top_k: 20,
  final_k: 3,
  similarity_threshold: 0.2,
  use_hybrid: true,
  use_rerank: false,
  use_hyde: false,
  use_multi_query: false,
};

export interface RetrievedBlock {
  chunk_id: string;
  document_id: string;
  content: string;
  title: string;
  author: string | null;
  source_ref: string | null;
  theory_tag: string | null;
  page_no: number | null;
  vector_score: number | null;
  keyword_score: number | null;
  rrf_score: number | null;
  rerank_score: number | null;
}

export interface Citation {
  n: number;
  chunk_id: string;
  source: string;
  author: string | null;
  theory: string | null;
  page_no: number | null;
}

export interface ChatResponse {
  answer: string;
  citations: Citation[];
  blocks: RetrievedBlock[];
  expanded_queries: string[];
  latency_ms: number | null;
}

export interface DocumentOut {
  id: string;
  title: string;
  author: string | null;
  source_type: string | null;
  source_ref: string | null;
  chunk_count: number;
}

export interface EvalRun {
  variant: string;
  metrics: Record<string, number> & { error?: string };
  avg_latency_ms: number;
}

export interface EvalResults {
  runs: EvalRun[];
  n_questions: number;
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status}: ${await res.text()}`);
  return res.json() as Promise<T>;
}

export async function chat(query: string, config: RagConfig): Promise<ChatResponse> {
  const res = await fetch(`${API_URL}/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ query, config }),
  });
  return json<ChatResponse>(res);
}

export async function listDocuments(): Promise<DocumentOut[]> {
  return json<DocumentOut[]>(await fetch(`${API_URL}/documents`));
}

export async function uploadDocument(file: File, method: string): Promise<unknown> {
  const form = new FormData();
  form.append("file", file);
  form.append("method", method);
  return json(await fetch(`${API_URL}/documents`, { method: "POST", body: form }));
}

export async function getEvalResults(): Promise<EvalResults> {
  return json<EvalResults>(await fetch(`${API_URL}/eval/results`));
}

export async function runEval(): Promise<unknown> {
  return json(await fetch(`${API_URL}/eval/run`, { method: "POST" }));
}
