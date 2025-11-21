"use client";

import { ChevronRightIcon, ChevronLeftIcon } from "@heroicons/react/24/outline";
import type { Route } from "next";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useTranslation } from "react-i18next";

import { getDirection } from "@/core/routing/locales";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { Button } from "@/ui/components/Button";
import { cn } from "@/ui/utils/cn";

export interface BreadcrumbItem {
  label: string;
  href?: Route | string;
}

interface BreadcrumbProps {
  items: BreadcrumbItem[];
  showBackButton?: boolean;
  onBack?: () => void;
  className?: string;
}

export function Breadcrumb({ items, showBackButton = true, onBack, className }: BreadcrumbProps) {
  const router = useRouter();
  const locale = useCurrentLocale();
  const direction = getDirection(locale);
  const { t } = useTranslation(["navigation"]);

  function handleBack() {
    if (onBack) {
      onBack();
    } else {
      router.back();
    }
  }

  const ChevronIcon = direction === "rtl" ? ChevronLeftIcon : ChevronRightIcon;

  return (
    <nav className={cn("flex items-center gap-2", className)} aria-label="Breadcrumb">
      {showBackButton && (
        <Button
          variant="ghost"
          size="sm"
          onClick={handleBack}
          icon={direction === "rtl" ? <ChevronRightIcon className="h-4 w-4" /> : <ChevronLeftIcon className="h-4 w-4" />}
          className="mr-2"
          aria-label={t("navigation:breadcrumbs.back")}
        />
      )}
      <ol className="flex items-center gap-2 text-sm">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          return (
            <li key={index} className="flex items-center gap-2">
              {index > 0 && (
                <ChevronIcon className="h-4 w-4 text-muted-foreground" aria-hidden="true" />
              )}
              {isLast ? (
                <span className="font-medium text-foreground" aria-current="page">
                  {item.label}
                </span>
              ) : item.href ? (
                <Link
                  href={item.href as Route}
                  className="text-muted-foreground transition-colors hover:text-foreground"
                >
                  {item.label}
                </Link>
              ) : (
                <span className="text-muted-foreground">{item.label}</span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}

