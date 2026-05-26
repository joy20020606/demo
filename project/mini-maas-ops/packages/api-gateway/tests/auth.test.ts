/**
 * Unit tests for JWT middleware helpers.
 *
 * 不打 DB、不開 Fastify server。純邏輯測試，跑得超快。
 */
import { describe, it, expect } from 'vitest';
import { requireTenant, requireRole } from '../src/middleware/auth.js';
import { TenantContextError, ForbiddenError } from '@maas/shared';
import type { FastifyRequest, FastifyReply } from 'fastify';

// 建一個假 request，只填我們關心的欄位
function mockRequest(user?: {
  userId: string;
  tenantId: string;
  role: 'ADMIN' | 'OPERATOR' | 'VIEWER';
  email: string;
}): FastifyRequest {
  return { user } as unknown as FastifyRequest;
}

const mockReply = {} as FastifyReply;

describe('requireTenant', () => {
  it('returns tenantId when JWT user has tenantId', () => {
    const req = mockRequest({
      userId: 'u1',
      tenantId: 't1',
      role: 'ADMIN',
      email: 'a@b.c',
    });
    expect(requireTenant(req)).toBe('t1');
  });

  it('throws TenantContextError when user is missing', () => {
    const req = mockRequest();
    expect(() => requireTenant(req)).toThrow(TenantContextError);
  });

  it('throws TenantContextError when tenantId is empty', () => {
    const req = mockRequest({
      userId: 'u1',
      tenantId: '',
      role: 'ADMIN',
      email: 'a@b.c',
    });
    expect(() => requireTenant(req)).toThrow(TenantContextError);
  });
});

describe('requireRole', () => {
  it('passes when role is allowed', async () => {
    const check = requireRole(['ADMIN', 'OPERATOR']);
    const req = mockRequest({
      userId: 'u1',
      tenantId: 't1',
      role: 'ADMIN',
      email: 'a@b.c',
    });
    await expect(check(req, mockReply)).resolves.toBeUndefined();
  });

  it('throws ForbiddenError when role is not allowed', async () => {
    const check = requireRole(['ADMIN']);
    const req = mockRequest({
      userId: 'u1',
      tenantId: 't1',
      role: 'VIEWER',
      email: 'a@b.c',
    });
    await expect(check(req, mockReply)).rejects.toThrow(ForbiddenError);
  });

  it('throws ForbiddenError when user is missing', async () => {
    const check = requireRole(['ADMIN']);
    const req = mockRequest();
    await expect(check(req, mockReply)).rejects.toThrow(ForbiddenError);
  });
});
