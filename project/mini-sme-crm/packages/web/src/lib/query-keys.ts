/**
 * React Query key factory
 *
 * 集中管理 query key 結構，避免散落各檔造成 invalidate 對不上 key。
 *
 * 使用：
 *   useQuery({ queryKey: qk.customers.list({ search: 'foo' }), ... })
 *   queryClient.invalidateQueries({ queryKey: qk.customers.all })
 */
import type { DealStage } from '@sme-crm/shared';

export const qk = {
  customers: {
    all: ['customers'] as const,
    list: (params: { search?: string } = {}) =>
      [...qk.customers.all, 'list', params] as const,
    detail: (id: string) => [...qk.customers.all, 'detail', id] as const,
  },
  deals: {
    all: ['deals'] as const,
    list: (params: { stage?: DealStage; customerId?: string } = {}) =>
      [...qk.deals.all, 'list', params] as const,
    detail: (id: string) => [...qk.deals.all, 'detail', id] as const,
  },
};
