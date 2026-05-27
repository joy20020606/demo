/**
 * Deal Service — 商機業務邏輯層
 *
 * 跟 CustomerService 同套設計：constructor 注入 PrismaClient，
 * 對外只暴露純業務 method，不知道 HTTP 存在。
 *
 * 比 CustomerService 多了：
 * - transitionStage()：狀態機轉移，含「終態（WON/LOST）不能再轉」規則
 * - list() 支援 stage / customerId 兩個過濾條件（Kanban 板需要按 stage 撈）
 * - Prisma Decimal → number 序列化（amount 欄位 DB 存 Decimal，對外給 number）
 */
import { Prisma, type PrismaClient, type Deal as PrismaDeal } from '@sme-crm/db';
import {
  ConflictError,
  NotFoundError,
  type CreateDealInput,
  type DealStage,
} from '@sme-crm/shared';

export interface ListDealsOptions {
  stage?: DealStage;
  customerId?: string;
  limit?: number;
  offset?: number;
}

/**
 * 終態 — 進入這兩個 stage 就鎖死，不能再轉
 * 業務上：成交跟失敗就結案，不該回頭。要重開請建新 Deal。
 */
const TERMINAL_STAGES: DealStage[] = ['WON', 'LOST'];

/**
 * Prisma 的 Decimal 不能直接 JSON 序列化（會出現怪格式），
 * 統一在 service 層轉成 number 對外。
 */
function serialize(deal: PrismaDeal) {
  return {
    ...deal,
    amount: deal.amount instanceof Prisma.Decimal
      ? deal.amount.toNumber()
      : Number(deal.amount),
  };
}

export class DealService {
  constructor(private readonly prisma: PrismaClient) {}

  async list(opts: ListDealsOptions = {}) {
    const { stage, customerId, limit = 50, offset = 0 } = opts;

    const where: Prisma.DealWhereInput = {
      ...(stage ? { stage } : {}),
      ...(customerId ? { customerId } : {}),
    };

    const [items, total] = await Promise.all([
      this.prisma.deal.findMany({
        where,
        skip: offset,
        take: limit,
        orderBy: { createdAt: 'desc' },
      }),
      this.prisma.deal.count({ where }),
    ]);

    return {
      items: items.map(serialize),
      total,
      limit,
      offset,
    };
  }

  async getById(id: string) {
    const deal = await this.prisma.deal.findUnique({ where: { id } });
    if (!deal) {
      throw new NotFoundError('Deal', id);
    }
    return serialize(deal);
  }

  async create(input: CreateDealInput) {
    // 確認客戶存在，避免 FK 錯誤直接噴 500
    const customer = await this.prisma.customer.findUnique({
      where: { id: input.customerId },
    });
    if (!customer) {
      throw new NotFoundError('Customer', input.customerId);
    }

    const created = await this.prisma.deal.create({
      data: {
        customerId: input.customerId,
        title: input.title,
        amount: new Prisma.Decimal(input.amount),
        stage: input.stage,
        expectedCloseAt: input.expectedCloseAt,
      },
    });
    return serialize(created);
  }

  /**
   * 一般欄位更新（不含 stage）。
   * Stage 轉移走另一個專屬 method transitionStage，
   * 避免不小心繞過狀態機規則。
   */
  async update(id: string, input: Partial<Omit<CreateDealInput, 'stage'>>) {
    await this.getById(id);

    const data: Prisma.DealUpdateInput = {
      ...(input.title !== undefined ? { title: input.title } : {}),
      ...(input.amount !== undefined
        ? { amount: new Prisma.Decimal(input.amount) }
        : {}),
      ...(input.expectedCloseAt !== undefined
        ? { expectedCloseAt: input.expectedCloseAt }
        : {}),
      ...(input.customerId !== undefined
        ? { customer: { connect: { id: input.customerId } } }
        : {}),
    };

    const updated = await this.prisma.deal.update({ where: { id }, data });
    return serialize(updated);
  }

  /**
   * 狀態機轉移 — 唯一改 stage 的入口
   *
   * 規則：
   * 1. 目前在終態（WON / LOST）→ 拒絕（409 Conflict）
   * 2. 轉到同一個 stage → no-op，直接回現狀
   * 3. 其他狀態之間自由轉（業務上不限定順序，例如可以從 LEAD 直接跳 NEGOTIATION）
   */
  async transitionStage(id: string, nextStage: DealStage) {
    const current = await this.getById(id);

    if (TERMINAL_STAGES.includes(current.stage)) {
      throw new ConflictError(
        `Deal ${id} is in terminal stage (${current.stage}) and cannot transition`,
      );
    }

    if (current.stage === nextStage) {
      return current; // no-op
    }

    const updated = await this.prisma.deal.update({
      where: { id },
      data: { stage: nextStage },
    });
    return serialize(updated);
  }

  async delete(id: string) {
    await this.getById(id);
    await this.prisma.deal.delete({ where: { id } });
    return { id };
  }
}
