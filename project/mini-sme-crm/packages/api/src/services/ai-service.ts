/**
 * AI Service — 包裝 Claude API 呼叫，跟 DB 互動
 *
 * 為什麼這層存在（而不是 route 直接 call @sme-crm/ai）？
 * - AI 函式需要先從 DB 撈資料（contacts、deal 相關），這邏輯應該在 service 層
 * - 評分結果要寫回 DB（cache），avoid 重複 call
 * - Phase 7 Agent 也要呼叫這層（同一個 entry point）
 */
import type { PrismaClient } from '@sme-crm/db';
import { summarizeContacts, scoreDeal } from '@sme-crm/ai';
import { NotFoundError } from '@sme-crm/shared';

export class AiService {
  constructor(private readonly prisma: PrismaClient) {}

  /**
   * 摘要某客戶的所有聯絡紀錄
   * 不存進 DB（每次呼叫都重算，使用者按按鈕才觸發）
   */
  async summarizeCustomerContacts(customerId: string) {
    const customer = await this.prisma.customer.findUnique({
      where: { id: customerId },
      include: {
        contacts: {
          orderBy: { occurredAt: 'desc' },
          take: 50, // 最多餵 50 筆給 LLM，避免 prompt 過大
        },
      },
    });

    if (!customer) {
      throw new NotFoundError('Customer', customerId);
    }

    if (customer.contacts.length === 0) {
      return {
        currentStatus: '尚無聯絡紀錄，請先建立至少一筆紀錄再產生摘要。',
        nextActions: [],
        raw: '',
        contactCount: 0,
      };
    }

    const summary = await summarizeContacts(
      customer.name,
      customer.contacts.map((c) => ({
        channel: c.channel,
        summary: c.summary,
        occurredAt: c.occurredAt,
      })),
    );

    return { ...summary, contactCount: customer.contacts.length };
  }

  /**
   * 給商機評分，結果寫回 DB（aiScore / aiScoreReason）
   */
  async scoreDealById(dealId: string) {
    const deal = await this.prisma.deal.findUnique({
      where: { id: dealId },
      include: {
        customer: {
          include: {
            contacts: {
              orderBy: { occurredAt: 'desc' },
              take: 1, // 只要最近一筆判斷「daysSinceLastContact」
            },
          },
        },
      },
    });

    if (!deal) {
      throw new NotFoundError('Deal', dealId);
    }

    const lastContact = deal.customer.contacts[0];
    const daysSinceLastContact = lastContact
      ? Math.floor(
          (Date.now() - lastContact.occurredAt.getTime()) / (1000 * 60 * 60 * 24),
        )
      : 999; // 沒互動過

    const contactCount = await this.prisma.contactLog.count({
      where: { customerId: deal.customerId },
    });

    const result = await scoreDeal({
      title: deal.title,
      amount: Number(deal.amount),
      stage: deal.stage,
      contactCount,
      daysSinceLastContact,
    });

    // 寫回 DB
    const updated = await this.prisma.deal.update({
      where: { id: dealId },
      data: {
        aiScore: result.score,
        aiScoreReason: result.reason,
      },
    });

    return {
      ...result,
      dealId: updated.id,
      scoredAt: updated.updatedAt,
    };
  }
}
