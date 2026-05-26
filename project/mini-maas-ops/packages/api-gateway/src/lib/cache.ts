/**
 * Cache-aside（旁路快取）輔助函式。
 *
 * 流程：先查 Redis → 命中就直接回（HIT）；
 *      沒命中 → 跑 loader 查真實資料 → 寫回 Redis → 回（MISS）。
 */
import type { Redis } from 'ioredis';

export interface CacheResult<T> {
  value: T;
  hit: boolean;
}

export async function cached<T>(
  redis: Redis,
  key: string,
  ttlSeconds: number,
  loader: () => Promise<T>,
): Promise<CacheResult<T>> {
  const raw = await redis.get(key);
  if (raw !== null) {
    return { value: JSON.parse(raw) as T, hit: true };
  }
  const value = await loader();
  // 'EX' = 設定過期秒數，TTL 到了 Redis 自動刪除
  await redis.set(key, JSON.stringify(value), 'EX', ttlSeconds);
  return { value, hit: false };
}

/**
 * 用 SCAN 安全地刪除某前綴的所有 key。
 * 不用 KEYS —— KEYS 在大資料量會阻塞整個 Redis；SCAN 是分批掃描。
 */
export async function invalidatePrefix(redis: Redis, prefix: string): Promise<number> {
  let cursor = '0';
  let deleted = 0;
  do {
    const [next, keys] = await redis.scan(cursor, 'MATCH', `${prefix}*`, 'COUNT', 100);
    cursor = next;
    if (keys.length > 0) {
      deleted += await redis.del(...keys);
    }
  } while (cursor !== '0');
  return deleted;
}
