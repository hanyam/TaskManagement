import { cookies } from "next/headers";
import { redirect } from "next/navigation";

import { AUTH_TOKEN_COOKIE } from "@/core/auth/constants";
import { isSupportedLocale, normalizeLocale } from "@/core/routing/locales";

export const dynamic = "force-dynamic";

interface LocalePageProps {
  params: {
    locale: string;
  };
  searchParams?: Record<string, string | string[] | undefined>;
}

export default function LocalePage({ params }: LocalePageProps) {
  const normalizedLocale = normalizeLocale(params.locale);

  if (!isSupportedLocale(params.locale)) {
    redirect("/en");
  }

  const token = cookies().get(AUTH_TOKEN_COOKIE)?.value;

  // Auto-redirect: authenticated users go to dashboard, others go to sign-in
  if (token) {
    redirect(`/${normalizedLocale}/dashboard`);
  } else {
    redirect(`/${normalizedLocale}/sign-in`);
  }
}

