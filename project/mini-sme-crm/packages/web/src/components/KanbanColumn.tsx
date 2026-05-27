/**
 * Kanban 欄位 — 是「放置目標」（droppable）
 *
 * 用 useDroppable 接收拖過來的卡片。
 * - isOver：滑鼠拖著卡片在這欄上方時，給視覺反饋
 */
'use client';

import { useDroppable } from '@dnd-kit/core';
import type { Deal, DealStage } from '@sme-crm/shared';
import { DealCard } from './DealCard';

const STAGE_LABELS: Record<DealStage, string> = {
  LEAD: '名單',
  CONTACTED: '已聯絡',
  PROPOSAL: '已報價',
  NEGOTIATION: '議價中',
  WON: '成交 🎉',
  LOST: '失敗',
};

const STAGE_COLORS: Record<DealStage, string> = {
  LEAD: 'border-gray-300 bg-gray-50',
  CONTACTED: 'border-blue-300 bg-blue-50',
  PROPOSAL: 'border-indigo-300 bg-indigo-50',
  NEGOTIATION: 'border-orange-300 bg-orange-50',
  WON: 'border-green-400 bg-green-50',
  LOST: 'border-red-300 bg-red-50',
};

const TERMINAL_STAGES: DealStage[] = ['WON', 'LOST'];

interface KanbanColumnProps {
  stage: DealStage;
  deals: Deal[];
}

export function KanbanColumn({ stage, deals }: KanbanColumnProps) {
  const isTerminal = TERMINAL_STAGES.includes(stage);

  const { setNodeRef, isOver } = useDroppable({
    id: stage,
  });

  return (
    <div
      ref={setNodeRef}
      className={`flex min-h-[400px] w-full flex-col rounded-lg border-2 p-3 transition ${
        STAGE_COLORS[stage]
      } ${isOver ? 'ring-2 ring-blue-400' : ''}`}
    >
      <header className="mb-3 flex items-center justify-between">
        <h3 className="text-sm font-bold text-gray-900">{STAGE_LABELS[stage]}</h3>
        <span className="rounded-full bg-white px-2 py-0.5 text-xs font-medium text-gray-700">
          {deals.length}
        </span>
      </header>
      <div className="flex flex-col gap-2">
        {deals.map((deal) => (
          <DealCard key={deal.id} deal={deal} isTerminal={isTerminal} />
        ))}
        {deals.length === 0 && (
          <p className="rounded-md border-2 border-dashed border-gray-300 p-4 text-center text-xs text-gray-400">
            （拖卡片過來）
          </p>
        )}
      </div>
    </div>
  );
}
