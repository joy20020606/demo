"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { api, getToken } from "@/lib/api";

interface Recommendation {
  id: string;
  campaign_id: string;
  type: string;
  reasoning: string;
  suggested_action: any;
  status: string;
  created_at: string;
}

export default function RecommendationsPage() {
  const router = useRouter();
  const [recs, setRecs] = useState<Recommendation[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!getToken()) {
      router.push("/");
      return;
    }
    api<Recommendation[]>("/api/ai/recommendations")
      .then(setRecs)
      .finally(() => setLoading(false));
  }, [router]);

async function updateStatus(id: string, status: "accepted" | "rejected") {
  try {
    await api(`/api/ai/recommendations/${id}`, {
      method: "PATCH",
      body: JSON.stringify({ status }),
    });
    // 重新載入整個列表(簡單做法)
    const updated = await api<Recommendation[]>("/api/ai/recommendations");
    setRecs(updated);
  } catch (err: any) {
    alert(`更新失敗: ${err.message}`);
  }
}

  if (loading) return <div className="p-8">載入中...</div>;

  return (
    <main className="p-8 max-w-5xl mx-auto">
      <Link href="/campaigns" className="text-sm text-gray-500 hover:text-black">
        ← 返回 campaigns
      </Link>

      <h1 className="text-3xl font-bold mt-2 mb-6">AI 建議審核</h1>

      {recs.length === 0 ? (
        <p className="text-gray-500">
          目前沒有建議。先去 campaign 詳細頁啟動 AI 分析。
        </p>
      ) : (
        <div className="space-y-4">
          {recs.map((r) => (
            <div
              key={r.id}
              className="bg-white rounded-lg shadow p-5 border-l-4 border-blue-400"
            >
              <div className="flex justify-between items-start mb-3">
                <div>
                  <span className="text-xs px-2 py-1 bg-blue-100 rounded mr-2">
                    {r.type}
                  </span>
                  <span className="text-xs text-gray-500">
                    {new Date(r.created_at).toLocaleString()}
                  </span>
                </div>
                <span
                  className={`text-xs px-2 py-1 rounded ${
                    r.status === "pending"
                      ? "bg-yellow-100 text-yellow-800"
                      : r.status === "accepted"
                      ? "bg-green-100 text-green-800"
                      : "bg-red-100 text-red-800"
                  }`}
                >
                  {r.status}
                </span>
              </div>

              <p className="text-sm text-gray-700 mb-3 whitespace-pre-wrap">
                {r.reasoning}
              </p>

              <pre className="text-xs bg-gray-50 rounded p-2 overflow-x-auto">
                {JSON.stringify(r.suggested_action, null, 2)}
              </pre>
              {r.status === "pending" && (
                <div className="flex gap-2 mt-3">
                  <button
                    onClick={() => updateStatus(r.id, "accepted")}
                    className="bg-green-500 text-white px-4 py-1 rounded hover:bg-green-600 text-sm"
                  >
                    Accept
                  </button>
                  <button
                    onClick={() => updateStatus(r.id, "rejected")}
                    className="bg-red-500 text-white px-4 py-1 rounded hover:bg-red-600 text-sm"
                  >
                    Reject
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </main>
  );
}