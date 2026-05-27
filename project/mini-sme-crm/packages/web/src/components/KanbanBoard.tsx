/**
 * Kanban 看板主體 — 拖拉商機到不同 stage
 *
 * 重點：
 * 1. 一次撈全部 deals（不分 stage），前端按 stage 分組
 * 2. DndContext 包整個看板
 * 3. onDragEnd 觸發 transitionStage mutation
 * 4. **Optimistic update**：拖完 UI 立刻變，背景才打 API
 *    若 API 失敗（例如 409 Conflict）→ rollback 回原狀
 */
'use client';

import { useMemo } from 'react';
import {
  DndContext,
  PointerSensor,
  KeyboardSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import type { Deal, DealStage } from '@sme-crm/shared';
import { ApiError, dealsApi } from '../lib/api-client';
import { qk } from '../lib/query-keys';
import { KanbanColumn } from './KanbanColumn';

const STAGES: DealStage[] = [
  'LEAD',
  'CONTACTED',
  'PROPOSAL',
  'NEGOTIATION',
  'WON',
  'LOST',
];

interface ListResponse {
  items: Deal[];
  total: number;
  limit: number;
  offset: number;
}

export function KanbanBoard() {
  const queryClient = useQueryClient();

  // 撈全部 deals
  const { data, isLoading, error } = useQuery({
    queryKey: qk.deals.list(),
    queryFn: () => dealsApi.list({ limit: 200 }),
  });

  // 按 stage 分組
  const dealsByStage = useMemo(() => {
    const map: Record<DealStage, Deal[]> = {
      LEAD: [],
      CONTACTED: [],
      PROPOSAL: [],
      NEGOTIATION: [],
      WON: [],
      LOST: [],
    };
    data?.items.forEach((d) => map[d.stage].push(d));
    return map;
  }, [data]);

  // 拖拉支援：滑鼠 + 鍵盤（a11y）
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor),
  );

  const mutation = useMutation({
    mutationFn: ({ id, stage }: { id: string; stage: DealStage }) =>
      dealsApi.transitionStage(id, stage),

    // Optimistic update：UI 立刻反應
    onMutate: async ({ id, stage }) => {
      await queryClient.cancelQueries({ queryKey: qk.deals.all });
      const previous = queryClient.getQueryData<ListResponse>(qk.deals.list());

      if (previous) {
        queryClient.setQueryData<ListResponse>(qk.deals.list(), {
          ...previous,
          items: previous.items.map((d) =>
            d.id === id ? { ...d, stage } : d,
          ),
        });
      }
      return { previous };
    },

    // API 失敗 → rollback
    onError: (err, _vars, context) => {
      if (context?.previous) {
        queryClient.setQueryData(qk.deals.list(), context.previous);
      }
      if (err instanceof ApiError) {
        alert(`轉移失敗 (${err.statusCode}): ${err.message}`);
      } else {
        alert('未知錯誤');
      }
    },

    // 成功也 refetch，確保跟後端同步
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: qk.deals.all });
    },
  });

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (!over) return;

    const dealId = String(active.id);
    const nextStage = over.id as DealStage;

    // 找到原 deal，確認真的有變
    const deal = data?.items.find((d) => d.id === dealId);
    if (!deal || deal.stage === nextStage) return;

    mutation.mutate({ id: dealId, stage: nextStage });
  };

  if (isLoading) {
    return <p className="text-gray-500">載入商機中...</p>;
  }
  if (error) {
    return (
      <p className="text-red-600">載入失敗：{(error as Error).message}</p>
    );
  }

  return (
    <DndContext sensors={sensors} onDragEnd={handleDragEnd}>
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-3 xl:grid-cols-6">
        {STAGES.map((stage) => (
          <KanbanColumn key={stage} stage={stage} deals={dealsByStage[stage]} />
        ))}
      </div>
    </DndContext>
  );
}
