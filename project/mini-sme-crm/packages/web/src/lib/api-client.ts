/**
 * 型別化 API client
 *
 * 設計：
 * - 把 fetch 包成一層 helper，集中處理 base URL / JSON / 錯誤
 * - reuse @sme-crm/shared 裡的 Zod schema，request/response 型別自動推導
 * - 拋自家 ApiError，UI 層可以根據 statusCode 分流（例如 409 顯示「不能轉」）
 */
import type {
  Customer,
  CreateCustomerInput,
  Deal,
  CreateDealInput,
  DealStage,
} from '@sme-crm/shared';

const BASE_URL =
  process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:3001';

/**
 * 後端統一錯誤格式：{ error, message, details? }
 */
export class ApiError extends Error {
  constructor(
    message: string,
    public readonly statusCode: number,
    public readonly code: string,
    public readonly details?: unknown,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

interface ListResponse<T> {
  items: T[];
  total: number;
  limit: number;
  offset: number;
}

/**
 * 共用 fetch wrapper：自動處理 JSON、錯誤轉成 ApiError
 */
async function request<T>(
  path: string,
  init: RequestInit & { json?: unknown } = {},
): Promise<T> {
  const { json, headers, ...rest } = init;

  const res = await fetch(`${BASE_URL}${path}`, {
    ...rest,
    headers: {
      'Content-Type': 'application/json',
      ...headers,
    },
    body: json !== undefined ? JSON.stringify(json) : init.body,
  });

  // 204 No Content（DELETE）
  if (res.status === 204) {
    return undefined as T;
  }

  const data = await res.json().catch(() => ({}));

  if (!res.ok) {
    throw new ApiError(
      (data as { message?: string }).message ?? res.statusText,
      res.status,
      (data as { error?: string }).error ?? 'UnknownError',
      (data as { details?: unknown }).details,
    );
  }

  return data as T;
}

// ============================================================
// Customers
// ============================================================
export const customersApi = {
  list: (params: { search?: string; limit?: number; offset?: number } = {}) => {
    const qs = new URLSearchParams();
    if (params.search) qs.set('search', params.search);
    if (params.limit !== undefined) qs.set('limit', String(params.limit));
    if (params.offset !== undefined) qs.set('offset', String(params.offset));
    const suffix = qs.toString() ? `?${qs.toString()}` : '';
    return request<ListResponse<Customer>>(`/api/customers${suffix}`);
  },

  getById: (id: string) => request<Customer>(`/api/customers/${id}`),

  create: (input: CreateCustomerInput) =>
    request<Customer>('/api/customers', { method: 'POST', json: input }),

  update: (id: string, input: Partial<CreateCustomerInput>) =>
    request<Customer>(`/api/customers/${id}`, { method: 'PATCH', json: input }),

  delete: (id: string) =>
    request<void>(`/api/customers/${id}`, { method: 'DELETE' }),
};

// ============================================================
// Deals
// ============================================================
export const dealsApi = {
  list: (
    params: {
      stage?: DealStage;
      customerId?: string;
      limit?: number;
      offset?: number;
    } = {},
  ) => {
    const qs = new URLSearchParams();
    if (params.stage) qs.set('stage', params.stage);
    if (params.customerId) qs.set('customerId', params.customerId);
    if (params.limit !== undefined) qs.set('limit', String(params.limit));
    if (params.offset !== undefined) qs.set('offset', String(params.offset));
    const suffix = qs.toString() ? `?${qs.toString()}` : '';
    return request<ListResponse<Deal>>(`/api/deals${suffix}`);
  },

  getById: (id: string) => request<Deal>(`/api/deals/${id}`),

  create: (input: CreateDealInput) =>
    request<Deal>('/api/deals', { method: 'POST', json: input }),

  update: (
    id: string,
    input: Partial<Omit<CreateDealInput, 'stage'>>,
  ) => request<Deal>(`/api/deals/${id}`, { method: 'PATCH', json: input }),

  transitionStage: (id: string, stage: DealStage) =>
    request<Deal>(`/api/deals/${id}/stage`, {
      method: 'PATCH',
      json: { stage },
    }),

  delete: (id: string) =>
    request<void>(`/api/deals/${id}`, { method: 'DELETE' }),
};
