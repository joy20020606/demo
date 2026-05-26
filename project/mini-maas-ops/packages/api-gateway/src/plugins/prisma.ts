/**
 * Prisma plugin — 把 PrismaClient 注入到 Fastify instance。
 * 之後在 route 內可以用 fastify.prisma 或 request.server.prisma 取用。
 */
import fp from 'fastify-plugin';
import { PrismaClient } from '@prisma/client';

declare module 'fastify' {
  interface FastifyInstance {
    prisma: PrismaClient;
  }
}

export default fp(async (fastify) => {
  const prisma = new PrismaClient({
    log: fastify.log.level === 'debug' ? ['query', 'info', 'warn', 'error'] : ['warn', 'error'],
  });

  await prisma.$connect();
  fastify.log.info('✅ Prisma connected to PostgreSQL');

  fastify.decorate('prisma', prisma);

  fastify.addHook('onClose', async () => {
    await prisma.$disconnect();
    fastify.log.info('🛑 Prisma disconnected');
  });
}, {
  name: 'prisma-plugin',
});
