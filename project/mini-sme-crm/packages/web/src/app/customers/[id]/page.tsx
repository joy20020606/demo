'use client';

import { useQuery } from '@tanstack/react-query';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { customersApi } from '../../../lib/api-client';
import { qk } from '../../../lib/query-keys';
import { ContactLogList } from '../../../components/ContactLogList';
import { AiSummaryPanel } from '../../../components/AiSummaryPanel';

export default function CustomerDetailPage() {
  const params = useParams<{ id: string }>();
  const customerId = params.id;

  const { data: customer, isLoading, error } = useQuery({
    queryKey: qk.customers.detail(customerId),
    queryFn: () => customersApi.getById(customerId),
    enabled: !!customerId,
  });

  if (isLoading) {
    return <p className="text-gray-500">載入客戶資料...</p>;
  }
  if (error || !customer) {
    return (
      <div className="space-y-3">
        <p className="text-red-600">
          載入失敗：{(error as Error)?.message ?? '客戶不存在'}
        </p>
        <Link href="/customers" className="text-sm text-blue-600 hover:underline">
          ← 回客戶列表
        </Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <Link
          href="/customers"
          className="text-sm text-gray-500 hover:text-gray-700"
        >
          ← 回客戶列表
        </Link>
      </div>

      {/* 客戶基本資料 */}
      <section className="rounded-lg border border-gray-200 bg-white p-5">
        <h1 className="text-2xl font-bold text-gray-900">{customer.name}</h1>
        <div className="mt-3 grid gap-2 text-sm sm:grid-cols-3">
          <div>
            <span className="text-gray-500">Email：</span>
            <span className="text-gray-900">{customer.email ?? '—'}</span>
          </div>
          <div>
            <span className="text-gray-500">電話：</span>
            <span className="text-gray-900">{customer.phone ?? '—'}</span>
          </div>
          <div>
            <span className="text-gray-500">公司：</span>
            <span className="text-gray-900">{customer.company ?? '—'}</span>
          </div>
        </div>
      </section>

      {/* AI 摘要 */}
      <AiSummaryPanel customerId={customerId} />

      {/* 聯絡紀錄 */}
      <ContactLogList customerId={customerId} />
    </div>
  );
}
