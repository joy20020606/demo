/**
 * 客戶聯絡紀錄列表 + 新增表單
 *
 * Phase 5：給 AI 摘要當資料來源
 */
'use client';

import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ContactChannelSchema,
  CreateContactLogSchema,
  type ContactChannel,
} from '@sme-crm/shared';
import { ApiError, contactLogsApi } from '../lib/api-client';
import { qk } from '../lib/query-keys';

const CHANNEL_LABELS: Record<ContactChannel, string> = {
  EMAIL: '📧 Email',
  PHONE: '📞 電話',
  LINE: '💬 LINE',
  MEETING: '🤝 面談',
  OTHER: '🗒️ 其他',
};

interface Props {
  customerId: string;
}

export function ContactLogList({ customerId }: Props) {
  const [channel, setChannel] = useState<ContactChannel>('PHONE');
  const [summary, setSummary] = useState('');
  const [occurredAt, setOccurredAt] = useState(
    new Date().toISOString().slice(0, 16),
  );
  const [error, setError] = useState<string | null>(null);

  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: qk.contactLogs.byCustomer(customerId),
    queryFn: () => contactLogsApi.listByCustomer(customerId),
  });

  const createMutation = useMutation({
    mutationFn: contactLogsApi.create,
    onSuccess: () => {
      setSummary('');
      setOccurredAt(new Date().toISOString().slice(0, 16));
      setError(null);
      queryClient.invalidateQueries({
        queryKey: qk.contactLogs.byCustomer(customerId),
      });
    },
    onError: (err: unknown) => {
      setError(err instanceof ApiError ? err.message : '未知錯誤');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: contactLogsApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: qk.contactLogs.byCustomer(customerId),
      });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const parsed = CreateContactLogSchema.safeParse({
      customerId,
      channel,
      summary,
      occurredAt: new Date(occurredAt),
    });

    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message ?? '輸入有誤');
      return;
    }

    createMutation.mutate(parsed.data);
  };

  return (
    <div className="space-y-4">
      {/* 新增表單 */}
      <form
        onSubmit={handleSubmit}
        className="space-y-3 rounded-lg border border-gray-200 bg-white p-4"
      >
        <h3 className="text-sm font-semibold text-gray-900">新增聯絡紀錄</h3>
        <div className="grid gap-3 sm:grid-cols-2">
          <select
            value={channel}
            onChange={(e) => setChannel(e.target.value as ContactChannel)}
            className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
          >
            {ContactChannelSchema.options.map((c) => (
              <option key={c} value={c}>
                {CHANNEL_LABELS[c]}
              </option>
            ))}
          </select>
          <input
            type="datetime-local"
            value={occurredAt}
            onChange={(e) => setOccurredAt(e.target.value)}
            className="rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
          />
        </div>
        <textarea
          required
          placeholder="這次互動內容（例如：客戶詢問月底前可否報價、希望週四前回覆）"
          value={summary}
          onChange={(e) => setSummary(e.target.value)}
          rows={3}
          className="w-full resize-y rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none"
        />
        {error && <p className="text-sm text-red-600">{error}</p>}
        <button
          type="submit"
          disabled={createMutation.isPending}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700 disabled:bg-gray-400"
        >
          {createMutation.isPending ? '建立中...' : '新增聯絡紀錄'}
        </button>
      </form>

      {/* 列表 */}
      <div className="space-y-2">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-semibold text-gray-900">
            聯絡紀錄
            {data && (
              <span className="ml-2 text-xs font-normal text-gray-500">
                共 {data.length} 筆
              </span>
            )}
          </h3>
        </div>

        {isLoading && (
          <p className="rounded-md border border-gray-200 bg-white p-4 text-sm text-gray-500">
            載入中...
          </p>
        )}

        {data?.length === 0 && (
          <p className="rounded-md border border-dashed border-gray-300 bg-gray-50 p-6 text-center text-sm text-gray-500">
            尚無聯絡紀錄。建立後即可產生 AI 摘要。
          </p>
        )}

        <ul className="space-y-2">
          {data?.map((log) => (
            <li
              key={log.id}
              className="group rounded-md border border-gray-200 bg-white p-3"
            >
              <div className="flex items-start justify-between gap-2">
                <div className="flex-1">
                  <div className="flex items-center gap-2 text-xs text-gray-500">
                    <span className="font-medium text-gray-700">
                      {CHANNEL_LABELS[log.channel as ContactChannel]}
                    </span>
                    <span>·</span>
                    <span>
                      {new Date(log.occurredAt).toLocaleString('zh-TW', {
                        month: 'numeric',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </span>
                  </div>
                  <p className="mt-1 whitespace-pre-wrap text-sm text-gray-800">
                    {log.summary}
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => {
                    if (confirm('確定刪除這筆紀錄？')) {
                      deleteMutation.mutate(log.id);
                    }
                  }}
                  className="invisible text-xs text-red-600 hover:underline group-hover:visible"
                >
                  刪除
                </button>
              </div>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
