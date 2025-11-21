'use client';

import type { Route } from "next";
import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";
import { useEffect, type PropsWithChildren, type ReactNode } from "react";
import { useTranslation } from "react-i18next";

import { useAuth } from "@/core/auth/AuthProvider";
import { UserMenu } from "@/core/auth/UserMenu";
import { mainNavigation } from "@/core/navigation/nav-items";
import { LocaleSwitcher } from "@/core/routing/LocaleSwitcher";
import { ThemeToggle } from "@/core/theme/ThemeToggle";
import { BreadcrumbNav } from "@/ui/layout/BreadcrumbNav";
import { cn } from "@/ui/utils/cn";

interface AppShellProps extends PropsWithChildren {
  headerSlot?: ReactNode;
}

export function AppShell({ children, headerSlot }: AppShellProps) {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const { t } = useTranslation(["navigation", "common"]);
  const { session } = useAuth();
  const segments = pathname.split("/").filter(Boolean);
  const locale = segments[0] ?? "en";

  const role = session?.user.role ?? "Employee";

  // Check if we're on a task details page
  const isTaskDetailsPage = /^\/[^/]+\/tasks\/[^/]+$/.test(pathname);
  // Check if we're on the tasks list page
  const isTasksListPage = /^\/[^/]+\/tasks$/.test(pathname);

  // Store the active menu context when on tasks list page
  useEffect(() => {
    if (isTasksListPage) {
      const currentFilter = searchParams.get("filter");
      if (currentFilter === "assigned") {
        sessionStorage.setItem("lastActiveTasksMenu", "my-assignments");
      } else {
        sessionStorage.setItem("lastActiveTasksMenu", "tasks");
      }
    }
  }, [isTasksListPage, searchParams]);

  const navItems = mainNavigation.filter((item) => {
    if (!item.roles?.length) {
      return true;
    }
    return item.roles.includes(role as never);
  });

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      <aside className="hidden w-64 shrink-0 flex-col border-r border-border bg-background/80 px-4 py-6 lg:flex">
        <div className="mb-10 flex flex-col gap-1 px-2">
          <span className="text-xs font-semibold uppercase tracking-widest text-primary">
            {t("common:app.name")}
          </span>
          <span className="text-sm text-muted-foreground">{t("common:app.description")}</span>
        </div>

        <nav className="flex flex-1 flex-col gap-1">
          {navItems.map((item) => {
            const targetPath = `/${locale}${item.href}`;
            const normalizedTarget = targetPath.replace(/\?.*$/, "");
            
            // Parse query params from item.href
            const queryString = item.href.includes("?") 
              ? item.href.split("?")[1]?.split("#")[0] ?? ""
              : "";
            const targetParams = new URLSearchParams(queryString);
            const targetFilter = targetParams.get("filter");
            const currentFilter = searchParams.get("filter");
            
            // Check if pathname matches
            const pathMatches = item.exact 
              ? pathname === normalizedTarget 
              : pathname.startsWith(normalizedTarget);
            
            // For items with query params, also check if the query params match
            let isActive = pathMatches;
            
            // Handle task details pages - use stored context to preserve parent menu state
            if (isTaskDetailsPage && normalizedTarget.includes("/tasks")) {
              const lastActiveMenu = typeof window !== "undefined" 
                ? sessionStorage.getItem("lastActiveTasksMenu")
                : null;
              if (targetFilter === "assigned") {
                // My Assignments menu item
                isActive = lastActiveMenu === "my-assignments";
              } else if (targetFilter === null) {
                // Tasks menu item (no filter) - default to "tasks" if no context stored
                isActive = lastActiveMenu === "tasks" || lastActiveMenu === null;
              }
            } else if (targetFilter !== null) {
              // This item has a filter query param, so check if current filter matches
              isActive = pathMatches && currentFilter === targetFilter;
            } else if (pathMatches && normalizedTarget.includes("/tasks")) {
              // This is the tasks item without filter, so it should only be active if there's no filter or filter is not "assigned"
              isActive = currentFilter !== "assigned";
            }
            
            const Icon = item.icon;
            const linkHref = targetPath as Route;

            return (
              <Link
                key={item.id}
                href={linkHref}
                className={cn(
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition",
                  isActive
                    ? "bg-primary text-primary-foreground shadow"
                    : "text-muted-foreground hover:bg-secondary/60 hover:text-foreground"
                )}
              >
                <Icon className="size-5" aria-hidden />
                {t(item.labelKey)}
              </Link>
            );
          })}
        </nav>
      </aside>

      <div className="flex min-h-screen flex-1 flex-col">
        <header className="flex items-center justify-between gap-4 border-b border-border bg-background/80 px-4 py-3">
          <div className="flex items-center gap-3 lg:hidden">
            <span className="text-base font-semibold text-primary">{t("common:app.name")}</span>
          </div>

          <div className="flex flex-1 items-center justify-end gap-2">
            <LocaleSwitcher className="hidden sm:flex" />
            <ThemeToggle />
            <UserMenu />
          </div>
        </header>

        {headerSlot ? (
          <div className="border-b border-border bg-background/60 px-6 py-4">{headerSlot}</div>
        ) : null}

        <div className="border-b border-border bg-background/60 px-6 py-4">
          <BreadcrumbNav />
        </div>

        <main className="flex flex-1 flex-col gap-6 bg-muted/10 px-4 py-6 sm:px-6 lg:px-8">{children}</main>
      </div>
    </div>
  );
}

