/**
 * Fastify server entry point.
 * 啟動順序：載入 .env → 建立 Fastify → 註冊 plugins → 註冊 routes → listen
 */
import 'dotenv/config';
import Fastify from 'fastify';
import cors from '@fastify/cors';
import sensible from '@fastify/sensible';

import { config } from './config.js';
import { logger } from '@maas/shared';
import { AppError } from '@maas/shared';

import prismaPlugin from './plugins/prisma.js';
import mongoPlugin from './plugins/mongo.js';
import redisPlugin from './plugins/redis.js';
import jwtPlugin from './plugins/jwt.js';

import { rateLimit } from './middleware/rate-limit.js';

import healthRoutes from './routes/health.js';
import authRoutes from './routes/auth.js';
import routeRoutes from './routes/routes.js';
import eventRoutes from './routes/events.js';

async function buildApp() {
  const app = Fastify({
    logger,
    disableRequestLogging: false,
  });

  // ----- Core plugins -----
  await app.register(cors, {
    origin: true, // Dev 允許所有 origin；正式環境改成白名單
    credentials: true,
  });
  await app.register(sensible);

  // ----- Infrastructure plugins（三資料庫）-----
  await app.register(prismaPlugin); // PostgreSQL
  await app.register(mongoPlugin);  // MongoDB
  await app.register(redisPlugin);  // Redis
  await app.register(jwtPlugin);

  // ----- Global rate limit（用 Redis，所以放在 redisPlugin 之後）-----
  // 每 IP 每分鐘最多 300 次請求；超過回 429
  app.addHook('onRequest', rateLimit({ max: 300, windowSeconds: 60, keyPrefix: 'global' }));

  // ----- Routes -----
  await app.register(healthRoutes);
  await app.register(authRoutes, { prefix: '/auth' });
  await app.register(routeRoutes, { prefix: '/api/routes' });
  await app.register(eventRoutes, { prefix: '/api/events' });

  // ----- Global error handler -----
  // 把 AppError 翻成 HTTP 回應；未知錯誤吃成 500。
  app.setErrorHandler((error, request, reply) => {
    request.log.error({ err: error }, 'Request failed');

    if (error instanceof AppError) {
      return reply.code(error.statusCode).send({
        error: {
          code: error.code,
          message: error.message,
          details: error.details,
        },
      });
    }

    // Zod validation errors（Fastify schema 驗證失敗）
    if (error.validation) {
      return reply.code(400).send({
        error: {
          code: 'VALIDATION_ERROR',
          message: 'Invalid request payload',
          details: error.validation,
        },
      });
    }

    // Fallback
    return reply.code(error.statusCode ?? 500).send({
      error: {
        code: 'INTERNAL_ERROR',
        message: config.NODE_ENV === 'production' ? 'Internal server error' : error.message,
      },
    });
  });

  return app;
}

async function start() {
  const app = await buildApp();

  try {
    await app.listen({ port: config.API_PORT, host: config.API_HOST });
    app.log.info(`🚀 api-gateway listening on http://${config.API_HOST}:${config.API_PORT}`);
  } catch (err) {
    app.log.fatal({ err }, 'Failed to start server');
    process.exit(1);
  }

  // Graceful shutdown
  const signals = ['SIGINT', 'SIGTERM'] as const;
  for (const signal of signals) {
    process.on(signal, async () => {
      app.log.info(`Received ${signal}, shutting down gracefully...`);
      await app.close();
      process.exit(0);
    });
  }
}

start();

export { buildApp };
