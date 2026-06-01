/**
 * AI 摘要面板
 *
 * 設計：
 * - 按鈕觸發（不自動 call，避免重複燒 token）
 * - 結果只存在 React state，refresh 就消失（每次想看就重新生）
 * - 中途 loading 期間鎖按鈕
 */
'use client';

import { useMutation } from '@tanstack/react-query';
import { ApiError, aiApi } from '../lib/api-client';

interface Props {
  customerId: string;
}

export function AiSummaryPanel({ customerId }: Props) {
  const mutation = useMutation({
    mutationFn: () => aiApi.summarizeCustomer(customerId),
  });

  return (
    <div className="space-y-3 rounded-lg border border-purple-200 bg-gradient-to-br from-purple-50 to-indigo-50 p-4">
      <div className="flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <span className="text-lg">🤖</span>
          <h3 className="text-sm font-semibold text-gray-900">AI 客戶現況摘要</h3>
        </div>
        <button
          type="button"
          onClick={() => mutation.mutate()}
          disabled={mutation.isPending}
          className="rounded-md bg-purple-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-purple-700 disabled:bg-gray-400"
        >
          {mutation.isPending ? '分析中...' : mutation.data ? '重新分析' : '產生摘要'}
        </button>
      </div>

      {!mutation.data && !mutation.isPending && !mutation.error && (
        <p className="text-xs text-gray-600">
          按右上角按鈕讓 Claude 把所有聯絡紀錄摘要成「客戶現況 + 下一步建議」
        </p>
      )}

      {mutation.isPending && (
        <div className="flex items-center gap-2 text-sm text-gray-600">
          <span className="inline-block h-2 w-2 animate-pulse rounded-full bg-purple-500" />
          Claude 分析中（5-10 秒）...
        </div>
      )}

      {mutation.error && (
        <p className="rounded-md bg-red-50 p-3 text-sm text-red-700">
          {mutation.error instanceof ApiError
            ? mutation.error.message
            : '產生失敗，請稍後再試'}
        </p>
      )}

      {mutation.data && (
        <div className="space-y-3">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">
              客戶現況
            </p>
            <p className="mt-1 text-sm text-gray-800">
              {mutation.data.currentStatus}
            </p>
          </div>

          {mutation.data.nextActions.length > 0 && (
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-gray-500">
                下一步建議
              </p>
              <ul className="mt-1 space-y-1">
                {mutation.data.nextActions.map((action, idx) => (
                  <li key={idx} className="flex gap-2 text-sm text-gray-800">
                    <span className="text-purple-600">▸</span>
                    {action}
                  </li>
                ))}
              </ul>
            </div>
          )}

          <p className="border-t border-purple-200 pt-2 text-xs text-gray-500">
            分析依據：{mutation.data.contactCount} 筆聯絡紀錄
          </p>
        </div>
      )}
    </div>
  );
}
