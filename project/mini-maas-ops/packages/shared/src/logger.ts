/**
 * Structured logger using Pino.
 * 結構化 logger，JSON 格式輸出，方便日後接 ELK / Datadog / CloudWatch。
 *
 * 對應 Metropia JD 的 "結構化 log + observability" 要求。
 */
import pino from 'pino';

const isDev = process.env.NODE_ENV !== 'production';

export const logger = pino({
  level: process.env.LOG_LEVEL ?? 'info',
  // Dev 環境用人類可讀格式；Production 用 JSON
  transport: isDev
    ? {
        target: 'pino-pretty',
        options: {
          colorize: true,
          translateTime: 'HH:MM:ss Z',
          ignore: 'pid,hostname',
        },
      }
    : undefined,
  // 全域欄位：每筆 log 都會帶上
  base: {
    service: process.env.SERVICE_NAME ?? 'mini-maas-ops',
    env: process.env.NODE_ENV ?? 'development',
  },
});

export type Logger = typeof logger;
