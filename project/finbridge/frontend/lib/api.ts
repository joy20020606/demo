export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8002";

export type TenantSlug = "acme" | "globex";

export interface Health {
  status: string;
  db: boolean;
}

export interface Tenant {
  id: string;
  slug: string;
  name: string;
  created_at: string;
}

export type ConnectorRunStatus = "running" | "success" | "partial" | "failed";

export interface ConnectorRun {
  id: string;
  tenant_id: string;
  connector: string;
  status: ConnectorRunStatus;
  started_at: string;
  finished_at: string | null;
  messages_total: number;
  messages_ok: number;
  messages_dead: number;
  error: string | null;
}

export type ReconciliationBreakStatus = "open" | "acknowledged";
export type ReconciliationBreakType =
  | "qty_mismatch"
  | "missing_position"
  | "extra_position";

export interface ReconciliationBreak {
  id: string;
  tenant_id: string;
  instrument_id: string | null;
  account_id: string;
  symbol: string;
  break_type: ReconciliationBreakType;
  expected: string;
  actual: string;
  delta: string;
  status: ReconciliationBreakStatus;
  detected_at: string;
}

export type DeadLetterStatus = "pending" | "replayed" | "resolved";

export interface DeadLetter {
  id: string;
  tenant_id: string;
  connector: string;
  source_message_id: string | null;
  raw_payload: string;
  error_type: string;
  error_detail: string;
  retry_count: number;
  status: DeadLetterStatus;
  created_at: string;
}

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status}: ${await res.text()}`);
  return res.json() as Promise<T>;
}

function headers(tenant: string): HeadersInit {
  return { "X-Tenant": tenant };
}

export async function getHealth(): Promise<Health> {
  return json<Health>(await fetch(`${API_URL}/health`));
}

export async function listTenants(): Promise<Tenant[]> {
  return json<Tenant[]>(await fetch(`${API_URL}/tenants`));
}

export async function listConnectorRuns(tenant: string): Promise<ConnectorRun[]> {
  return json<ConnectorRun[]>(
    await fetch(`${API_URL}/connector-runs`, { headers: headers(tenant) }),
  );
}

export async function runConnector(
  tenant: string,
  name: string,
): Promise<ConnectorRun> {
  return json<ConnectorRun>(
    await fetch(`${API_URL}/connectors/${name}/run`, {
      method: "POST",
      headers: headers(tenant),
    }),
  );
}

export async function listBreaks(
  tenant: string,
  status?: ReconciliationBreakStatus,
): Promise<ReconciliationBreak[]> {
  const qs = status ? `?status=${status}` : "";
  return json<ReconciliationBreak[]>(
    await fetch(`${API_URL}/reconciliation/breaks${qs}`, {
      headers: headers(tenant),
    }),
  );
}

export async function runReconciliation(
  tenant: string,
): Promise<{ breaks: number }> {
  return json<{ breaks: number }>(
    await fetch(`${API_URL}/reconciliation/run`, {
      method: "POST",
      headers: headers(tenant),
    }),
  );
}

export async function listDeadLetters(
  tenant: string,
  status?: DeadLetterStatus,
): Promise<DeadLetter[]> {
  const qs = status ? `?status=${status}` : "";
  return json<DeadLetter[]>(
    await fetch(`${API_URL}/dead-letter${qs}`, { headers: headers(tenant) }),
  );
}

export async function replayDeadLetter(
  tenant: string,
  id: string,
): Promise<DeadLetter> {
  return json<DeadLetter>(
    await fetch(`${API_URL}/dead-letter/${id}/replay`, {
      method: "POST",
      headers: headers(tenant),
    }),
  );
}
