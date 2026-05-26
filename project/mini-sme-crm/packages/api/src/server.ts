/**
 * Fastify API server
 *
 * 啟動：pnpm dev:api（從 root 跑，會 tsx watch 自動重啟）
 * 部署：Railway，build 後跑 node dist/server.js
 */
import Fastify from 'fastify';
import cors from '@fastify/cors';
import sensible from '@fastify/sensible';
import fastifySwagger from '@fastify/swagger';
import fastifySwaggerUi from '@fastify/swagger-ui';
import {
  jsonSchemaTransform,
  serializerCompiler,
  validatorCompiler,
} from 'fastify-type-provider-zod';
import prismaPlugin from './plugins/prisma.js';
import { setupErrorHandler } from './plugins/validation.js';
import { registerHealthRoutes } from './routes/health.js';
import { registerCustomerRoutes } from './routes/customers.js';

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

  // ---- Zod 當作 Fastify 的 validator / serializer ----
  // 設定以後，每條 route 上面 schema: { body: ZodSchema, ... } 會自動驗證 + 推導型別
  app.setValidatorCompiler(validatorCompiler);
  app.setSerializerCompiler(serializerCompiler);

  // ---- 基礎 plugin ----
  await app.register(cors, {
    origin: process.env.NEXT_PUBLIC_API_URL ? true : '*',
  });
  await app.register(sensible);

  // ---- Swagger / OpenAPI ----
  // 把 Zod schema 轉成 OpenAPI spec，再丟給 Swagger UI 渲染
  await app.register(fastifySwagger, {
    openapi: {
      info: {
        title: 'Mini SME CRM API',
        description: 'AI-powered CRM for Taiwanese SMEs',
        version: '0.1.0',
      },
      servers: [{ url: `http://localhost:${PORT}` }],
      tags: [
        { name: 'health', description: '健康檢查' },
        { name: 'customers', description: '客戶 CRUD' },
      ],
    },
    transform: jsonSchemaTransform,
  });

  await app.register(fastifySwaggerUi, {
    routePrefix: '/docs',
    uiConfig: {
      docExpansion: 'list',
      deepLinking: true,
    },
  });

  // ---- 基礎設施 plugin（DB） ----
  await app.register(prismaPlugin);

  // ---- 全域 error handler ----
  await setupErrorHandler(app);

  // ---- 路由 ----
  await registerHealthRoutes(app);
  await registerCustomerRoutes(app);

  return app;
}

async function start() {
  const app = await buildServer();
  try {
    await app.listen({ port: PORT, host: HOST });
    app.log.info(`🚀 api listening on http://${HOST}:${PORT}`);
    app.log.info(`📚 swagger ui at http://localhost:${PORT}/docs`);
  } catch (err) {
    app.log.error(err);
    process.exit(1);
  }
}

start();
