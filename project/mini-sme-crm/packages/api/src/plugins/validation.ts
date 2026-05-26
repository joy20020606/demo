/**
 * 統一 Zod 驗證 helper
 *
 * 設計：
 * - 提供 `validateBody`、`validateParams`、`validateQuery` 三個 schema parser
 * - 失敗時 throw 400，由 Fastify error handler 統一回 JSON
 *
 * 為什麼不用 Fastify 內建 JSON Schema？
 * - Zod schema 已經在 @sme-crm/shared 定義，可以前後端共用
 * - Zod 的 .infer 直接給 TypeScript 型別，零重複
 */
import type { FastifyReply, FastifyRequest } from 'fastify';
import { ZodError, type ZodSchema } from 'zod';
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
 * 全域 error handler — 認得 ValidationError / AppError 就回對應 status
 */
export async function setupErrorHandler(app: import('fastify').FastifyInstance) {
  app.setErrorHandler(async (error, _request, reply: FastifyReply) => {
    if (error instanceof ValidationError) {
      return reply.code(400).send({
        error: 'ValidationError',
        message: error.message,
        details: error.details,
      });
    }

    // 從 @sme-crm/shared 來的 AppError（含子類）
    if ('statusCode' in error && typeof error.statusCode === 'number') {
      return reply.code(error.statusCode).send({
        error: error.name,
        message: error.message,
      });
    }

    // 未知錯誤一律 500，並 log
    app.log.error(error);
    return reply.code(500).send({
      error: 'InternalError',
      message: 'Something went wrong',
    });
  });
}
