"use client";

import { usePathname } from "next/navigation";
import { type ReactNode, type Context, createContext, useContext } from "react";
import { useTranslation } from "react-i18next";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { Breadcrumb, type BreadcrumbItem } from "@/ui/components/Breadcrumb";

// Context for dynamic breadcrumb items (e.g., task title)
interface BreadcrumbContextValue {
  items?: BreadcrumbItem[];
}

const BreadcrumbContext: Context<BreadcrumbContextValue | undefined> = createContext<
  BreadcrumbContextValue | undefined
>(undefined);

export function BreadcrumbProvider({ children, items }: { children: ReactNode; items?: BreadcrumbItem[] }) {
  return <BreadcrumbContext.Provider value={items ? { items } : undefined}>{children}</BreadcrumbContext.Provider>;
}

export function useBreadcrumbContext() {
  return useContext(BreadcrumbContext);
}

export function BreadcrumbNav() {
  const pathname = usePathname();
  const locale = useCurrentLocale();
  const { t } = useTranslation(["navigation"]);
  const context = useBreadcrumbContext();

  // Always call hooks unconditionally
  const autoBreadcrumbs = useBreadcrumbs(pathname, locale, t);

  // If context provides breadcrumbs, use them (for dynamic content like task titles)
  if (context?.items) {
    return <Breadcrumb items={context.items} />;
  }

  // Otherwise, use auto-generated breadcrumbs
  if (autoBreadcrumbs.length === 0) {
    return null;
  }

  return <Breadcrumb items={autoBreadcrumbs} />;
}

function useBreadcrumbs(
  pathname: string,
  locale: string,
  t: (key: string) => string
): BreadcrumbItem[] {
  const segments = pathname.split("/").filter(Boolean);
  
  // Remove locale segment if present
  const pathSegments = segments[0] === locale || segments[0] === "en" || segments[0] === "ar"
    ? segments.slice(1)
    : segments;

  if (pathSegments.length === 0) {
    // Home/Dashboard
    return [{ label: t("navigation:breadcrumbs.home"), href: `/${locale}/dashboard` }];
  }

  const breadcrumbs: BreadcrumbItem[] = [
    { label: t("navigation:breadcrumbs.home"), href: `/${locale}/dashboard` }
  ];

  // Handle different routes
  if (pathSegments[0] === "dashboard") {
    breadcrumbs.push({ label: t("navigation:appShell.dashboard") });
  } else if (pathSegments[0] === "tasks") {
    breadcrumbs.push({ label: t("navigation:breadcrumbs.tasks"), href: `/${locale}/tasks` });

    if (pathSegments[1] === "create") {
      breadcrumbs.push({ label: t("navigation:breadcrumbs.createTask") });
    } else if (pathSegments[1] && pathSegments[1] !== "tasks") {
      // Task details page - show generic "Task details" (can be overridden via context)
      breadcrumbs.push({ label: t("navigation:breadcrumbs.taskDetails") });
    }
  }

  return breadcrumbs;
}

