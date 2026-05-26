/**
 * Zod schemas for input validation.
 * 所有 API 進來的 input 都先過這裡，type-safe + runtime check。
 */
import { z } from 'zod';

// ----- Auth -----
export const loginSchema = z.object({
  email: z.string().email('Invalid email format'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});
export type LoginInput = z.infer<typeof loginSchema>;

// ----- Geo -----
export const geoPointSchema = z.object({
  lat: z.number().min(-90).max(90),
  lng: z.number().min(-180).max(180),
  address: z.string().min(1).max(500),
});
export type GeoPointInput = z.infer<typeof geoPointSchema>;

// ----- Routes -----
export const createRouteSchema = z.object({
  name: z.string().min(1).max(200),
  startLocation: geoPointSchema,
  endLocation: geoPointSchema,
  distanceKm: z.number().positive(),
});
export type CreateRouteInput = z.infer<typeof createRouteSchema>;

// ----- Common -----
export const idParamSchema = z.object({
  id: z.string().min(1),
});
export type IdParam = z.infer<typeof idParamSchema>;

export const paginationSchema = z.object({
  page: z.coerce.number().int().min(1).default(1),
  pageSize: z.coerce.number().int().min(1).max(100).default(20),
});
export type Pagination = z.infer<typeof paginationSchema>;

// ----- Commute Events -----
export const commuteEventTypeSchema = z.enum([
  'CHECK_IN',
  'CARPOOL_MATCH',
  'CONGESTION_REPORT',
  'OFF_PEAK_TRAVEL',
]);
export type CommuteEventTypeInput = z.infer<typeof commuteEventTypeSchema>;

// GET /api/events 的 query 參數
export const eventQuerySchema = z.object({
  routeId: z.string().optional(),
  eventType: commuteEventTypeSchema.optional(),
  page: z.coerce.number().int().min(1).default(1),
  pageSize: z.coerce.number().int().min(1).max(100).default(20),
});
export type EventQuery = z.infer<typeof eventQuerySchema>;
