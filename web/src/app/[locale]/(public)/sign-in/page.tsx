import { normalizeLocale, type SupportedLocale } from "@/core/routing/locales";
import { SignInForm } from "@/features/auth/components/SignInForm";

interface SignInPageProps {
  params: {
    locale: string;
  };
  searchParams: Record<string, string | string[] | undefined>;
}

export default function SignInPage({ params, searchParams }: SignInPageProps) {
  const locale = normalizeLocale(params.locale) as SupportedLocale;
  const redirectTo = typeof searchParams.redirect === "string" ? searchParams.redirect : undefined;

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-background via-background to-primary/10 px-4 py-16">
      <div className="w-full max-w-lg rounded-xl bg-background/80 p-8 shadow-card backdrop-blur">
        <SignInForm locale={locale} redirectTo={redirectTo} />
      </div>
    </div>
  );
}

