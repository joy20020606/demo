import { z } from 'zod';

/**
 * Customer — 客戶基本資料
 * 用 Zod schema 同時定義「執行時驗證」+「靜態型別」
 */
export const CustomerSchema = z.object({
  id: z.string().uuid(),
  name: z.string().min(1).max(100),
  email: z.string().email().optional(),
  phone: z.string().optional(),
  company: z.string().optional(),
  tags: z.array(z.string()).default([]),
  createdAt: z.coerce.date(),
  updatedAt: z.coerce.date(),
});

export type Customer = z.infer<typeof CustomerSchema>;

export const CreateCustomerSchema = CustomerSchema.omit({
  id: true,
  createdAt: true,
  updatedAt: true,
});
export type CreateCustomerInput = z.infer<typeof CreateCustomerSchema>;
