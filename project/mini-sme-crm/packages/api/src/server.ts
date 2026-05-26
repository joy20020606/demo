/**
 * Fastify API server
 *
 * 啟動：pnpm dev（從 root 跑，會 tsx watch 自動重啟）
 * 部署：Railway，build 後跑 node dist/server.js
 */
import Fastify from 'fastify';
import cors from '@fastify/cors';
import sensible from '@fastify/sensible';
import { registerHealthRoutes } from './routes/health.js';

const PORT = Number(process.env.API_PORT ?? 3001);
const HOST = process.env.API_HOST ?? '0.0.0.0';

async function buildServer() {
  const app = Fastify({
    logger: {
      level: process.env.NODE_ENV === 'production' ? 'info' : 'debug',
      transport:
        process.env.NODE_ENV === 'production'
          ? undefined
          : { target: 'pino-pretty' },
    },
  });

  await app.register(cors, {
    origin: process.env.NEXT_PUBLIC_API_URL ? true : '*',
  });
  await app.register(sensible);

  // 路由
  await registerHealthRoutes(app);

  return app;
}

async function start() {
  const app = await buildServer();
  try {
    await app.listen({ port: PORT, host: HOST });
    app.log.info(`🚀 api listening on http://${HOST}:${PORT}`);
  } catch (err) {
    app.log.error(err);
    process.exit(1);
  }
}

start();
