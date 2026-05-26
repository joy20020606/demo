/**
 * Common domain types shared between packages.
 * 跨 package 共用的領域型別。
 */

// ----- JWT Payload -----
// 注意：tenantId 是多租戶資料隔離的關鍵欄位
export interface JwtPayload {
  userId: string;
  tenantId: string;
  role: UserRole;
  email: string;
}

// ----- Enums -----
export type UserRole = 'ADMIN' | 'OPERATOR' | 'VIEWER';

export type IncentiveProgramStatus =
  | 'DRAFT'      // 草稿
  | 'PENDING'    // 等待 AI / 人工審核 (Human-in-the-loop)
  | 'APPROVED'   // 已核准
  | 'ACTIVE'     // 進行中
  | 'COMPLETED'  // 已結束
  | 'REJECTED';  // 已拒絕

// ----- Domain Entities (對應 Prisma model) -----
export interface Tenant {
  id: string;
  name: string;
  createdAt: Date;
}

export interface User {
  id: string;
  email: string;
  role: UserRole;
  tenantId: string;
}

export interface GeoPoint {
  lat: number;
  lng: number;
  address: string;
}

export interface Route {
  id: string;
  name: string;
  startLocation: GeoPoint;
  endLocation: GeoPoint;
  distanceKm: number;
  tenantId: string;
  createdAt: Date;
}

export interface IncentiveProgram {
  id: string;
  name: string;
  description: string;
  budget: number;
  status: IncentiveProgramStatus;
  tenantId: string;
  createdAt: Date;
}

// ----- Commute Events (存在 MongoDB) -----
// 通勤事件。每種事件的 payload 結構不同 —— 正是 MongoDB 彈性 schema 的用武之地。
export type CommuteEventType =
  | 'CHECK_IN'           // 上車打卡
  | 'CARPOOL_MATCH'      // 共乘配對成功
  | 'CONGESTION_REPORT'  // 壅塞回報
  | 'OFF_PEAK_TRAVEL';   // 錯峰出行

export interface CommuteEvent {
  tenantId: string;
  routeId: string;
  eventType: CommuteEventType;
  occurredAt: Date;
  payload: Record<string, unknown>;
  createdAt: Date;
  updatedAt: Date;
}

// ----- API Response Envelopes -----
// 統一的 API 回應格式
export interface ApiSuccess<T> {
  data: T;
}

export interface ApiError {
  error: {
    code: string;
    message: string;
    details?: unknown;
  };
}
