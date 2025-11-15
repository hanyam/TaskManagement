import type { SupportedLocale } from "@/core/routing/locales";

export interface ApiEnvelope<T> {
  success: boolean;
  data?: T;
  message?: string | null;
  errors?: ApiErrorDetail[];
  traceId?: string | null;
  timestamp?: string;
  links?: ApiActionLink[];
}

export interface ApiActionLink {
  rel: string;
  href: string;
  method: string;
}

export interface ApiErrorDetail {
  code: string;
  message: string;
  field?: string | null;
}

export type ApiErrorType =
  | "validation"
  | "notFound"
  | "conflict"
  | "forbidden"
  | "unauthorized"
  | "internal"
  | "network"
  | "unknown";

export type ApiMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export interface ApiRequestConfig<TBody = unknown, TQuery = Record<string, unknown>> {
  path: string;
  method?: ApiMethod;
  body?: TBody;
  query?: TQuery;
  headers?: HeadersInit;
  signal?: AbortSignal;
  /**
   * By default all requests attach the bearer token when available.
   * Set to false to opt out (e.g. for anonymous commands).
   */
  auth?: boolean;
  /**
   * Some commands need idempotency keys. When provided, the header `Idempotency-Key`
   * is populated automatically.
   */
  idempotencyKey?: string;
  /**
   * Locale context to send with the request for server-side localization.
   */
  locale?: SupportedLocale;
}

export interface ApiSuccessResponse<T> {
  data: T;
  message?: string | null;
  traceId?: string | null;
  links?: ApiActionLink[] | undefined;
}

export interface ApiErrorResponse extends Error {
  status: number;
  type: ApiErrorType;
  details: ApiErrorDetail[];
  traceId?: string | null;
  message: string;
  rawMessage?: string | null;
}

export interface ApiClient {
  request<TResponse, TBody = unknown, TQuery = Record<string, unknown>>(
    config: ApiRequestConfig<TBody, TQuery>
  ): Promise<ApiSuccessResponse<TResponse>>;
}

