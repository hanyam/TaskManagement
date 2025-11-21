import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import type { ReactNode } from "react";

import { AUTH_TOKEN_COOKIE } from "@/core/auth/constants";
import { normalizeLocale } from "@/core/routing/locales";
import { AppShell } from "@/ui/layout/AppShell";

interface AppLayoutProps {
  children: ReactNode;
  params: {
    locale: string;
  };
}

export default function AppLayout({ children, params }: AppLayoutProps) {
  const token = cookies().get(AUTH_TOKEN_COOKIE)?.value;
  const locale = normalizeLocale(params.locale);

  // Require authentication for all app routes
  if (!token) {
    redirect(`/${locale}/sign-in?redirect=${encodeURIComponent(`/${locale}/dashboard`)}`);
  }

  return <AppShell>{children}</AppShell>;
}

