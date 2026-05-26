/**
 * @sme-crm/ai — AI 能力層（用 Claude API）
 *
 * 提供兩個核心功能：
 * 1. summarizeContacts — 把多筆聯絡記錄濃縮成「客戶現況 + 下一步建議」
 * 2. scoreDeal — 給商機打 0-100 分 + 評分理由
 */

export * from './client.js';
export * from './summarize-contacts.js';
export * from './score-deal.js';
