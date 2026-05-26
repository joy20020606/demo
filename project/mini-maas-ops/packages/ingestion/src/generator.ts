/**
 * Mock 通勤事件產生器。
 *
 * 重點：不同 eventType 產生不同結構的 payload ——
 * 這正是「為什麼用 MongoDB 存事件」的具體展現（彈性 schema）。
 */
import type { CommuteEventType } from '@maas/shared';

const EVENT_TYPES: CommuteEventType[] = [
  'CHECK_IN',
  'CARPOOL_MATCH',
  'CONGESTION_REPORT',
  'OFF_PEAK_TRAVEL',
];

function pick<T>(arr: T[]): T {
  // 呼叫端保證 arr 非空
  return arr[Math.floor(Math.random() * arr.length)] as T;
}

function randInt(min: number, max: number): number {
  return min + Math.floor(Math.random() * (max - min + 1));
}

/**
 * 依事件類型產生對應結構的 payload。
 */
function buildPayload(eventType: CommuteEventType): Record<string, unknown> {
  switch (eventType) {
    case 'CHECK_IN':
      return { commuterCount: randInt(1, 5) };
    case 'CARPOOL_MATCH':
      return { driverId: `driver-${randInt(1, 99)}`, riderCount: randInt(1, 3) };
    case 'CONGESTION_REPORT':
      return {
        congestionIndex: Number(Math.random().toFixed(2)),
        avgSpeedKmh: randInt(8, 60),
      };
    case 'OFF_PEAK_TRAVEL':
      return { rewardPoints: 10 * randInt(1, 5) };
    default:
      return {};
  }
}

export interface GeneratedEvent {
  tenantId: string;
  routeId: string;
  eventType: CommuteEventType;
  occurredAt: Date;
  payload: Record<string, unknown>;
}

export function generateEvent(tenantId: string, routeIds: string[]): GeneratedEvent {
  const eventType = pick(EVENT_TYPES);
  return {
    tenantId,
    routeId: pick(routeIds),
    eventType,
    occurredAt: new Date(),
    payload: buildPayload(eventType),
  };
}
