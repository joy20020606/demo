/**
 * Mongoose 模型 — CommuteEvent。
 *
 * 放在 shared package，讓 ingestion（負責寫入）和 api-gateway（負責讀取）
 * 共用同一份 schema 定義，避免兩邊各寫一份、日後不同步。
 *
 * 注意：ingestion 和 api-gateway 是兩個獨立的 Node process，
 *      各自有自己的 mongoose 實例，所以兩邊都註冊這個 model 不會衝突。
 */
import mongoose, { Schema, type Model } from 'mongoose';
import type { CommuteEventType } from '../types.js';

export interface CommuteEventDoc {
  tenantId: string;
  routeId: string;
  eventType: CommuteEventType;
  occurredAt: Date;
  payload: Record<string, unknown>;
  createdAt: Date;
  updatedAt: Date;
}

const commuteEventSchema = new Schema<CommuteEventDoc>(
  {
    tenantId: { type: String, required: true, index: true },
    routeId: { type: String, required: true, index: true },
    eventType: {
      type: String,
      required: true,
      enum: ['CHECK_IN', 'CARPOOL_MATCH', 'CONGESTION_REPORT', 'OFF_PEAK_TRAVEL'],
    },
    occurredAt: { type: Date, required: true },
    // Mixed 型別 = 任意結構。這是 MongoDB 彈性 schema 的關鍵。
    payload: { type: Schema.Types.Mixed, default: {} },
  },
  {
    timestamps: true,             // 自動維護 createdAt / updatedAt
    collection: 'commute_events',
  },
);

// 複合索引：對應最常見的查詢「某租戶的事件、依發生時間新到舊」
commuteEventSchema.index({ tenantId: 1, occurredAt: -1 });

/**
 * 用 mongoose.models 做防呆 —— 同一個 process 內重複 import 此檔時，
 * 不會拋出 "Cannot overwrite model once compiled" 錯誤。
 */
export const CommuteEventModel: Model<CommuteEventDoc> =
  (mongoose.models.CommuteEvent as Model<CommuteEventDoc> | undefined) ??
  mongoose.model<CommuteEventDoc>('CommuteEvent', commuteEventSchema);
