"use client";

import { useCallback, useEffect, useState } from "react";

import StatusBadge from "@/components/StatusBadge";
import { EmptyRow, Table, Td } from "@/components/Table";
import { useTenant } from "@/components/TenantProvider";
import {
  listDeadLetters,
  replayDeadLetter,
  type DeadLetter,
} from "@/lib/api";

export default function DeadLetterPage() {
  const { tenant } = useTenant();
  const [letters, setLetters] = useState<DeadLetter[]>([]);
  const [busy, setBusy] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      setLetters(await listDeadLetters(tenant));
    } catch (e) {
      setError(String(e));
    }
  }, [tenant]);

  useEffect(() => {
    load();
  }, [load]);

  async function onReplay(id: string) {
    setBusy(id);
    setError(null);
    try {
      await replayDeadLetter(tenant, id);
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
        <h1 className="text-2xl font-bold">Dead Letter Queue</h1>
        <p className="text-sm text-slate-600">
          Failed messages for tenant{" "}
          <span className="font-medium text-ink">{tenant}</span> — replay to
          retry ingestion.
        </p>
      </section>

      {error && (
        <p className="rounded border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
          {error}
        </p>
      )}

      <Table
        columns={[
          "Connector",
          "Source msg id",
          "Error type",
          "Status",
          "Retries",
          "",
        ]}
      >
        {letters.length === 0 ? (
          <EmptyRow colSpan={6} label="Dead letter queue is empty." />
        ) : (
          letters.map((d) => (
            <tr key={d.id}>
              <Td>
                <span className="font-medium">{d.connector}</span>
              </Td>
              <Td>
                <span className="text-slate-500">
                  {d.source_message_id ?? "—"}
                </span>
              </Td>
              <Td>{d.error_type}</Td>
              <Td>
                <StatusBadge status={d.status} />
              </Td>
              <Td>{d.retry_count}</Td>
              <Td>
                <button
                  type="button"
                  onClick={() => onReplay(d.id)}
                  disabled={busy !== null || d.status !== "pending"}
                  className="rounded bg-accent px-3 py-1 text-xs font-medium text-white hover:opacity-90 disabled:opacity-40"
                >
                  {busy === d.id ? "Replaying…" : "Replay"}
                </button>
              </Td>
            </tr>
          ))
        )}
      </Table>
    </div>
  );
}
