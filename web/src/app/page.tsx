import { cookies, headers } from "next/headers";
import { redirect } from "next/navigation";

import { AUTH_TOKEN_COOKIE } from "@/core/auth/constants";
import {
  DEFAULT_LOCALE,
  LOCALE_COOKIE_KEY,
  normalizeLocale,
  resolveLocaleFromAcceptLanguage
} from "@/core/routing/locales";

export default function Home() {
  const cookieStore = cookies();
  const storedLocale =
    cookieStore.get(LOCALE_COOKIE_KEY)?.value ?? cookieStore.get("NEXT_LOCALE")?.value ?? null;

  const headerList = headers();
  const acceptLanguage = headerList.get("accept-language");
  const browserLocale = resolveLocaleFromAcceptLanguage(acceptLanguage) ?? undefined;

  const locale = normalizeLocale(storedLocale ?? browserLocale ?? DEFAULT_LOCALE);
  const token = cookieStore.get(AUTH_TOKEN_COOKIE)?.value;

  // Auto-sign in: redirect to dashboard if authenticated, otherwise to sign-in
  if (token) {
    redirect(`/${locale}/dashboard`);
  } else {
    redirect(`/${locale}/sign-in`);
  }
}
