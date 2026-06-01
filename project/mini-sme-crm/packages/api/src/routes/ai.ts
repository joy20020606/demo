/**
 * AI routes — 觸發 Claude API 呼叫
 *
 *   POST /api/customers/:id/ai-summary   產生客戶現況摘要（不存 DB）
 *   POST /api/deals/:id/ai-score          給商機評分（存進 DB）
 *
 * 用 POST 不用 GET：這些是「指令」而非「查詢」，會花錢 + 修改 state（評分）。
 */
import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { AiService } from '../services/ai-service.js';

const IdParamsSchema = z.object({
  id: z.string().uuid(),
});

const SummaryResponseSchema = z.object({
  currentStatus: z.string(),
  nextActions: z.array(z.string()),
  raw: z.string(),
  contactCount: z.number().int().nonnegative(),
});

const ScoreResponseSchema = z.object({
  dealId: z.string().uuid(),
  score: z.number().int().min(0).max(100),
  reason: z.string(),
  scoredAt: z.coerce.date(),
});

const ErrorResponseSchema = z.object({
  error: z.string(),
  message: z.string(),
  details: z.unknown().optional(),
});

export async function registerAiRoutes(app: FastifyInstance) {
  const service = new AiService(app.prisma);
  const r = app.withTypeProvider<ZodTypeProvider>();

  // POST /api/customers/:id/ai-summary
  r.post(
    '/api/customers/:id/ai-summary',
    {
      schema: {
        tags: ['ai'],
        summary: '產生客戶現況摘要（Claude API）',
        description:
          '把該客戶所有聯絡紀錄丟給 Claude 摘要成「現況 + 下一步」。每次呼叫都會花 Claude API token（按使用者按鈕觸發）。',
        params: IdParamsSchema,
        response: {
          200: SummaryResponseSchema,
          404: ErrorResponseSchema,
          500: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.summarizeCustomerContacts(req.params.id),
  );

  // POST /api/deals/:id/ai-score
  r.post(
    '/api/deals/:id/ai-score',
    {
      schema: {
        tags: ['ai'],
        summary: '給商機評分 0-100（Claude API，結果存進 DB）',
        description:
          '評分結果寫入 deal.aiScore + aiScoreReason，Kanban 卡片自動讀取最新分數。',
        params: IdParamsSchema,
        response: {
          200: ScoreResponseSchema,
          404: ErrorResponseSchema,
          500: ErrorResponseSchema,
        },
      },
    },
    async (req) => service.scoreDealById(req.params.id),
  );
}
