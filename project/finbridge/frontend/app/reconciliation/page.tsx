"use client";

import { useCallback, useEffect, useState } from "react";

import StatusBadge from "@/components/StatusBadge";
import { EmptyRow, Table, Td } from "@/components/Table";
import { useTenant } from "@/components/TenantProvider";
import {
  listBreaks,
  runReconciliation,
  type ReconciliationBreak,
} from "@/lib/api";

export default function ReconciliationPage() {
  const { tenant } = useTenant();
  const [breaks, setBreaks] = useState<ReconciliationBreak[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      setBreaks(await listBreaks(tenant));
    } catch (e) {
      setError(String(e));
    }
  }, [tenant]);

  useEffect(() => {
    load();
  }, [load]);

  async function onRun() {
    setBusy(true);
    setError(null);
    try {
      await runReconciliation(tenant);
      await load();
    } catch (e) {
      setError(String(e));
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="space-y-6">
      <section className="space-y-2">
        <h1 className="text-2xl font-bold">Reconciliation</h1>
        <p className="text-sm text-slate-600">
          Compare FinBridge positions against declared upstream for tenant{" "}
          <span className="font-medium text-ink">{tenant}</span>.
        </p>
      </section>

      {error && (
        <p className="rounded border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
          {error}
        </p>
      )}

      <button
        type="button"
        onClick={onRun}
        disabled={busy}
        className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-50"
      >
        {busy ? "Running…" : "Run reconciliation"}
      </button>

      <Table
        columns={[
          "Account",
          "Symbol",
          "Break type",
          "Expected",
          "Actual",
          "Delta",
          "Status",
        ]}
      >
        {breaks.length === 0 ? (
          <EmptyRow colSpan={7} label="No breaks — all reconciled." />
        ) : (
          breaks.map((b) => (
            <tr key={b.id}>
              <Td>{b.account_id}</Td>
              <Td>
                <span className="font-medium">{b.symbol}</span>
              </Td>
              <Td>{b.break_type}</Td>
              <Td>
                <span className="tabular-nums">{b.expected}</span>
              </Td>
              <Td>
                <span className="tabular-nums">{b.actual}</span>
              </Td>
              <Td>
                <span className="tabular-nums">{b.delta}</span>
              </Td>
              <Td>
                <StatusBadge status={b.status} />
              </Td>
            </tr>
          ))
        )}
      </Table>
    </div>
  );
}
