import { createApiError, createNetworkError } from "@/core/api/errors";
import { parseEnvelope } from "@/core/api/response";
import type {
  ApiClient,
  ApiEnvelope,
  ApiRequestConfig,
  ApiSuccessResponse
} from "@/core/api/types";
import { getEnvConfig } from "@/core/config/env";
import { getDirection } from "@/core/routing/locales";

type ResolveAuthTokenFn = () => Promise<string | undefined>;

const JSON_METHODS = new Set(["POST", "PUT", "PATCH"] as const);

function buildUrl(path: string, query?: Record<string, unknown>): URL {
  const baseUrl = getEnvConfig().apiBaseUrl;
  const url = new URL(path.startsWith("http") ? path : `${baseUrl}${path}`);

  if (query && Object.keys(query).length > 0) {
    const searchParams = new URLSearchParams(url.search);
    Object.entries(query).forEach(([key, value]) => {
      if (value === undefined || value === null) {
        return;
      }

      if (Array.isArray(value)) {
        value
          .filter((item) => item !== undefined && item !== null)
          .forEach((item) => searchParams.append(key, String(item)));
        return;
      }

      searchParams.set(key, String(value));
    });

    url.search = searchParams.toString();
  }

  return url;
}

function buildHeaders<TBody, TQuery>(
  method: string,
  authToken: string | undefined,
  config: ApiRequestConfig<TBody, TQuery>
): Headers {
  const headers = new Headers(config.headers ?? {});
  headers.set("Accept", "application/json");

  if (JSON_METHODS.has(method as typeof JSON_METHODS extends Set<infer U> ? U : never)) {
    headers.set("Content-Type", "application/json");
  }

  if (config.locale) {
    headers.set("Accept-Language", config.locale);
    headers.set("X-Locale", config.locale);
    headers.set("X-Locale-Dir", getDirection(config.locale));
  }

  if (config.auth !== false && authToken) {
    headers.set("Authorization", `Bearer ${authToken}`);
  }

  if (config.idempotencyKey) {
    headers.set("Idempotency-Key", config.idempotencyKey);
  }

  return headers;
}

async function parseJson<T>(response: Response): Promise<ApiEnvelope<T>> {
  try {
    const json = (await response.json()) as ApiEnvelope<T>;
    return parseEnvelope<T>(json);
  } catch (error) {
    throw createNetworkError(error);
  }
}

export function createApiClient(resolveAuthToken: ResolveAuthTokenFn): ApiClient {
  return {
    async request<TResponse, TBody = unknown, TQuery = Record<string, unknown>>(
      config: ApiRequestConfig<TBody, TQuery>
    ): Promise<ApiSuccessResponse<TResponse>> {
      const method = (config.method ?? "GET").toUpperCase();
      const authToken =
        config.auth === false
          ? undefined
          : await resolveAuthToken();
      const url = buildUrl(config.path, config.query as Record<string, unknown>);
      const headers = buildHeaders(method, authToken, config);

      const controller = new AbortController();
      const signal = config.signal ?? controller.signal;

      const requestInit: RequestInit = {
        method,
        headers,
        signal,
        cache: "no-store",
        credentials: "include"
      };

      if (config.body !== undefined && config.body !== null) {
        requestInit.body = JSON.stringify(config.body);
      }

      try {
        const response = await fetch(url, requestInit);
        const envelope = await parseJson<TResponse>(response);

        // Debug logging (only if enabled)
        if (typeof window !== "undefined") {
          const { debugApi } = await import("@/core/debug/logger");
          debugApi(method, config.path, {
            status: response.status,
            ok: response.ok,
            hasData: envelope.data !== undefined,
            hasLinks: (envelope.links?.length ?? 0) > 0,
            linksCount: envelope.links?.length || 0
          });
        }

        // Handle 401 Unauthorized - token expired or invalid
        if (response.status === 401 && config.auth !== false) {
          // Clear expired session
          if (typeof window !== "undefined") {
            const { clearClientAuthSession } = await import("@/core/auth/session.client");
            clearClientAuthSession();

            // Try to refresh token silently
            const { attemptSilentTokenRefresh } = await import("@/core/auth/tokenRefresh");
            const refreshedToken = await attemptSilentTokenRefresh();

            if (refreshedToken) {
              // Retry the request with the new token (only once)
              const newHeaders = buildHeaders(method, refreshedToken, config);
              const retryResponse = await fetch(url, {
                ...requestInit,
                headers: newHeaders
              });

              if (retryResponse.ok) {
                const retryEnvelope = await parseJson<TResponse>(retryResponse);
                if (retryEnvelope.success && retryEnvelope.data !== undefined) {
                  return {
                    data: retryEnvelope.data,
                    message: retryEnvelope.message ?? null,
                    traceId: retryEnvelope.traceId ?? null,
                    links: retryEnvelope.links ?? undefined
                  };
                }
              }
            }

            // If retry failed or not applicable, redirect to sign-in
            if (!window.location.pathname.includes("/sign-in")) {
              const currentPath = window.location.pathname + window.location.search;
              window.location.href = `/sign-in?redirect=${encodeURIComponent(currentPath)}`;
            }
          }

          throw createApiError({
            status: response.status,
            envelope,
            fallbackMessage: "Authentication required. Please sign in again."
          });
        }

        if (!response.ok || !envelope.success) {
          throw createApiError({
            status: response.status,
            envelope,
            fallbackMessage: response.statusText
          });
        }

        if (envelope.data === undefined) {
          return {
            data: {} as TResponse,
            message: envelope.message ?? null,
            traceId: envelope.traceId ?? null,
            links: envelope.links ?? undefined
          };
        }

        return {
          data: envelope.data,
          message: envelope.message ?? null,
          traceId: envelope.traceId ?? null,
          links: envelope.links ?? undefined
        };
      } catch (error) {
        if (error instanceof Error && error.name === "ApiError") {
          throw error;
        }

        throw createNetworkError(error);
      } finally {
        if (!config.signal) {
          controller.abort();
        }
      }
    }
  };
}


