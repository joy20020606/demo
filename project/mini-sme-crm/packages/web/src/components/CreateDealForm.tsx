/**
 * 建立商機表單
 *
 * - customerId 用下拉選單從現有客戶選
 * - 建立後 invalidate deals list，Kanban 自動更新
 */
'use client';

import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { CreateDealSchema } from '@sme-crm/shared';
import { ApiError, customersApi, dealsApi } from '../lib/api-client';
import { qk } from '../lib/query-keys';

export function CreateDealForm() {
  const [customerId, setCustomerId] = useState('');
  const [title, setTitle] = useState('');
  const [amount, setAmount] = useState('');
  const [error, setError] = useState<string | null>(null);

  const queryClient = useQueryClient();

  // 客戶下拉選單來源
  const customersQuery = useQuery({
    queryKey: qk.customers.list(),
    queryFn: () => customersApi.list({ limit: 200 }),
  });

  const mutation = useMutation({
    mutationFn: dealsApi.create,
    onSuccess: () => {
      setTitle('');
      setAmount('');
      setError(null);
      queryClient.invalidateQueries({ queryKey: qk.deals.all });
    },
    onError: (err: unknown) => {
      setError(err instanceof ApiError ? err.message : '未知錯誤');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const parsed = CreateDealSchema.safeParse({
      customerId,
      title,
      amount: Number(amount),
      stage: 'LEAD' as const,
    });

    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message ?? '輸入有誤');
      return;
    }

    mutation.mutate(parsed.data);
  };

  const hasCustomers = (customersQuery.data?.items.length ?? 0) > 0;

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-3 rounded-lg border border-gray-200 bg-white p-4"
    >
      <h3 className="text-sm font-semibold text-gray-900">新增商機</h3>

      {!hasCustomers && (
        <p className="text-sm text-amber-700">
          ⚠️ 還沒有客戶資料，請先到「客戶」頁建立。
        </p>
      )}

      <div className="grid gap-3 sm:grid-cols-3">
        <select
          required
          value={customerId}
          onChange={(e) => setCustomerId(e.target.value)}
          disabled={!hasCustomers}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none disabled:bg-gray-100"
        >
          <option value="">選擇客戶 *</option>
          {customersQuery.data?.items.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
              {c.company ? ` (${c.company})` : ''}
            </option>
          ))}
        </select>
        <input
          type="text"
          required
          placeholder="商機標題 *"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        />
        <input
          type="number"
          required
          min="0"
          step="1"
          placeholder="金額（NTD） *"
          value={amount}
          onChange={(e) => setAmount(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        />
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <button
        type="submit"
        disabled={mutation.isPending || !hasCustomers}
        className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:bg-gray-400"
      >
        {mutation.isPending ? '建立中...' : '建立商機（預設 LEAD）'}
      </button>
    </form>
  );
}
