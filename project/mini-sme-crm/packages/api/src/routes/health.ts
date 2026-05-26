import type { FastifyInstance } from 'fastify';

/**
 * Health check — Railway / Vercel / Kubernetes 都會打這個 endpoint 判斷服務存活
 */
export async function registerHealthRoutes(app: FastifyInstance) {
  app.get('/health', async () => ({
    status: 'ok',
    service: 'mini-sme-crm-api',
    timestamp: new Date().toISOString(),
  }));
}
