/**
 * Redis plugin — 提供快取與限流所需的 Redis 連線。
 *
 * 連好後 decorate 到 app，route / middleware 用 app.redis 或
 * request.server.redis 取用。
 */
import fp from 'fastify-plugin';
import { Redis } from 'ioredis';
import { config } from '../config.js';

declare module 'fastify' {
  interface FastifyInstance {
    redis: Redis;
  }
}

export default fp(async (fastify) => {
  const redis = new Redis(config.REDIS_URL, {
    maxRetriesPerRequest: 3,
  });

  redis.on('connect', () => fastify.log.info('✅ Redis connected'));
  redis.on('error', (err) => fastify.log.error({ err }, 'Redis error'));

  fastify.decorate('redis', redis);

  fastify.addHook('onClose', async () => {
    await redis.quit();
    fastify.log.info('🛑 Redis disconnected');
  });
}, {
  name: 'redis-plugin',
});
