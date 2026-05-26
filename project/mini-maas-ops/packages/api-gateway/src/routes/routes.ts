/**
 * Routes CRUD endpoints — 所有操作強制 tenant filter。
 *
 *   GET    /api/routes        列出（本租戶，加 Redis 快取）
 *   GET    /api/routes/:id    取單筆（必須屬於本租戶）
 *   POST   /api/routes        建立（自動塞 tenantId，並讓該租戶的列表快取失效）
 */
import type { FastifyInstance } from 'fastify';
import { authenticate, requireTenant } from '../middleware/auth.js';
import {
  createRouteSchema,
  idParamSchema,
  paginationSchema,
  NotFoundError,
  ValidationError,
} from '@maas/shared';
import { cached, invalidatePrefix } from '../lib/cache.js';

const ROUTES_CACHE_TTL = 60; // 快取存活 60 秒

export default async function routeRoutes(app: FastifyInstance) {
  // 整個 route group 都要 JWT 驗證
  app.addHook('preHandler', authenticate);

  // ----- GET /api/routes（加 Redis 快取）-----
  app.get('/', async (request) => {
    const tenantId = requireTenant(request);
    const { page, pageSize } = paginationSchema.parse(request.query);

    // 快取 key 帶 tenantId + 分頁參數，確保不同租戶 / 不同頁互不污染
    const cacheKey = `cache:routes:${tenantId}:${page}:${pageSize}`;

    const { value, hit } = await cached(app.redis, cacheKey, ROUTES_CACHE_TTL, async () => {
      const [items, total] = await Promise.all([
        app.prisma.route.findMany({
          where: { tenantId },
          skip: (page - 1) * pageSize,
          take: pageSize,
          orderBy: { createdAt: 'desc' },
        }),
        app.prisma.route.count({ where: { tenantId } }),
      ]);
      return {
        items,
        pagination: { page, pageSize, total, totalPages: Math.ceil(total / pageSize) },
      };
    });

    // meta.cache 讓你從回應直接看出這次是 HIT 還是 MISS（demo / debug 用）
    return { data: value, meta: { cache: hit ? 'HIT' : 'MISS' } };
  });

  // ----- GET /api/routes/:id -----
  app.get('/:id', async (request) => {
    const tenantId = requireTenant(request);
    const { id } = idParamSchema.parse(request.params);

    const route = await app.prisma.route.findFirst({
      where: { id, tenantId }, // 雙重保證：id 對 + tenantId 對
    });

    if (!route) throw new NotFoundError('Route');

    return { data: route };
  });

  // ----- POST /api/routes -----
  app.post('/', async (request, reply) => {
    const tenantId = requireTenant(request);
    const parsed = createRouteSchema.safeParse(request.body);
    if (!parsed.success) {
      throw new ValidationError('Invalid route payload', parsed.error.flatten().fieldErrors);
    }

    const created = await app.prisma.route.create({
      data: { ...parsed.data, tenantId },
    });

    // 新增路線後，資料變了 → 讓該租戶所有列表快取失效
    await invalidatePrefix(app.redis, `cache:routes:${tenantId}:`);

    return reply.code(201).send({ data: created });
  });
}
