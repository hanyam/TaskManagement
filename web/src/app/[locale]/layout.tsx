import { notFound } from "next/navigation";
import type { ReactNode } from "react";

import { getServerAuthToken, getServerUser } from "@/core/auth/session.server";
import type { AuthSession } from "@/core/auth/types";
import { AppProviders } from "@/core/providers/AppProviders";
import { isSupportedLocale, normalizeLocale } from "@/core/routing/locales";

interface LocaleLayoutProps {
  children: ReactNode;
  params: {
    locale: string;
  };
}

export default function LocaleLayout({ children, params }: LocaleLayoutProps) {
  if (!isSupportedLocale(params.locale)) {
    notFound();
  }

  const locale = normalizeLocale(params.locale);

  const token = getServerAuthToken();
  const user = getServerUser();

  const initialSession: AuthSession | null =
    token && user
      ? {
          token,
          user
        }
      : null;

  return (
    <AppProviders locale={locale} initialSession={initialSession}>
      {children}
    </AppProviders>
  );
}

