"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { api, getToken, clearToken } from "@/lib/api";

interface Campaign {
  id: string;
  name: string;
  type: string;
  state: string;
  daily_budget: number;
  target_acos: number;
  impressions_7d: number;
  clicks_7d: number;
  spend_7d: number;
  sales_7d: number;
  acos_7d: number;
}

export default function CampaignsPage() {
  const router = useRouter();
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!getToken()) {
      router.push("/");
      return;
    }
    api<Campaign[]>("/api/campaigns")
      .then(setCampaigns)
      .catch(() => router.push("/"))
      .finally(() => setLoading(false));
  }, [router]);

  function handleLogout() {
    clearToken();
    router.push("/");
  }

  if (loading) return <div className="p-8">載入中...</div>;

  return (
    <main className="p-8 max-w-7xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">廣告活動</h1>
        <button onClick={handleLogout} className="text-sm text-gray-500 hover:text-black">
          登出
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {campaigns.map((c) => {
          const acosOverTarget = c.acos_7d > c.target_acos;
          return (
            <Link
              key={c.id}
              href={`/campaigns/${c.id}`}
              className="bg-white rounded-lg shadow p-5 hover:shadow-md transition"
            >
              <div className="flex justify-between items-start mb-3">
                <h2 className="font-semibold text-lg">{c.name}</h2>
                <span className="text-xs px-2 py-1 bg-gray-100 rounded">{c.type}</span>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-500">7 天花費</span>
                  <span>${c.spend_7d.toFixed(2)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">7 天銷售</span>
                  <span>${c.sales_7d.toFixed(2)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">ACOS</span>
                  <span className={acosOverTarget ? "text-red-600 font-semibold" : "text-green-600"}>
                    {c.acos_7d.toFixed(1)}% / {c.target_acos.toFixed(0)}%
                  </span>
                </div>
              </div>
            </Link>
          );
        })}
      </div>
    </main>
  );
}
