import { createAnthropicClient, DEFAULT_MODEL } from './client.js';

export interface ContactLogInput {
  channel: string;
  summary: string;
  occurredAt: Date;
}

export interface ContactSummary {
  currentStatus: string;
  nextActions: string[];
  raw: string;
}

/**
 * 把一系列聯絡記錄丟給 Claude，產出「客戶現況 + 下一步建議」
 *
 * Prompt 設計重點：
 * - 給 system prompt 設定角色（資深業務主管）
 * - 給結構化輸出指示（用 JSON）
 * - few-shot 不放這裡（這個任務 zero-shot 就夠了）
 */
export async function summarizeContacts(
  customerName: string,
  contacts: ContactLogInput[]
): Promise<ContactSummary> {
  const client = createAnthropicClient();

  const formattedContacts = contacts
    .map(
      (c, i) =>
        `${i + 1}. [${c.occurredAt.toISOString().slice(0, 10)}] (${c.channel}) ${c.summary}`
    )
    .join('\n');

  const response = await client.messages.create({
    model: DEFAULT_MODEL,
    max_tokens: 1024,
    system:
      '你是一位資深業務主管，擅長從零散的聯絡記錄中抓出客戶當前狀態與下一步該做什麼。' +
      '請用繁體中文回答，並嚴格輸出 JSON 格式：{"currentStatus": "...", "nextActions": ["...", "..."]}',
    messages: [
      {
        role: 'user',
        content: `客戶：${customerName}\n\n聯絡記錄：\n${formattedContacts}\n\n請輸出 JSON。`,
      },
    ],
  });

  const textBlock = response.content.find((b) => b.type === 'text');
  const raw = textBlock?.type === 'text' ? textBlock.text : '';

  // 嘗試解析 JSON（如果模型偶爾出包，回 fallback）
  try {
    const jsonMatch = raw.match(/\{[\s\S]*\}/);
    if (jsonMatch) {
      const parsed = JSON.parse(jsonMatch[0]) as Pick<
        ContactSummary,
        'currentStatus' | 'nextActions'
      >;
      return { ...parsed, raw };
    }
  } catch {
    // fall through
  }

  return {
    currentStatus: raw || '（AI 回覆解析失敗）',
    nextActions: [],
    raw,
  };
}
