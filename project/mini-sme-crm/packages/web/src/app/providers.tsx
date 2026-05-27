/**
 * React Query Provider — 包在 layout 最外層
 *
 * 必須是 'use client'，因為 QueryClient 是 client-side stateful 物件。
 *
 * useState 讓 QueryClient 在 React 元件樹生命週期內只建立一次，
 * 而不是每次 re-render 都新建（避免 cache 被清掉）。
 */
'use client';

import { useState, type ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000, // 30 秒內視為新鮮，不重打
            refetchOnWindowFocus: false, // demo 用，少干擾
          },
        },
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}
