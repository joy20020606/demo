/**
 * Deal routes — 6 個 HTTP endpoint
 *
 *   GET    /api/deals                列表（支援 ?stage / ?customerId 過濾 + 分頁）
 *   GET    /api/deals/:id            單筆
 *   POST   /api/deals                建立
 *   PATCH  /api/deals/:id            更新基本欄位（不含 stage）
 *   PATCH  /api/deals/:id/stage      狀態機轉移專屬入口
 *   DELETE /api/deals/:id
 *
 * 設計重點：
 * - Stage 轉移走獨立 endpoint，把狀態機規則隔離在 service.transitionStage()
 *   （前端 Kanban drag-drop 也只需要 call 這個 endpoint）
 * - amount 對外是 number，service 內部會跟 Prisma Decimal 互轉
 */
import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import {
  CreateDealSchema,
  DealSchema,
  DealStageSchema,
} from '@sme-crm/shared';
import { DealService } from '../services/deal-service.js';

// ---- 路由專屬 schema ----
const ParamsSchema = z.object({
  id: z.string().uuid(),
});

const ListQuerySchema = z.object({
  stage: DealStageSchema.optional(),
  customerId: z.string().uuid().optional(),
  limit: z.coerce.number().int().positive().max(200).default(50),
  offset: z.coerce.number().int().nonnegative().default(0),
});

const ListResponseSchema = z.object({
  items: z.array(DealSchema),
  total: z.number().int().nonnegative(),
  limit: z.number().int().positive(),
  offset: z.number().int().nonnegative(),
});

// 一般 PATCH 不允許改 stage，避免繞過狀態機規則
const PatchBodySchema = CreateDealSchema.omit({ stage: true }).partial();

// 狀態機轉移專屬 body
const TransitionStageBodySchema = z.object({
  stage: DealStageSchema,
});

const ErrorResponseSchema = z.object({
  error: z.string(),
  message: z.string(),
  details: z.unknown().optional(),
});

// ---- Plugin 註冊 ----
export async function registerDealRoutes(app: FastifyInstance) {
  const service = new DealService(app.prisma);
  const r = app.withTypeProvider<ZodTypeProvider>();

  // GET /api/deals
  r.get(
    '/api/deals',
    {
      schema: {
        tags: ['deals'],
        summary: '列出商機',
        description: '可用 stage / customerId 過濾。Kanban 看板就是用 stage 撈 6 次（或一次撈全部前端分組）',
        querystring: ListQuerySchema,
        response: { 200: ListResponseSchema },
      },
    },
    async (req) => service.list(req.query),
  );

  // GET /api/deals/:id
  r.get(
    '/api/deals/:id',
    {
      schema: {
        tags: ['deals'],
        summary: '取得單一商機',
        params: ParamsSchema,
        response: {
          200: DealSchema,
          404: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.getById(req.params.id),
  );

  // POST /api/deals
  r.post(
    '/api/deals',
    {
      schema: {
        tags: ['deals'],
        summary: '建立商機',
        description: 'customerId 不存在會回 404',
        body: CreateDealSchema,
        response: {
          201: DealSchema,
          400: ErrorResponseSchema,
          404: ErrorResponseSchema,
        },
      },
    },
    async (req, reply) => {
      const created = await service.create(req.body);
      return reply.code(201).send(created);
    },
  );

  // PATCH /api/deals/:id — 一般欄位更新（不含 stage）
  r.patch(
    '/api/deals/:id',
    {
      schema: {
        tags: ['deals'],
        summary: '更新商機基本欄位',
        description: '注意：改 stage 請用 PATCH /api/deals/:id/stage（走狀態機檢查）',
        params: ParamsSchema,
        body: PatchBodySchema,
        response: {
          200: DealSchema,
          404: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.update(req.params.id, req.body),
  );

  // PATCH /api/deals/:id/stage — 狀態機轉移
  r.patch(
    '/api/deals/:id/stage',
    {
      schema: {
        tags: ['deals'],
        summary: '轉移商機狀態（Kanban drag-drop 入口）',
        description:
          '規則：WON / LOST 是終態，不能再轉。其他狀態之間可自由轉。違規回 409 Conflict。',
        params: ParamsSchema,
        body: TransitionStageBodySchema,
        response: {
          200: DealSchema,
          404: ErrorResponseSchema,
          409: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.transitionStage(req.params.id, req.body.stage),
  );

  // DELETE /api/deals/:id
  r.delete(
    '/api/deals/:id',
    {
      schema: {
        tags: ['deals'],
        summary: '刪除商機',
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
