/**
 * ContactLog Service
 *
 * 設計：
 * - 跟 Customer / Deal 同樣 constructor DI 模式
 * - 列表一律「by customerId」（沒有「所有客戶的聯絡紀錄」這種查詢）
 * - 建立時驗 customerId 存在
 */
import type { PrismaClient } from '@sme-crm/db';
import {
  NotFoundError,
  type CreateContactLogInput,
} from '@sme-crm/shared';

export class ContactLogService {
  constructor(private readonly prisma: PrismaClient) {}

  /**
   * 列出某客戶的所有聯絡紀錄（最新在前）
   */
  async listByCustomer(customerId: string) {
    // 先確認客戶存在
    const customer = await this.prisma.customer.findUnique({
      where: { id: customerId },
    });
    if (!customer) {
      throw new NotFoundError('Customer', customerId);
    }

    return this.prisma.contactLog.findMany({
      where: { customerId },
      orderBy: { occurredAt: 'desc' },
    });
  }

  async getById(id: string) {
    const log = await this.prisma.contactLog.findUnique({ where: { id } });
    if (!log) {
      throw new NotFoundError('ContactLog', id);
    }
    return log;
  }

  async create(input: CreateContactLogInput) {
    // 確認客戶存在，避免 FK 錯誤
    const customer = await this.prisma.customer.findUnique({
      where: { id: input.customerId },
    });
    if (!customer) {
      throw new NotFoundError('Customer', input.customerId);
    }

    return this.prisma.contactLog.create({
      data: {
        customerId: input.customerId,
        channel: input.channel,
        summary: input.summary,
        occurredAt: input.occurredAt,
      },
    });
  }

  async delete(id: string) {
    await this.getById(id);
    await this.prisma.contactLog.delete({ where: { id } });
    return { id };
  }
}
