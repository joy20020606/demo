"use client";

import { useEffect, useState } from "react";
import { DocumentOut, listDocuments, uploadDocument } from "@/lib/api";

const METHODS = ["fixed", "semantic", "propositional"];

export default function UploadPage() {
  const [file, setFile] = useState<File | null>(null);
  const [method, setMethod] = useState("fixed");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [docs, setDocs] = useState<DocumentOut[]>([]);

  const refresh = () => listDocuments().then(setDocs).catch(() => {});
  useEffect(() => { refresh(); }, []);

  async function submit() {
    if (!file) return;
    setBusy(true);
    setError("");
    try {
      await uploadDocument(file, method);
      setFile(null);
      refresh();
    } catch (e) {
      setError(String(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="space-y-8">
      <section className="space-y-4">
        <h1 className="text-2xl font-bold">上傳文件</h1>
        <input type="file" accept=".pdf,.txt"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
        <div className="flex items-center gap-3">
          <span className="text-sm">分塊策略：</span>
          <select value={method} onChange={(e) => setMethod(e.target.value)}
            className="rounded border px-2 py-1 text-sm">
            {METHODS.map((m) => <option key={m} value={m}>{m}</option>)}
          </select>
          <button onClick={submit} disabled={!file || busy}
            className="rounded bg-accent px-4 py-2 text-white disabled:opacity-50">
            {busy ? "處理中…" : "匯入"}
          </button>
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold">已匯入文件</h2>
        <div className="space-y-2">
          {docs.map((d) => (
            <div key={d.id} className="rounded border bg-white p-3 text-sm">
              <div className="font-medium">{d.title}</div>
              <div className="text-slate-600">
                作者：{d.author ?? "—"}　類型：{d.source_type ?? "—"}
                區塊：{d.chunk_count}
              </div>
            </div>
          ))}
          {!docs.length && <p className="text-sm text-slate-500">尚無文件。</p>}
        </div>
      </section>
    </div>
  );
}
