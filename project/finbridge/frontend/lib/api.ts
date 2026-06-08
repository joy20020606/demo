export const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8002";

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

async function json<T>(res: Response): Promise<T> {
  if (!res.ok) throw new Error(`${res.status}: ${await res.text()}`);
  return res.json() as Promise<T>;
}

export async function getHealth(): Promise<Health> {
  return json<Health>(await fetch(`${API_URL}/health`));
}

export async function listTenants(): Promise<Tenant[]> {
  return json<Tenant[]>(await fetch(`${API_URL}/tenants`));
}
