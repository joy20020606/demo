/**
 * Customer Service — 業務邏輯層
 *
 * 設計準則：
 * - 接收 PrismaClient 當參數（依賴注入），方便測試替換 mock
 * - 對外暴露純業務 method（不知道 HTTP 存在）
 * - throw 業務錯誤（NotFoundError 等），由 route 層轉成 HTTP response
 *
 * 為什麼 service 不直接 import prisma？
 * 因為 Phase 7 Agent 也要呼叫這層，那時候 prisma 可能是 transaction client。
 * 把 prisma 當參數注入，service 才能在不同 context 下 reuse。
 */
import type { PrismaClient } from '@sme-crm/db';
import { NotFoundError, type CreateCustomerInput } from '@sme-crm/shared';

export interface ListCustomersOptions {
  search?: string;
  limit?: number;
  offset?: number;
}

export class CustomerService {
  constructor(private readonly prisma: PrismaClient) {}

  async list(opts: ListCustomersOptions = {}) {
    const { search, limit = 50, offset = 0 } = opts;

    const where = search
      ? {
          OR: [
            { name: { contains: search, mode: 'insensitive' as const } },
            { company: { contains: search, mode: 'insensitive' as const } },
            { email: { contains: search, mode: 'insensitive' as const } },
          ],
        }
      : {};

    const [items, total] = await Promise.all([
      this.prisma.customer.findMany({
        where,
        skip: offset,
        take: limit,
        orderBy: { createdAt: 'desc' },
      }),
      this.prisma.customer.count({ where }),
    ]);

    return { items, total, limit, offset };
  }

  async getById(id: string) {
    const customer = await this.prisma.customer.findUnique({ where: { id } });
    if (!customer) {
      throw new NotFoundError('Customer', id);
    }
    return customer;
  }

  async create(input: CreateCustomerInput) {
    return this.prisma.customer.create({
      data: {
        name: input.name,
        email: input.email,
        phone: input.phone,
        company: input.company,
        tags: input.tags ?? [],
      },
    });
  }

  async update(id: string, input: Partial<CreateCustomerInput>) {
    // 先確認存在（throw NotFound）
    await this.getById(id);

    return this.prisma.customer.update({
      where: { id },
      data: input,
    });
  }

  async delete(id: string) {
    await this.getById(id); // throw NotFound if 不存在
    await this.prisma.customer.delete({ where: { id } });
    return { id };
  }
}
