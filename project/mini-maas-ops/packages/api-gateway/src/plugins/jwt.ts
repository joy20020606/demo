/**
 * JWT plugin — 註冊 @fastify/jwt 並擴充 type definition。
 */
import fp from 'fastify-plugin';
import jwt from '@fastify/jwt';
import type { JwtPayload } from '@maas/shared';
import { config } from '../config.js';

declare module '@fastify/jwt' {
  interface FastifyJWT {
    payload: JwtPayload;
    user: JwtPayload;
  }
}

export default fp(async (fastify) => {
  await fastify.register(jwt, {
    secret: config.JWT_SECRET,
    sign: {
      expiresIn: config.JWT_EXPIRES_IN,
    },
  });

  fastify.log.info('✅ JWT plugin registered');
}, {
  name: 'jwt-plugin',
});
