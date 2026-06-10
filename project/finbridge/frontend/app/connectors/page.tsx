"use client";

import { useCallback, useEffect, useState } from "react";

import StatusBadge from "@/components/StatusBadge";
import { EmptyRow, Table, Td } from "@/components/Table";
import { useTenant } from "@/components/TenantProvider";
import {
  listConnectorRuns,
  runConnector,
  type ConnectorRun,
} from "@/lib/api";

const CONNECTORS = ["trading_rest", "bank_xml", "csv_batch"];

export default function ConnectorsPage() {
  const { tenant } = useTenant();
  const [runs, setRuns] = useState<ConnectorRun[]>([]);
  const [busy, setBusy] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      setRuns(await listConnectorRuns(tenant));
    } catch (e) {
      setError(String(e));
    }
  }, [tenant]);

  useEffect(() => {
    load();
  }, [load]);

  async function onRun(name: string) {
    setBusy(name);
    setError(null);
    try {
      await runConnector(tenant, name);
      await load();
    } catch (e) {
      setError(String(e));
    } finally {
      setBusy(null);
    }
  }

  return (
    <div className="space-y-6">
      <section className="space-y-2">
        <h1 className="text-2xl font-bold">Connectors</h1>
        <p className="text-sm text-slate-600">
          Trigger ingestion for tenant{" "}
          <span className="font-medium text-ink">{tenant}</span>.
        </p>
      </section>

      {error && (
        <p className="rounded border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
          {error}
        </p>
      )}

      <div className="flex flex-wrap gap-3">
        {CONNECTORS.map((name) => (
          <button
            key={name}
            type="button"
            onClick={() => onRun(name)}
            disabled={busy !== null}
            className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-50"
          >
            {busy === name ? "Running…" : `Run ${name}`}
          </button>
        ))}
      </div>

      <Table
        columns={["Connector", "Status", "Total", "OK", "Dead", "Started"]}
      >
        {runs.length === 0 ? (
          <EmptyRow colSpan={6} label="No connector runs yet." />
        ) : (
          runs.map((r) => (
            <tr key={r.id}>
              <Td>
                <span className="font-medium">{r.connector}</span>
              </Td>
              <Td>
                <StatusBadge status={r.status} />
              </Td>
              <Td>{r.messages_total}</Td>
              <Td>{r.messages_ok}</Td>
              <Td>{r.messages_dead}</Td>
              <Td>
                <span className="text-slate-500">
                  {new Date(r.started_at).toLocaleString()}
                </span>
              </Td>
            </tr>
          ))
        )}
      </Table>
    </div>
  );
}
