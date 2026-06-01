/**
 * Kanban 卡片 — 可拖拉 + AI 評分 badge
 *
 * 結構：
 * - useDraggable 讓元件變成「拖曳源」
 * - WON/LOST 終態禁止拖
 * - 顯示 AI 分數（如果有）+ 「重新評分」按鈕
 *
 * 注意：「重新評分」按鈕的 onClick 要 stopPropagation，
 * 否則會跟 dnd-kit 的 listeners 衝突，導致按按鈕變成「開始拖」。
 */
'use client';

import { useDraggable } from '@dnd-kit/core';
import { CSS } from '@dnd-kit/utilities';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import type { Deal } from '@sme-crm/shared';
import { aiApi } from '../lib/api-client';
import { qk } from '../lib/query-keys';

interface DealCardProps {
  deal: Deal;
  isTerminal: boolean;
}

function ScoreBadge({ score }: { score: number }) {
  const colorClass =
    score >= 80
      ? 'bg-green-100 text-green-800'
      : score >= 50
        ? 'bg-yellow-100 text-yellow-800'
        : 'bg-red-100 text-red-800';
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-semibold ${colorClass}`}
    >
      🤖 {score}
    </span>
  );
}

export function DealCard({ deal, isTerminal }: DealCardProps) {
  const queryClient = useQueryClient();

  const { attributes, listeners, setNodeRef, transform, isDragging } =
    useDraggable({
      id: deal.id,
      disabled: isTerminal,
    });

  const scoreMutation = useMutation({
    mutationFn: () => aiApi.scoreDeal(deal.id),
    onSuccess: () => {
      // 刷新整個 Kanban 拿到最新 aiScore
      queryClient.invalidateQueries({ queryKey: qk.deals.all });
    },
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

      {/* AI 評分區 */}
      <div className="mt-2 space-y-1.5 border-t border-gray-100 pt-2">
        {/* 第一行：badge + 按鈕 */}
        <div className="flex items-center justify-between gap-2">
          {typeof deal.aiScore === 'number' ? (
            <ScoreBadge score={deal.aiScore} />
          ) : (
            <span className="text-xs text-gray-400">未評分</span>
          )}

          <button
            type="button"
            // 阻止 dnd-kit 把 click 當拖曳
            onPointerDown={(e) => e.stopPropagation()}
            onClick={(e) => {
              e.stopPropagation();
              scoreMutation.mutate();
            }}
            disabled={scoreMutation.isPending}
            title={
              typeof deal.aiScore === 'number'
                ? '重新呼叫 Claude 評估（依目前最新狀態）'
                : '呼叫 Claude 進行第一次評分'
            }
            className="shrink-0 rounded-md bg-purple-50 px-2 py-1 text-xs font-medium text-purple-700 transition hover:bg-purple-100 disabled:opacity-50"
          >
            {scoreMutation.isPending
              ? '評分中...'
              : typeof deal.aiScore === 'number'
                ? '🤖 重評'
                : '🤖 評分'}
          </button>
        </div>

        {/* 第二行：理由（最多 2 行，超過顯示 ...） */}
        {deal.aiScoreReason && (
          <p
            className="line-clamp-2 text-xs leading-snug text-gray-600"
            title={deal.aiScoreReason}
          >
            {deal.aiScoreReason}
          </p>
        )}
      </div>
    </div>
  );
}
