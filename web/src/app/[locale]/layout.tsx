import { notFound } from "next/navigation";
import type { ReactNode } from "react";

import { clearServerAuthSession, getServerAuthToken, getServerUser } from "@/core/auth/session.server";
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

  // Safely get token and user, handling any errors
  let initialSession: AuthSession | null = null;
  try {
    const token = getServerAuthToken(); // This now validates expiration
    const user = getServerUser();

    // Only create session if token is valid (not expired)
    if (token && user) {
      initialSession = {
        token,
        user
      };
    } else if (token === undefined && user) {
      // Token was expired or invalid - clear the session
      // Only clear if we had a user (meaning there was a session)
      try {
        clearServerAuthSession();
      } catch {
        // Ignore errors when clearing
      }
    }
  } catch (error) {
    // If token validation fails, just proceed without session
    // This prevents the entire page from crashing
    // Server-side error - log to server console (debug logger may not work in server components)
    // eslint-disable-next-line no-console
    console.error("Error validating server session:", error);
    initialSession = null;
  }

  return (
    <AppProviders locale={locale} initialSession={initialSession}>
      {children}
    </AppProviders>
  );
}

