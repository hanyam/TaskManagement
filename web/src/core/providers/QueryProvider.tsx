/* eslint-disable react/jsx-no-constructed-context-values */
"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { DefaultOptions } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { useRouter } from "next/navigation";
import { useState } from "react";
import type { PropsWithChildren } from "react";
import { toast } from "sonner";

import type { ApiErrorResponse } from "@/core/api";
import { useAuth } from "@/core/auth/AuthProvider";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";

const RETRY_COUNT = 2;

function isApiError(error: unknown): error is ApiErrorResponse {
  return Boolean(error) && typeof error === "object" && (error as ApiErrorResponse).name === "ApiError";
}

export function QueryProvider({ children }: PropsWithChildren) {
  const { clearSession } = useAuth();
  const router = useRouter();
  const locale = useCurrentLocale();

  const handleUnauthorized = async () => {
    // Clear session from localStorage and state
    clearSession();
    
    // Clear server-side cookies
    try {
      await fetch("/api/auth/logout", {
        method: "POST",
        credentials: "include"
      });
    } catch {
      // Ignore errors when clearing cookies
    }
    
    // Redirect to sign-in
    router.push(`/${locale}/sign-in`);
    router.refresh();
  };

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
        }
      })
  );

  const enhancedDefaults = {
    queries: {
      ...(client.getDefaultOptions().queries ?? {}),
      onError: (error: unknown, _query: unknown) => {
        if (!isApiError(error)) {
          toast.error(error instanceof Error ? error.message : "Unexpected error");
          return;
        }

        if (error.type === "unauthorized") {
          toast.error("Your session has expired. Please sign in again.");
          // Automatically logout and redirect
          handleUnauthorized();
          return;
        }

        if (error.details.length > 0) {
          toast.error(error.details[0]?.message ?? "Request failed");
          return;
        }

        toast.error(error.message);
      }
    } as unknown as DefaultOptions["queries"],
    mutations: {
      ...(client.getDefaultOptions().mutations ?? {}),
      onError: (
        error: unknown,
        _variables: unknown,
        _context: unknown,
        mutation: { meta?: Record<string, unknown> } | undefined
      ) => {
        const showToast = mutation?.meta?.showToastOnError ?? true;
        if (!showToast) {
          return;
        }

        if (!isApiError(error)) {
          toast.error(error instanceof Error ? error.message : "Unexpected error");
          return;
        }

        if (error.type === "unauthorized") {
          toast.error("Your session has expired. Please sign in again.");
          // Automatically logout and redirect
          handleUnauthorized();
          return;
        }

        if (error.type === "validation" && error.details.length > 0) {
          toast.error(error.details[0]?.message ?? "Validation failed");
          return;
        }

        toast.error(error.message);
      },
      onSuccess: (
        _data: unknown,
        _variables: unknown,
        _context: unknown,
        mutation: { meta?: Record<string, unknown> } | undefined
      ) => {
        const successMessage = mutation?.meta?.successMessage as string | undefined;
        if (successMessage) {
          toast.success(successMessage);
        }
      }
    } as unknown as DefaultOptions["mutations"]
  } as DefaultOptions;

  client.setDefaultOptions(enhancedDefaults);

  return (
    <QueryClientProvider client={client}>
      {children}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}

