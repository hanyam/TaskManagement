"use client";

import type { Route } from "next";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useTransition } from "react";

import { normalizeLocale, type SupportedLocale } from "@/core/routing/locales";
import { Button } from "@/ui/components/Button";
import { cn } from "@/ui/utils/cn";

interface LocaleSwitcherProps {
  className?: string;
}

export function LocaleSwitcher({ className }: LocaleSwitcherProps) {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const currentLocale = normalizeLocale(pathname.split("/").filter(Boolean)[0] ?? "en");
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

  function handleToggle() {
    const targetLocale: SupportedLocale = currentLocale === "en" ? "ar" : "en";
    startTransition(() => {
      router.push(buildTargetPath(targetLocale) as Route);
    });
  }

  // Show opposite language indicator
  const displayText = currentLocale === "en" ? "ع" : "en";

  return (
    <Button
      variant="ghost"
      size="sm"
      onClick={handleToggle}
      className={cn("min-w-[2.5rem] font-medium", className)}
      aria-label={currentLocale === "en" ? "Switch to Arabic" : "Switch to English"}
    >
      {displayText}
    </Button>
  );
}

