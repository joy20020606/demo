/**
 * PostgreSQL 一次性讀取 —— 取得路線清單作為產生事件的基礎資料。
 *
 * ingestion 故意用輕量的 pg driver（而非 Prisma），讓這個微服務保持獨立、
 * 不依賴 api-gateway 的資料層。這也是「微服務各自管自己的依賴」的體現。
 */
import pg from 'pg';
import { logger } from '@maas/shared';

export interface RouteRef {
  id: string;
  tenantId: string;
}

export async function fetchRoutes(databaseUrl: string): Promise<RouteRef[]> {
  const client = new pg.Client({ connectionString: databaseUrl });
  await client.connect();
  try {
    const res = await client.query<{ id: string; tenant_id: string }>(
      'SELECT id, tenant_id FROM routes',
    );
    logger.info(`Loaded ${res.rows.length} routes from PostgreSQL`);
    return res.rows.map((r) => ({ id: r.id, tenantId: r.tenant_id }));
  } finally {
    await client.end();
  }
}
