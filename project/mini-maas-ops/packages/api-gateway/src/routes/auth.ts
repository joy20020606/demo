/**
 * POST /auth/login — 驗證帳密、簽 JWT 回傳。
 *
 * 流程：
 *   0. 限流（每 IP 15 分鐘最多 5 次，防暴力破解）
 *   1. Zod 驗 input
 *   2. 找使用者（含 tenant 關聯）
 *   3. bcrypt 比對密碼
 *   4. 簽 JWT（payload 含 userId / tenantId / role / email）
 */
import type { FastifyInstance } from 'fastify';
import bcrypt from 'bcryptjs';
import { loginSchema, AuthError } from '@maas/shared';
import type { JwtPayload } from '@maas/shared';
import { rateLimit } from '../middleware/rate-limit.js';

export default async function authRoutes(app: FastifyInstance) {
  app.post(
    '/login',
    {
      // 登入限流較嚴格：每 IP 15 分鐘最多 5 次
      preHandler: rateLimit({ max: 5, windowSeconds: 900, keyPrefix: 'login' }),
    },
    async (request, reply) => {
      const parsed = loginSchema.safeParse(request.body);
      if (!parsed.success) {
        return reply.code(400).send({
          error: {
            code: 'VALIDATION_ERROR',
            message: 'Invalid login payload',
            details: parsed.error.flatten().fieldErrors,
          },
        });
      }
      const { email, password } = parsed.data;

      const user = await app.prisma.user.findUnique({
        where: { email },
        include: { tenant: true },
      });

      if (!user) throw new AuthError('Invalid credentials');

      const passwordOk = await bcrypt.compare(password, user.passwordHash);
      if (!passwordOk) throw new AuthError('Invalid credentials');

      const payload: JwtPayload = {
        userId: user.id,
        tenantId: user.tenantId,
        role: user.role,
        email: user.email,
      };

      const token = app.jwt.sign(payload);

      return {
        data: {
          token,
          user: {
            id: user.id,
            email: user.email,
            role: user.role,
            tenant: { id: user.tenant.id, name: user.tenant.name },
          },
        },
      };
    },
  );
}
