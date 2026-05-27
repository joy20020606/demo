/**
 * Kanban 卡片 — 可拖拉
 *
 * 用 useDraggable 讓元件變成「拖曳源」。
 * - isDragging：拖曳中要做視覺反饋
 * - transform：dnd-kit 給的位移，要套進 style
 * - WON/LOST 是終態，禁止拖（後端會擋，前端也先擋掉省 round trip）
 */
'use client';

import { useDraggable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import type { Deal } from '@sme-crm/shared';

interface DealCardProps {
  deal: Deal;
  isTerminal: boolean;
}

export function DealCard({ deal, isTerminal }: DealCardProps) {
  const { attributes, listeners, setNodeRef, transform, isDragging } =
    useDraggable({
      id: deal.id,
      disabled: isTerminal, // 終態不能拖
    });

  return (
    <div
      ref={setNodeRef}
      style={{
        transform: CSS.Translate.toString(transform),
        opacity: isDragging ? 0.4 : 1,
      }}
      {...attributes}
      {...listeners}
      className={`rounded-md border bg-white p-3 shadow-sm transition ${
        isTerminal
          ? 'cursor-not-allowed border-gray-300 opacity-75'
          : 'cursor-grab border-gray-200 hover:border-blue-400 active:cursor-grabbing'
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <h4 className="text-sm font-semibold text-gray-900">{deal.title}</h4>
        <span className="shrink-0 text-xs font-medium text-gray-600">
          NT$ {deal.amount.toLocaleString('zh-TW')}
        </span>
      </div>
      {deal.expectedCloseAt && (
        <p className="mt-1 text-xs text-gray-500">
          預計成交：
          {new Date(deal.expectedCloseAt).toLocaleDateString('zh-TW')}
        </p>
      )}
      {isTerminal && (
        <p className="mt-1 text-xs italic text-gray-500">🔒 已結案</p>
      )}
    </div>
  );
}
