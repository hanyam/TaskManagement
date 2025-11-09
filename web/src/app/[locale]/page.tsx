import { cookies } from "next/headers";
import { redirect } from "next/navigation";

import { AUTH_TOKEN_COOKIE } from "@/core/auth/constants";
import { isSupportedLocale, normalizeLocale } from "@/core/routing/locales";
import { SignInForm } from "@/features/auth/components/SignInForm";

export const dynamic = "force-dynamic";

interface LocalePageProps {
  params: {
    locale: string;
  };
  searchParams?: Record<string, string | string[] | undefined>;
}

export default function LocalePage({ params, searchParams }: LocalePageProps) {
  const normalizedLocale = normalizeLocale(params.locale);

  if (!isSupportedLocale(params.locale)) {
    redirect("/en");
  }

  const token = cookies().get(AUTH_TOKEN_COOKIE)?.value;

  if (token) {
    redirect(`/${normalizedLocale}/dashboard`);
  }

  const redirectTo =
    typeof searchParams?.redirect === "string" ? searchParams.redirect : undefined;

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-background via-background to-primary/10 px-4 py-16">
      <div className="w-full max-w-lg rounded-xl bg-background/80 p-8 shadow-card backdrop-blur">
        {/* Reuse sign-in form directly so users without a session land here */}
        <SignInForm locale={normalizedLocale} redirectTo={redirectTo} />
      </div>
    </div>
  );
}

