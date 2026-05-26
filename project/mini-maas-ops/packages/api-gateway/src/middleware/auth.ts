/**
 * JWT 認證 + 多租戶上下文 middleware。
 *
 * 設計重點：
 * 1. authenticate：驗 JWT 簽章，把 payload 塞進 request.user
 * 2. requireTenant：從 request.user 取出 tenantId；沒有就拒絕（防呆）
 * 3. requireRole：角色檢查（ADMIN > OPERATOR > VIEWER）
 *
 * 之所以強制 tenantId：多租戶 SaaS 最常見的安全洞是「忘記過濾 tenantId」
 * 導致 A 公司資料被 B 公司看到。把它做成 middleware-level 保證。
 */
import type { FastifyReply, FastifyRequest } from 'fastify';
import { AuthError, ForbiddenError, TenantContextError, type UserRole } from '@maas/shared';

/**
 * 驗 JWT；失敗時拋 AuthError → 由 server.ts 的 errorHandler 接住。
 */
export async function authenticate(
  request: FastifyRequest,
  _reply: FastifyReply,
): Promise<void> {
  try {
    await request.jwtVerify();
  } catch {
    throw new AuthError('Invalid or expired token');
  }
}

/**
 * 從 request.user 中取出 tenantId，並回傳。
 * 必須在 authenticate 之後呼叫（authenticate 會把 payload 塞進 request.user）。
 */
export function requireTenant(request: FastifyRequest): string {
  const tenantId = request.user?.tenantId;
  if (!tenantId) {
    throw new TenantContextError();
  }
  return tenantId;
}

/**
 * 角色檢查。傳入允許的角色清單。
 */
export function requireRole(allowed: UserRole[]) {
  return async function (request: FastifyRequest, _reply: FastifyReply): Promise<void> {
    const role = request.user?.role;
    if (!role || !allowed.includes(role)) {
      throw new ForbiddenError(`Required roles: ${allowed.join(', ')}`);
    }
  };
}
