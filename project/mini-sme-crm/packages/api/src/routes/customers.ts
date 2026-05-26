/**
 * Customer routes — 5 個 HTTP endpoint
 *
 *   GET    /api/customers          列表（含分頁 + 搜尋）
 *   GET    /api/customers/:id      單筆
 *   POST   /api/customers          建立
 *   PATCH  /api/customers/:id      部分更新
 *   DELETE /api/customers/:id      刪除
 *
 * Route 層只做：
 * 1. 驗證 request（Zod schema）
 * 2. 呼叫 service
 * 3. 回對應 status code
 */
import type { FastifyInstance } from 'fastify';
import { z } from 'zod';
import { CreateCustomerSchema } from '@sme-crm/shared';
import { CustomerService } from '../services/customer-service.js';
import { parseBody, parseParams, parseQuery } from '../plugins/validation.js';

// ---- 路由專屬 schema ----
const ParamsSchema = z.object({
  id: z.string().uuid(),
});

const ListQuerySchema = z.object({
  search: z.string().optional(),
  limit: z.coerce.number().int().positive().max(200).default(50),
  offset: z.coerce.number().int().nonnegative().default(0),
});

const PatchBodySchema = CreateCustomerSchema.partial();

// ---- Plugin 註冊 ----
export async function registerCustomerRoutes(app: FastifyInstance) {
  const service = new CustomerService(app.prisma);

  // GET /api/customers
  app.get('/api/customers', async (req) => {
    const q = parseQuery(req, ListQuerySchema);
    return service.list(q);
  });

  // GET /api/customers/:id
  app.get('/api/customers/:id', async (req) => {
    const { id } = parseParams(req, ParamsSchema);
    return service.getById(id);
  });

  // POST /api/customers
  app.post('/api/customers', async (req, reply) => {
    const body = parseBody(req, CreateCustomerSchema);
    const created = await service.create(body);
    return reply.code(201).send(created);
  });

  // PATCH /api/customers/:id
  app.patch('/api/customers/:id', async (req) => {
    const { id } = parseParams(req, ParamsSchema);
    const body = parseBody(req, PatchBodySchema);
    return service.update(id, body);
  });

  // DELETE /api/customers/:id
  app.delete('/api/customers/:id', async (req, reply) => {
    const { id } = parseParams(req, ParamsSchema);
    await service.delete(id);
    return reply.code(204).send();
  });
}
