/**
 * GET /api/events — 從 MongoDB 讀取本租戶的通勤事件。
 *
 * 展示重點：同一個 API 服務，routes 走 PostgreSQL、events 走 MongoDB，
 * 各取所長 —— 這就是 polyglot persistence 落地的樣子。
 *
 * 支援 query：?routeId=xxx&eventType=CHECK_IN&page=1&pageSize=20
 */
import type { FastifyInstance } from 'fastify';
import { authenticate, requireTenant } from '../middleware/auth.js';
import { eventQuerySchema, CommuteEventModel } from '@maas/shared';

export default async function eventRoutes(app: FastifyInstance) {
  // 跟 routes 一樣，整組要 JWT 驗證
  app.addHook('preHandler', authenticate);

  // ----- GET /api/events -----
  app.get('/', async (request) => {
    const tenantId = requireTenant(request);
    const q = eventQuerySchema.parse(request.query);

    // 強制 tenantId 過濾 —— 多租戶隔離原則，跟 PostgreSQL 那邊一致
    const filter: Record<string, unknown> = { tenantId };
    if (q.routeId) filter.routeId = q.routeId;
    if (q.eventType) filter.eventType = q.eventType;

    const [items, total] = await Promise.all([
      CommuteEventModel.find(filter)
        .sort({ occurredAt: -1 })
        .skip((q.page - 1) * q.pageSize)
        .limit(q.pageSize)
        .lean(), // .lean() = 回傳純 JS 物件，不包成 Mongoose document，較快
      CommuteEventModel.countDocuments(filter),
    ]);

    return {
      data: {
        items,
        pagination: {
          page: q.page,
          pageSize: q.pageSize,
          total,
          totalPages: Math.ceil(total / q.pageSize),
        },
      },
    };
  });
}
