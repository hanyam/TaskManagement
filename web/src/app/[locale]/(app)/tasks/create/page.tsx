"use client";

import { TaskCreateView } from "@/features/tasks/components/TaskCreateView";
import { BreadcrumbProvider } from "@/ui/layout/BreadcrumbNav";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { useTranslation } from "react-i18next";

export default function TaskCreatePage() {
  const locale = useCurrentLocale();
  const { t } = useTranslation(["navigation"]);

  const breadcrumbItems = [
    { label: t("navigation:breadcrumbs.home"), href: `/${locale}/dashboard` },
    { label: t("navigation:breadcrumbs.tasks"), href: `/${locale}/tasks` },
    { label: t("navigation:breadcrumbs.createTask") }
  ];

  return (
    <BreadcrumbProvider items={breadcrumbItems}>
      <TaskCreateView />
    </BreadcrumbProvider>
  );
}

