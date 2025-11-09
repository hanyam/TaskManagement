"use client";

import type { Route } from "next";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useTransition } from "react";
import { useTranslation } from "react-i18next";

import { normalizeLocale, type SupportedLocale } from "@/core/routing/locales";
import { cn } from "@/ui/utils/cn";

const LOCALE_OPTIONS: Array<{ value: SupportedLocale; label: string }> = [
  { value: "en", label: "English" },
  { value: "ar", label: "العربية" }
];

interface LocaleSwitcherProps {
  className?: string;
}

export function LocaleSwitcher({ className }: LocaleSwitcherProps) {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const currentLocale = normalizeLocale(pathname.split("/").filter(Boolean)[0] ?? "en");
  const { t } = useTranslation("navigation");
  const [, startTransition] = useTransition();

  function buildTargetPath(locale: SupportedLocale) {
    const segments = pathname.split("/").filter(Boolean);
    if (segments.length === 0) {
      return `/${locale}`;
    }
    segments[0] = locale;
    const path = `/${segments.join("/")}`;
    const queryString = searchParams.toString();
    return queryString ? `${path}?${queryString}` : path;
  }

  function handleChange(event: React.ChangeEvent<HTMLSelectElement>) {
    const targetLocale = normalizeLocale(event.target.value);
    startTransition(() => {
      router.push(buildTargetPath(targetLocale) as Route);
    });
  }

  return (
    <div className={cn("inline-flex items-center gap-2", className)}>
      <label htmlFor="locale-switcher" className="text-xs font-medium text-muted-foreground">
        {t("appShell.language")}
      </label>
      <select
        id="locale-switcher"
        name="locale"
        value={currentLocale}
        onChange={handleChange}
        className="h-9 rounded-md border border-input bg-background px-3 text-sm text-foreground focus-visible:border-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
      >
        {LOCALE_OPTIONS.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
}

