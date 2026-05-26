/**
 * ingestion 微服務進入點。
 *
 * 職責：
 *   1. 連 MongoDB
 *   2. 從 PostgreSQL 一次性讀出路線清單（事件的基礎資料）
 *   3. 定時產生 mock 通勤事件，寫進 MongoDB 的 commute_events collection
 *
 * 真實系統中，第 2、3 步會換成接 GTFS-realtime 之類的真實交通資料來源。
 */
import 'dotenv/config';
import { logger, CommuteEventModel } from '@maas/shared';
import { connectMongo, disconnectMongo } from './db/mongo.js';
import { fetchRoutes } from './db/postgres.js';
import { generateEvent } from './generator.js';

const MONGO_URL = process.env.MONGO_URL ?? 'mongodb://localhost:27017/maas_ops';
const DATABASE_URL = process.env.DATABASE_URL ?? '';
const INTERVAL_MS = Number(process.env.INGESTION_INTERVAL_MS ?? 3000);

async function main(): Promise<void> {
  if (!DATABASE_URL) {
    throw new Error('DATABASE_URL is required（用來讀路線清單）');
  }

  await connectMongo(MONGO_URL);

  const routes = await fetchRoutes(DATABASE_URL);
  if (routes.length === 0) {
    logger.error('PostgreSQL 裡沒有路線。請先在 api-gateway 跑 `pnpm db:seed`。');
    process.exit(1);
  }

  // 把 routeId 依 tenant 分組
  const routesByTenant = new Map<string, string[]>();
  for (const r of routes) {
    const list = routesByTenant.get(r.tenantId) ?? [];
    list.push(r.id);
    routesByTenant.set(r.tenantId, list);
  }

  logger.info(
    `ingestion 啟動：${routesByTenant.size} 個租戶，每 ${INTERVAL_MS}ms 產生一輪事件`,
  );

  let total = 0;
  const timer = setInterval(() => {
    void (async () => {
      for (const [tenantId, routeIds] of routesByTenant) {
        const evt = generateEvent(tenantId, routeIds);
        await CommuteEventModel.create(evt);
        total += 1;
        logger.info(
          { eventType: evt.eventType, routeId: evt.routeId },
          `event #${total} ingested`,
        );
      }
    })().catch((err) => logger.error({ err }, 'Failed to ingest event'));
  }, INTERVAL_MS);

  // graceful shutdown
  for (const sig of ['SIGINT', 'SIGTERM'] as const) {
    process.on(sig, () => {
      logger.info(`收到 ${sig}，停止 ingestion...`);
      clearInterval(timer);
      void disconnectMongo().finally(() => process.exit(0));
    });
  }
}

main().catch((err) => {
  logger.error({ err }, 'ingestion service failed to start');
  process.exit(1);
});
