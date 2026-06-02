import Link from "next/link";

const pipeline = [
  "查詢擴增 (HyDE / Multi-query)",
  "混合檢索 (pgvector + tsvector)",
  "RRF 融合",
  "Cross-Encoder 重排序",
  "Top-3 上下文注入",
  "Claude 生成 + [n] 引用",
  "引用驗證 (來源/作者/理論)",
];

const skills = [
  ["語意 / 命題分塊", "fixed / semantic / propositional 三策略可比較"],
  ["混合搜尋", "pgvector HNSW + Postgres tsvector，RRF 融合"],
  ["重排序", "BAAI/bge-reranker-base cross-encoder（可開關）"],
  ["查詢擴增", "HyDE 假設文件 + Multi-query 改寫"],
  ["Agent 編排", "LangGraph StateGraph + tool-calling"],
  ["引用正確性", "[n] 標記映射回 chunk，捏造引用會被丟棄"],
  ["自動化評估", "RAGAS：faithfulness / context precision 等"],
  ["可調參", "Top-k、門檻、overlap 前端即時調整"],
];

export default function Home() {
  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <h1 className="text-3xl font-bold">知識庫問答 Agent</h1>
        <p className="text-slate-600">
          上傳論文 / 書籍 → 語意分塊 → 混合檢索 + 重排序 → LangGraph Agent
          帶<strong>來源、作者、理論</strong>引用回答。可調參 + RAGAS 評估儀表板。
        </p>
        <div className="flex gap-3 pt-2">
          <Link href="/upload" className="rounded bg-accent px-4 py-2 text-white">
            上傳文件
          </Link>
          <Link href="/chat" className="rounded border px-4 py-2">
            開始問答
          </Link>
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold">RAG Pipeline</h2>
        <div className="flex flex-wrap items-center gap-2 text-sm">
          {pipeline.map((p, i) => (
            <span key={p} className="flex items-center gap-2">
              <span className="rounded bg-white px-3 py-1 shadow-sm">{p}</span>
              {i < pipeline.length - 1 && <span className="text-slate-400">→</span>}
            </span>
          ))}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold">技能對應</h2>
        <div className="grid gap-3 sm:grid-cols-2">
          {skills.map(([title, desc]) => (
            <div key={title} className="rounded border bg-white p-4">
              <div className="font-medium">{title}</div>
              <div className="text-sm text-slate-600">{desc}</div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
