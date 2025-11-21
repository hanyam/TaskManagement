"use client";

import { Menu, Transition } from "@headlessui/react";
import { useRouter } from "next/navigation";
import { Fragment } from "react";
import { useTranslation } from "react-i18next";

import { useAuth } from "@/core/auth/AuthProvider";
import { Button } from "@/ui/components/Button";
import { Spinner } from "@/ui/components/Spinner";
import { cn } from "@/ui/utils/cn";

export function UserMenu() {
  const { session, clearSession } = useAuth();
  const router = useRouter();
  const { t } = useTranslation(["common"]);

  async function handleSignOut() {
    const locale = typeof window !== "undefined" ? window.location.pathname.split("/")[1] || "en" : "en";
    await fetch("/api/auth/logout", {
      method: "POST",
      credentials: "include"
    });
    clearSession();
    router.push(`/${locale}/signed-out`);
    router.refresh();
  }

  if (!session?.user) {
    return (
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <Spinner size="sm" />
        {t("common:states.loading")}
      </div>
    );
  }

  const initials = session.user.displayName
    .split(" ")
    .map((part) => part.charAt(0))
    .join("")
    .slice(0, 2)
    .toUpperCase();

  return (
    <Menu as="div" className="relative inline-block text-left">
      <Menu.Button as={Button} variant="ghost" size="sm" className="flex items-center gap-2">
        <span className="flex size-8 items-center justify-center rounded-full bg-primary/10 text-sm font-semibold text-primary">
          {initials}
        </span>
        <span className="hidden flex-col items-start text-left sm:flex">
          <span className="text-sm font-medium text-foreground">{session.user.displayName}</span>
          <span className="text-xs text-muted-foreground">{session.user.email}</span>
        </span>
      </Menu.Button>

      <Transition
        as={Fragment}
        enter="transition ease-out duration-100"
        enterFrom="transform opacity-0 scale-95"
        enterTo="transform opacity-100 scale-100"
        leave="transition ease-in duration-75"
        leaveFrom="transform opacity-100 scale-100"
        leaveTo="transform opacity-0 scale-95"
      >
        <Menu.Items className="absolute right-0 mt-2 w-48 origin-top-right rounded-lg border border-border bg-background p-1 shadow-lg focus:outline-none">
          <Menu.Item>
            {({ active }) => (
              <button
                type="button"
                className={cn(
                  "w-full rounded-md px-3 py-2 text-left text-sm text-destructive hover:bg-destructive/10",
                  active && "bg-destructive/10"
                )}
                onClick={handleSignOut}
              >
                {t("common:actions.signOut")}
              </button>
            )}
          </Menu.Item>
        </Menu.Items>
      </Transition>
    </Menu>
  );
}

