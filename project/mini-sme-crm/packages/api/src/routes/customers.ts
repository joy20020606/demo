/**
 * Customer routes — 5 個 HTTP endpoint
 *
 *   GET    /api/customers          列表（含分頁 + 搜尋）
 *   GET    /api/customers/:id      單筆
 *   POST   /api/customers          建立
 *   PATCH  /api/customers/:id      部分更新
 *   DELETE /api/customers/:id      刪除
 *
 * 設計：
 * - 用 fastify-type-provider-zod，每條 route 的 schema 只寫一次，同時拿到：
 *   1) request 自動驗證（驗失敗自動 throw → errorHandler 回 400）
 *   2) req.body / req.query / req.params 完整型別推導
 *   3) Swagger UI 文件（/docs）自動產出
 * - Route 層只負責「轉發到 service + 回 status code」
 */
import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { CreateCustomerSchema, CustomerSchema } from '@sme-crm/shared';
import { CustomerService } from '../services/customer-service.js';

// ---- 路由專屬 schema ----
const ParamsSchema = z.object({
  id: z.string().uuid(),
});

const ListQuerySchema = z.object({
  search: z.string().optional(),
  limit: z.coerce.number().int().positive().max(200).default(50),
  offset: z.coerce.number().int().nonnegative().default(0),
});

const ListResponseSchema = z.object({
  items: z.array(CustomerSchema),
  total: z.number().int().nonnegative(),
  limit: z.number().int().positive(),
  offset: z.number().int().nonnegative(),
});

const PatchBodySchema = CreateCustomerSchema.partial();

const ErrorResponseSchema = z.object({
  error: z.string(),
  message: z.string(),
  details: z.unknown().optional(),
});

// ---- Plugin 註冊 ----
export async function registerCustomerRoutes(app: FastifyInstance) {
  const service = new CustomerService(app.prisma);

  // 拿一個帶 Zod 型別推導的 app handle，下面 route 全用 r
  const r = app.withTypeProvider<ZodTypeProvider>();

  // GET /api/customers
  r.get(
    '/api/customers',
    {
      schema: {
        tags: ['customers'],
        summary: '列出客戶',
        description: '支援模糊搜尋（name / company / email）+ 分頁',
        querystring: ListQuerySchema,
        response: {
          200: ListResponseSchema,
        },
      },
    },
    async (req) => service.list(req.query),
  );

  // GET /api/customers/:id
  r.get(
    '/api/customers/:id',
    {
      schema: {
        tags: ['customers'],
        summary: '取得單一客戶',
        params: ParamsSchema,
        response: {
          200: CustomerSchema,
          404: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.getById(req.params.id),
  );

  // POST /api/customers
  r.post(
    '/api/customers',
    {
      schema: {
        tags: ['customers'],
        summary: '建立客戶',
        body: CreateCustomerSchema,
        response: {
          201: CustomerSchema,
          400: ErrorResponseSchema,
        },
      },
    },
    async (req, reply) => {
      const created = await service.create(req.body);
      return reply.code(201).send(created);
    },
  );

  // PATCH /api/customers/:id
  r.patch(
    '/api/customers/:id',
    {
      schema: {
        tags: ['customers'],
        summary: '更新客戶（部分欄位）',
        params: ParamsSchema,
        body: PatchBodySchema,
        response: {
          200: CustomerSchema,
          404: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.update(req.params.id, req.body),
  );

  // DELETE /api/customers/:id
  r.delete(
    '/api/customers/:id',
    {
      schema: {
        tags: ['customers'],
        summary: '刪除客戶',
        params: ParamsSchema,
        response: {
          204: z.null(),
          404: ErrorResponseSchema,
        },
      },
    },
    async (req, reply) => {
      await service.delete(req.params.id);
      return reply.code(204).send();
    },
  );
}
