/**
 * Fastify plugin — 把 PrismaClient 注入 Fastify instance
 *
 * 用法：
 *   await app.register(prismaPlugin)
 *   // 之後在 route handler 用 fastify.prisma 或 request.server.prisma
 *
 * 為什麼用 plugin 而不直接 import？
 * 1. 生命週期管理：Fastify 關閉時自動 disconnect
 * 2. 測試可注入 mock client
 * 3. 整個 app 共用一個 PrismaClient 實例（避免連線池爆掉）
 */
import fp from 'fastify-plugin';
import type { FastifyPluginAsync } from 'fastify';
import { prisma } from '@sme-crm/db';
import type { PrismaClient } from '@sme-crm/db';

// 擴充 Fastify 的型別，讓 app.prisma 有型別提示
declare module 'fastify' {
  interface FastifyInstance {
    prisma: PrismaClient;
  }
}

const prismaPlugin: FastifyPluginAsync = async (app) => {
  // 連線測試（fail-fast：DB 連不上就讓 app 起不來）
  await prisma.$connect();
  app.log.info('✅ Prisma connected to PostgreSQL');

  app.decorate('prisma', prisma);

  // Fastify 關閉時 disconnect
  app.addHook('onClose', async (instance) => {
    await instance.prisma.$disconnect();
    instance.log.info('Prisma disconnected');
  });
};

// fp() 把 plugin 標記為「不要建立隔離 scope」，這樣 decorate 的 prisma 全 app 都看得到
export default fp(prismaPlugin, { name: 'prisma' });
