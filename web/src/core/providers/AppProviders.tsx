"use client";

import type { PropsWithChildren } from "react";
import { Toaster } from "sonner";

import { AuthProvider } from "@/core/auth/AuthProvider";
import type { AuthSession } from "@/core/auth/types";
import { QueryProvider } from "@/core/providers/QueryProvider";
import type { SupportedLocale } from "@/core/routing/locales";
import { ThemeProvider } from "@/core/theme/ThemeProvider";
import { I18nProvider } from "@/i18n/I18nProvider";

interface AppProvidersProps extends PropsWithChildren {
  locale: SupportedLocale;
  initialSession?: AuthSession | null;
}

export function AppProviders({ locale, initialSession, children }: AppProvidersProps) {
  return (
    <I18nProvider locale={locale}>
      <ThemeProvider>
        <AuthProvider initialSession={initialSession}>
          <QueryProvider>
            {children}
            <Toaster position="top-center" richColors closeButton />
          </QueryProvider>
        </AuthProvider>
      </ThemeProvider>
    </I18nProvider>
  );
}

