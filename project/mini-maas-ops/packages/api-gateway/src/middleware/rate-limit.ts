/**
 * 限流 middleware — 用 Redis INCR + EXPIRE 做固定視窗（fixed window）限流。
 *
 * 原理：
 *   1. key 內含「時間視窗編號」，每個視窗一個獨立計數器
 *   2. 每個請求對計數器 INCR；第一次 INCR 時設 EXPIRE 讓 key 自動過期
 *   3. 計數超過上限 → 拋 429 Too Many Requests
 *
 * 為什麼用 Redis：計數器要跨多個 server 實例共享，且需要原子遞增 +
 * 自動過期 —— 這正是 Redis 的強項。
 */
import type { FastifyReply, FastifyRequest } from 'fastify';
import { AppError } from '@maas/shared';

export class RateLimitError extends AppError {
  constructor(windowSeconds: number) {
    super('RATE_LIMITED', `Too many requests. Retry after ${windowSeconds}s.`, 429);
  }
}

export interface RateLimitOptions {
  max: number;           // 視窗內允許的最大請求數
  windowSeconds: number; // 視窗長度（秒）
  keyPrefix: string;     // 區分不同限流規則（global / login...）
}

export function rateLimit(opts: RateLimitOptions) {
  return async function (request: FastifyRequest, _reply: FastifyReply): Promise<void> {
    const redis = request.server.redis;

    // 視窗編號：同一個時間視窗內的請求共用一個計數器
    const window = Math.floor(Date.now() / 1000 / opts.windowSeconds);
    const key = `ratelimit:${opts.keyPrefix}:${request.ip}:${window}`;

    const count = await redis.incr(key);
    if (count === 1) {
      // 第一次進這個視窗 → 設過期時間，讓計數器自動清掉
      await redis.expire(key, opts.windowSeconds);
    }
    if (count > opts.max) {
      throw new RateLimitError(opts.windowSeconds);
    }
  };
}
