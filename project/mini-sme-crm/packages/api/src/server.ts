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
import { registerDealRoutes } from './routes/deals.js';
import { registerContactLogRoutes } from './routes/contact-logs.js';
import { registerAiRoutes } from './routes/ai.js';

// Railway / Vercel 用 PORT，本地用 API_PORT
const PORT = Number(process.env.PORT ?? process.env.API_PORT ?? 3001);
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
  // CORS：production 限定 FRONTEND_URL（Vercel 域名），dev 全開
  // 也允許 Vercel preview deploy（*.vercel.app）方便看 PR 預覽
  await app.register(cors, {
    origin:
      process.env.NODE_ENV === 'production'
        ? [
            process.env.FRONTEND_URL ?? '',
            /\.vercel\.app$/,
          ].filter(Boolean)
        : true, // dev：全開
    credentials: true,
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
      servers: [
        {
          url:
            process.env.API_PUBLIC_URL ?? `http://localhost:${PORT}`,
        },
      ],
      tags: [
        { name: 'health', description: '健康檢查' },
        { name: 'customers', description: '客戶 CRUD' },
        { name: 'deals', description: '商機 CRUD + Kanban 狀態機' },
        { name: 'contact-logs', description: '客戶聯絡紀錄' },
        { name: 'ai', description: 'Claude API 摘要 + 評分' },
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
  await registerDealRoutes(app);
  await registerContactLogRoutes(app);
  await registerAiRoutes(app);

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
