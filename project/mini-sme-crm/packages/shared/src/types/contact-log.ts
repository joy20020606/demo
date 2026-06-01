import { z } from 'zod';

/**
 * ContactLog — 跟客戶的每一筆互動紀錄
 * （業務打電話、發 email、開會、LINE 訊息...）
 *
 * 這個資料的價值：累積夠多筆後，可以用 AI 摘要 + 之後 RAG 語意搜尋
 */

export const ContactChannelSchema = z.enum([
  'EMAIL',
  'PHONE',
  'LINE',
  'MEETING',
  'OTHER',
]);
export type ContactChannel = z.infer<typeof ContactChannelSchema>;

export const ContactLogSchema = z.object({
  id: z.string().uuid(),
  customerId: z.string().uuid(),
  channel: ContactChannelSchema,
  summary: z.string().min(1).max(2000),
  occurredAt: z.coerce.date(),
  createdAt: z.coerce.date(),
});
export type ContactLog = z.infer<typeof ContactLogSchema>;

export const CreateContactLogSchema = ContactLogSchema.omit({
  id: true,
  createdAt: true,
});
export type CreateContactLogInput = z.infer<typeof CreateContactLogSchema>;
