/**
 * ContactLog routes
 *
 *   GET    /api/customers/:customerId/contact-logs   列出某客戶的所有聯絡紀錄
 *   POST   /api/contact-logs                         建立
 *   DELETE /api/contact-logs/:id                     刪除
 *
 * 注意 URL 設計：
 * - list 走「客戶下面的子資源」：/api/customers/:customerId/contact-logs
 * - create 走獨立路徑：body 含 customerId（前端比較好寫）
 * - delete 走獨立路徑：只需要 log id
 */
import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import {
  ContactLogSchema,
  CreateContactLogSchema,
} from '@sme-crm/shared';
import { ContactLogService } from '../services/contact-log-service.js';

const IdParamsSchema = z.object({
  id: z.string().uuid(),
});

const CustomerIdParamsSchema = z.object({
  customerId: z.string().uuid(),
});

const ErrorResponseSchema = z.object({
  error: z.string(),
  message: z.string(),
  details: z.unknown().optional(),
});

export async function registerContactLogRoutes(app: FastifyInstance) {
  const service = new ContactLogService(app.prisma);
  const r = app.withTypeProvider<ZodTypeProvider>();

  // GET /api/customers/:customerId/contact-logs
  r.get(
    '/api/customers/:customerId/contact-logs',
    {
      schema: {
        tags: ['contact-logs'],
        summary: '列出某客戶的所有聯絡紀錄',
        params: CustomerIdParamsSchema,
        response: {
          200: z.array(ContactLogSchema),
          404: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.listByCustomer(req.params.customerId),
  );

  // POST /api/contact-logs
  r.post(
    '/api/contact-logs',
    {
      schema: {
        tags: ['contact-logs'],
        summary: '建立聯絡紀錄',
        body: CreateContactLogSchema,
        response: {
          201: ContactLogSchema,
          404: ErrorResponseSchema,
        },
      },
    },
    async (req, reply) => {
      const created = await service.create(req.body);
      return reply.code(201).send(created);
    },
  );

  // DELETE /api/contact-logs/:id
  r.delete(
    '/api/contact-logs/:id',
    {
      schema: {
        tags: ['contact-logs'],
        summary: '刪除聯絡紀錄',
        params: IdParamsSchema,
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
