import { z } from 'zod';

/**
 * Deal — 商機（業務追蹤的銷售案件）
 */
export const DealStageSchema = z.enum([
  'LEAD',          // 名單
  'CONTACTED',     // 已聯絡
  'PROPOSAL',      // 已報價
  'NEGOTIATION',   // 議價中
  'WON',           // 成交
  'LOST',          // 失敗
]);
export type DealStage = z.infer<typeof DealStageSchema>;

export const DealSchema = z.object({
  id: z.string().uuid(),
  customerId: z.string().uuid(),
  title: z.string().min(1).max(200),
  amount: z.number().nonnegative(),
  stage: DealStageSchema,
  // .nullish()：DB optional 欄位 Prisma 回 null，前端建立時可省略（undefined）
  aiScore: z.number().min(0).max(100).nullish(),
  aiScoreReason: z.string().nullish(),
  expectedCloseAt: z.coerce.date().nullish(),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
});

export type Deal = z.infer<typeof DealSchema>;

export const CreateDealSchema = DealSchema.omit({
  id: true,
  aiScore: true,
  aiScoreReason: true,
  createdAt: true,
  updatedAt: true,
});
export type CreateDealInput = z.infer<typeof CreateDealSchema>;
