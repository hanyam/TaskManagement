import Link from "next/link";
import { usePathname } from "next/navigation";
import type { PropsWithChildren, ReactNode } from "react";
import { useTranslation } from "react-i18next";

import { useAuth } from "@/core/auth/AuthProvider";
import { UserMenu } from "@/core/auth/UserMenu";
import { mainNavigation } from "@/core/navigation/nav-items";
import { LocaleSwitcher } from "@/core/routing/LocaleSwitcher";
import { ThemeToggle } from "@/core/theme/ThemeToggle";
import { cn } from "@/ui/utils/cn";

interface AppShellProps extends PropsWithChildren {
  headerSlot?: ReactNode;
}

export function AppShell({ children, headerSlot }: AppShellProps) {
  const pathname = usePathname();
  const { t } = useTranslation(["navigation", "common"]);
  const { session } = useAuth();
  const segments = pathname.split("/").filter(Boolean);
  const locale = segments[0] ?? "en";

  const role = session?.user.role ?? "Employee";

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
            const isActive = item.exact ? pathname === targetPath : pathname.startsWith(normalizedTarget);
            const Icon = item.icon;

            return (
              <Link
                key={item.id}
                href={targetPath}
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

        <main className="flex flex-1 flex-col gap-6 bg-muted/10 px-4 py-6 sm:px-6 lg:px-8">{children}</main>
      </div>
    </div>
  );
}

