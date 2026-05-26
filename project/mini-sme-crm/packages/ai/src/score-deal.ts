import { createAnthropicClient, DEFAULT_MODEL } from './client.js';

export interface ScoreDealInput {
  title: string;
  amount: number;
  stage: string;
  contactCount: number;
  daysSinceLastContact: number;
}

export interface DealScore {
  score: number;       // 0-100
  reason: string;      // 評分理由（一兩句話）
}

/**
 * 用 Claude 給商機打分（0-100），並給出理由
 *
 * 設計重點：
 * - 嚴格指定輸出格式（JSON）
 * - 給評分維度（金額、stage、互動頻率），避免模型自由發揮
 */
export async function scoreDeal(input: ScoreDealInput): Promise<DealScore> {
  const client = createAnthropicClient();

  const response = await client.messages.create({
    model: DEFAULT_MODEL,
    max_tokens: 512,
    system:
      '你是 B2B 業務分析師，根據商機資料給出 0-100 的成交可能性分數。' +
      '評分維度：金額大小、目前 stage、互動頻率、最近聯絡時間。' +
      '請嚴格輸出 JSON：{"score": <int 0-100>, "reason": "<一兩句中文理由>"}',
    messages: [
      {
        role: 'user',
        content: JSON.stringify(input, null, 2),
      },
    ],
  });

  const textBlock = response.content.find((b) => b.type === 'text');
  const raw = textBlock?.type === 'text' ? textBlock.text : '';

  try {
    const match = raw.match(/\{[\s\S]*\}/);
    if (match) {
      const parsed = JSON.parse(match[0]) as DealScore;
      return {
        score: Math.max(0, Math.min(100, Math.round(parsed.score))),
        reason: parsed.reason,
      };
    }
  } catch {
    // fall through
  }

  return { score: 0, reason: 'AI 評分失敗' };
}
