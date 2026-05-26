/**
 * 全域 error handler + Zod validation helpers
 *
 * Error handler 認得：
 * 1. Zod schema validation 失敗（fastify-type-provider-zod 拋的）→ 400
 * 2. 我們自家 AppError（NotFoundError、ValidationError 等）→ 對應 statusCode
 * 3. 其他未知錯誤 → 500（並 log）
 *
 * parseBody / parseParams / parseQuery 三個 helper 是給「不走 route schema 自動驗證」
 * 的場景用的（例如 service 內要驗來自 queue 的資料）。Route 層現在直接靠
 * fastify-type-provider-zod 自動驗證，不需要呼叫這些 helper。
 */
import type { FastifyReply, FastifyRequest } from 'fastify';
import { ZodError, type ZodSchema } from 'zod';
import { hasZodFastifySchemaValidationErrors } from 'fastify-type-provider-zod';
import { ValidationError } from '@sme-crm/shared';

/**
 * 把 zod parse 的失敗轉成 ValidationError
 */
function parseOrThrow<T>(schema: ZodSchema<T>, data: unknown, source: string): T {
  try {
    return schema.parse(data);
  } catch (err) {
    if (err instanceof ZodError) {
      throw new ValidationError(`Invalid ${source}`, err.issues);
    }
    throw err;
  }
}

export function parseBody<T>(req: FastifyRequest, schema: ZodSchema<T>): T {
  return parseOrThrow(schema, req.body, 'body');
}

export function parseParams<T>(req: FastifyRequest, schema: ZodSchema<T>): T {
  return parseOrThrow(schema, req.params, 'params');
}

export function parseQuery<T>(req: FastifyRequest, schema: ZodSchema<T>): T {
  return parseOrThrow(schema, req.query, 'query');
}

/**
 * 全域 error handler
 */
export async function setupErrorHandler(app: import('fastify').FastifyInstance) {
  app.setErrorHandler(async (error, _request, reply: FastifyReply) => {
    // 1. fastify-type-provider-zod：route schema 自動驗證失敗
    if (hasZodFastifySchemaValidationErrors(error)) {
      return reply.code(400).send({
        error: 'ValidationError',
        message: 'Request validation failed',
        details: error.validation,
      });
    }

    // 2. 我們手動 throw 的 ValidationError（例如 parseBody 拋的）
    if (error instanceof ValidationError) {
      return reply.code(400).send({
        error: 'ValidationError',
        message: error.message,
        details: error.details,
      });
    }

    // 3. 從 @sme-crm/shared 來的 AppError（含 NotFoundError 等子類）
    if ('statusCode' in error && typeof error.statusCode === 'number') {
      return reply.code(error.statusCode).send({
        error: error.name,
        message: error.message,
      });
    }

    // 4. 未知錯誤一律 500，並 log
    app.log.error(error);
    return reply.code(500).send({
      error: 'InternalError',
      message: 'Something went wrong',
    });
  });
}
