/**
 * @sme-crm/db — Prisma client wrapper
 *
 * 統一從這裡 import PrismaClient，方便日後加上 logging / metrics / soft delete 等中介層
 */

import { PrismaClient } from '@prisma/client';

declare global {
  // eslint-disable-next-line no-var
  var __prisma: PrismaClient | undefined;
}

// 開發環境避免 hot reload 時建立多個 client（會耗盡 DB 連線）
export const prisma: PrismaClient =
  global.__prisma ??
  new PrismaClient({
    log: process.env.NODE_ENV === 'development' ? ['query', 'error', 'warn'] : ['error'],
  });

if (process.env.NODE_ENV !== 'production') {
  global.__prisma = prisma;
}

// Runtime export — Prisma namespace 含 Decimal class、WhereInput types
export { Prisma } from '@prisma/client';

// 型別 re-export — 讓其他 package 可以 import Prisma model types
export type {
  PrismaClient,
  Customer,
  Deal,
  ContactLog,
  DealStage,
  ContactChannel,
} from '@prisma/client';
