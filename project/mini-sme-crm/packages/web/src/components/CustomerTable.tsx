/**
 * 客戶列表表格
 *
 * 用 React Query useQuery 抓資料：自動 cache、loading/error 狀態
 */
'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';
import { customersApi } from '../lib/api-client';
import { qk } from '../lib/query-keys';

export function CustomerTable() {
  const [search, setSearch] = useState('');

  const { data, isLoading, error } = useQuery({
    queryKey: qk.customers.list({ search }),
    queryFn: () => customersApi.list({ search: search || undefined }),
  });

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between">
        <input
          type="text"
          placeholder="搜尋名字 / 公司 / email..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-64 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        />
        {data && (
          <span className="text-sm text-gray-500">共 {data.total} 筆</span>
        )}
      </div>

      <div className="overflow-hidden rounded-lg border border-gray-200 bg-white">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-2 text-left font-semibold text-gray-700">
                名字
              </th>
              <th className="px-4 py-2 text-left font-semibold text-gray-700">
                Email
              </th>
              <th className="px-4 py-2 text-left font-semibold text-gray-700">
                公司
              </th>
              <th className="px-4 py-2 text-left font-semibold text-gray-700">
                建立時間
              </th>
              <th className="px-4 py-2 text-right font-semibold text-gray-700"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {isLoading && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-500">
                  載入中...
                </td>
              </tr>
            )}
            {error && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-red-600">
                  載入失敗：{(error as Error).message}
                </td>
              </tr>
            )}
            {data?.items.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-8 text-center text-gray-500">
                  沒有客戶資料
                </td>
              </tr>
            )}
            {data?.items.map((c) => (
              <tr key={c.id} className="hover:bg-gray-50">
                <td className="px-4 py-2 font-medium">
                  <Link
                    href={`/customers/${c.id}`}
                    className="text-blue-600 hover:underline"
                  >
                    {c.name}
                  </Link>
                </td>
                <td className="px-4 py-2 text-gray-700">{c.email ?? '—'}</td>
                <td className="px-4 py-2 text-gray-700">{c.company ?? '—'}</td>
                <td className="px-4 py-2 text-gray-500">
                  {new Date(c.createdAt).toLocaleDateString('zh-TW')}
                </td>
                <td className="px-4 py-2 text-right">
                  <Link
                    href={`/customers/${c.id}`}
                    className="text-xs text-gray-500 hover:text-blue-600"
                  >
                    詳細 →
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
