/**
 * 建立客戶表單
 *
 * - 用 React Query mutation：成功後 invalidate customers list
 * - 用 shared 的 Zod schema 在前端再驗一次（瀏覽器先擋掉，省一次後端 round trip）
 */
'use client';

import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { CreateCustomerSchema } from '@sme-crm/shared';
import { ApiError, customersApi } from '../lib/api-client';
import { qk } from '../lib/query-keys';

export function CreateCustomerForm() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [company, setCompany] = useState('');
  const [error, setError] = useState<string | null>(null);

  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: customersApi.create,
    onSuccess: () => {
      // 清欄位 + 觸發列表 refetch
      setName('');
      setEmail('');
      setCompany('');
      setError(null);
      queryClient.invalidateQueries({ queryKey: qk.customers.all });
    },
    onError: (err: unknown) => {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError('未知錯誤');
      }
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // 前端先驗（Zod schema 共用，瀏覽器擋掉省 round trip）
    const parsed = CreateCustomerSchema.safeParse({
      name,
      email: email || undefined,
      company: company || undefined,
    });

    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message ?? '輸入有誤');
      return;
    }

    mutation.mutate(parsed.data);
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-3 rounded-lg border border-gray-200 bg-white p-4"
    >
      <h3 className="text-sm font-semibold text-gray-900">新增客戶</h3>
      <div className="grid gap-3 sm:grid-cols-3">
        <input
          type="text"
          required
          placeholder="名字 *"
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        />
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        />
        <input
          type="text"
          placeholder="公司"
          value={company}
          onChange={(e) => setCompany(e.target.value)}
          className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        />
      </div>
      {error && <p className="text-sm text-red-600">{error}</p>}
      <button
        type="submit"
        disabled={mutation.isPending}
        className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:bg-gray-400"
      >
        {mutation.isPending ? '建立中...' : '建立客戶'}
      </button>
    </form>
  );
}
