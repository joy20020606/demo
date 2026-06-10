"use client";

import { useCallback, useEffect, useState } from "react";

import StatusBadge from "@/components/StatusBadge";
import { useTenant } from "@/components/TenantProvider";
import {
  getHealth,
  listBreaks,
  listConnectorRuns,
  listDeadLetters,
  type ConnectorRun,
  type Health,
} from "@/lib/api";

export default function Home() {
  const { tenant } = useTenant();
  const [health, setHealth] = useState<Health | null>(null);
  const [runs, setRuns] = useState<ConnectorRun[]>([]);
  const [openBreaks, setOpenBreaks] = useState<number | null>(null);
  const [pendingDl, setPendingDl] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      const [h, r, breaks, dl] = await Promise.all([
        getHealth(),
        listConnectorRuns(tenant),
        listBreaks(tenant, "open"),
        listDeadLetters(tenant, "pending"),
      ]);
      setHealth(h);
      setRuns(r);
      setOpenBreaks(breaks.length);
      setPendingDl(dl.length);
    } catch (e) {
      setError(String(e));
    }
  }, [tenant]);

  useEffect(() => {
    load();
  }, [load]);

  return (
    <div className="space-y-8">
      <section className="space-y-2">
        <h1 className="text-2xl font-bold">Ops Overview</h1>
        <p className="text-sm text-slate-600">
          Tenant <span className="font-medium text-ink">{tenant}</span> — pipeline
          health at a glance.
        </p>
      </section>

      {error && (
        <p className="rounded border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
          {error}
        </p>
      )}

      <section className="grid gap-4 sm:grid-cols-3">
        <Card label="API health">
          {health ? (
            <span className="flex items-center gap-2">
              <StatusBadge status={health.db ? "success" : "failed"} />
              <span className="text-sm text-slate-500">
                db {health.db ? "up" : "down"}
              </span>
            </span>
          ) : (
            "—"
          )}
        </Card>
        <Card label="Open recon breaks">
          <span className="text-2xl font-semibold">{openBreaks ?? "—"}</span>
        </Card>
        <Card label="Pending dead letters">
          <span className="text-2xl font-semibold">{pendingDl ?? "—"}</span>
        </Card>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Latest connector runs</h2>
        <div className="space-y-2">
          {runs.length === 0 && (
            <p className="text-sm text-slate-400">No runs yet.</p>
          )}
          {runs.slice(0, 5).map((r) => (
            <div
              key={r.id}
              className="flex items-center justify-between rounded-lg border bg-white px-4 py-3 text-sm"
            >
              <div className="flex items-center gap-3">
                <StatusBadge status={r.status} />
                <span className="font-medium">{r.connector}</span>
              </div>
              <span className="text-slate-500">
                {r.messages_ok}/{r.messages_total} ok · {r.messages_dead} dead
              </span>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

function Card({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <div className="rounded-lg border bg-white p-4">
      <p className="text-xs uppercase tracking-wide text-slate-400">{label}</p>
      <div className="mt-2">{children}</div>
    </div>
  );
}
