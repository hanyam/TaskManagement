/* eslint-disable react/jsx-no-constructed-context-values */
"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useState } from "react";
import type { PropsWithChildren } from "react";
import { toast } from "sonner";

import type { ApiErrorResponse } from "@/core/api";

const RETRY_COUNT = 2;

function isApiError(error: unknown): error is ApiErrorResponse {
  return Boolean(error) && typeof error === "object" && (error as ApiErrorResponse).name === "ApiError";
}

export function QueryProvider({ children }: PropsWithChildren) {
  const [client] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            refetchOnWindowFocus: false,
            refetchOnReconnect: true,
            retry: RETRY_COUNT,
            staleTime: 30_000,
            meta: {
              showToastOnError: true
            }
          },
          mutations: {
            retry: 0,
            meta: {
              showToastOnError: true,
              successMessage: undefined as string | undefined
            }
          }
        },
        queryCache: undefined,
        mutationCache: undefined
      })
  );

  client.getDefaultOptions().queries = {
    ...client.getDefaultOptions().queries,
    onError: (error, _query) => {
      if (!isApiError(error)) {
        toast.error(error instanceof Error ? error.message : "Unexpected error");
        return;
      }

      if (error.type === "unauthorized") {
        toast.error("Your session has expired. Please sign in again.");
        return;
      }

      if (error.details.length > 0) {
        toast.error(error.details[0]?.message ?? "Request failed");
        return;
      }

      toast.error(error.message);
    }
  };

  client.getDefaultOptions().mutations = {
    ...client.getDefaultOptions().mutations,
    onError: (error, _variables, _context, mutation) => {
      const showToast = mutation?.meta?.showToastOnError ?? true;
      if (!showToast) {
        return;
      }

      if (!isApiError(error)) {
        toast.error(error instanceof Error ? error.message : "Unexpected error");
        return;
      }

      if (error.type === "validation" && error.details.length > 0) {
        toast.error(error.details[0]?.message ?? "Validation failed");
        return;
      }

      toast.error(error.message);
    },
    onSuccess: (_data, _variables, _context, mutation) => {
      const successMessage = mutation?.meta?.successMessage as string | undefined;
      if (successMessage) {
        toast.success(successMessage);
      }
    }
  };

  return (
    <QueryClientProvider client={client}>
      {children}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

