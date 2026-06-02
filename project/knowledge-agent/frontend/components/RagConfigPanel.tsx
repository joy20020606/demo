"use client";

import { RagConfig } from "@/lib/api";

export function RagConfigPanel({
  config,
  onChange,
}: {
  config: RagConfig;
  onChange: (c: RagConfig) => void;
}) {
  const set = <K extends keyof RagConfig>(k: K, v: RagConfig[K]) =>
    onChange({ ...config, [k]: v });

  const toggles: [keyof RagConfig, string][] = [
    ["use_hybrid", "Hybrid (vector + keyword)"],
    ["use_rerank", "Cross-Encoder Rerank"],
    ["use_hyde", "HyDE"],
    ["use_multi_query", "Multi-query"],
  ];

  return (
    <div className="space-y-4 rounded border bg-white p-4">
      <div className="font-medium">檢索參數</div>

      <label className="block text-sm">
        Top-k (檢索池): {config.top_k}
        <input
          type="range" min={5} max={50} value={config.top_k}
          onChange={(e) => set("top_k", Number(e.target.value))}
          className="w-full"
        />
      </label>

      <label className="block text-sm">
        Final-k (注入 LLM): {config.final_k}
        <input
          type="range" min={1} max={10} value={config.final_k}
          onChange={(e) => set("final_k", Number(e.target.value))}
          className="w-full"
        />
      </label>

      <label className="block text-sm">
        相似度門檻: {config.similarity_threshold.toFixed(2)}
        <input
          type="range" min={0} max={1} step={0.05} value={config.similarity_threshold}
          onChange={(e) => set("similarity_threshold", Number(e.target.value))}
          className="w-full"
        />
      </label>

      <div className="space-y-2">
        {toggles.map(([key, label]) => (
          <label key={key} className="flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              checked={config[key] as boolean}
              onChange={(e) => set(key, e.target.checked as RagConfig[typeof key])}
            />
            {label}
          </label>
        ))}
      </div>
    </div>
  );
}
