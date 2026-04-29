"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from "recharts";
import { api, getToken } from "@/lib/api";

interface Metric {
  date: string;
  impressions: number;
  clicks: number;
  spend: number;
  sales: number;
  acos: number;
}

interface AgentResponse {
  final_answer: string;
  trace: Array<{
    step: number;
    thoughts: string[];
    tool_calls: Array<{ name: string; input: any }>;
  }>;
}

export default function CampaignDetailPage() {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;

  const [metrics, setMetrics] = useState<Metric[]>([]);
  const [loading, setLoading] = useState(true);
  const [aiLoading, setAiLoading] = useState(false);
  const [aiResult, setAiResult] = useState<AgentResponse | null>(null);

  useEffect(() => {
    if (!getToken()) {
      router.push("/");
      return;
    }
    api<Metric[]>(`/api/campaigns/${id}/metrics?days=30`)
      .then(setMetrics)
      .finally(() => setLoading(false));
  }, [id, router]);

  async function runAgent() {
    setAiLoading(true);
    setAiResult(null);
    try {
      const result = await api<AgentResponse>("/api/ai/analyze", {
        method: "POST",
        body: JSON.stringify({ campaign_id: id }),
      });
      setAiResult(result);
    } catch (err: any) {
      alert(err.message);
    } finally {
      setAiLoading(false);
    }
  }

  if (loading) return <div className="p-8">載入中...</div>;

  return (
    <main className="p-8 max-w-7xl mx-auto">
      <Link href="/campaigns" className="text-sm text-gray-500 hover:text-black">
        ← 返回列表
      </Link>

      <h1 className="text-3xl font-bold mt-2 mb-6">廣告活動詳細</h1>

      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="font-semibold mb-4">過去 30 天 ACOS 趨勢</h2>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={metrics}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis />
            <Tooltip />
            <Legend />
            <Line type="monotone" dataKey="acos" stroke="#ef4444" name="ACOS %" />
          </LineChart>
        </ResponsiveContainer>
      </div>

      <div className="bg-white rounded-lg shadow p-6 mb-6">
        <h2 className="font-semibold mb-4">花費 vs 銷售</h2>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={metrics}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="date" />
            <YAxis />
            <Tooltip />
            <Legend />
            <Line type="monotone" dataKey="spend" stroke="#f59e0b" name="花費 $" />
            <Line type="monotone" dataKey="sales" stroke="#10b981" name="銷售 $" />
          </LineChart>
        </ResponsiveContainer>
      </div>

      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="font-semibold">🤖 Claude AI 分析</h2>
          <button
            onClick={runAgent}
            disabled={aiLoading}
            className="bg-black text-white px-4 py-2 rounded hover:bg-gray-800 disabled:opacity-50"
          >
            {aiLoading ? "分析中... (約 30 秒)" : "啟動 AI Agent 分析"}
          </button>
        </div>

        {aiResult && (
          <div>
            <div className="bg-blue-50 border border-blue-200 rounded p-4 mb-4">
              <h3 className="font-semibold mb-2">📋 最終建議</h3>
              <p className="text-sm whitespace-pre-wrap">{aiResult.final_answer}</p>
            </div>

            <details className="text-sm">
              <summary className="cursor-pointer text-gray-500">▸ 查看推理過程({aiResult.trace.length} 步)</summary>
              <div className="mt-3 space-y-3">
                {aiResult.trace.map((step) => (
                  <div key={step.step} className="border-l-2 border-gray-300 pl-3">
                    <div className="font-semibold">Step {step.step}</div>
                    {step.thoughts.map((t, i) => (
                      <p key={i} className="text-gray-600 mt-1">💭 {t}</p>
                    ))}
                    {step.tool_calls.map((tc, i) => (
                      <p key={i} className="text-blue-600 mt-1 font-mono text-xs">
                        🔧 {tc.name}({JSON.stringify(tc.input)})
                      </p>
                    ))}
                  </div>
                ))}
              </div>
            </details>
          </div>
        )}
      </div>
    </main>
  );
}
