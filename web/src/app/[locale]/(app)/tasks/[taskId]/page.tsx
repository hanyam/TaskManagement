"use client";

import { TaskDetailsView } from "@/features/tasks/components/TaskDetailsView";
import { BreadcrumbProvider } from "@/ui/layout/BreadcrumbNav";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { useTaskDetailsQuery } from "@/features/tasks/api/queries";
import { useTranslation } from "react-i18next";

interface TaskDetailsPageProps {
  params: {
    taskId: string;
  };
}

export default function TaskDetailsPage({ params }: TaskDetailsPageProps) {
  const locale = useCurrentLocale();
  const { t } = useTranslation(["navigation"]);
  const { data: response } = useTaskDetailsQuery(params.taskId, true);
  const task = response?.data;

  const breadcrumbItems = [
    { label: t("navigation:breadcrumbs.home"), href: `/${locale}/dashboard` },
    { label: t("navigation:breadcrumbs.tasks"), href: `/${locale}/tasks` },
    { label: task?.title ?? t("navigation:breadcrumbs.taskDetails") }
  ];

  return (
    <BreadcrumbProvider items={breadcrumbItems}>
      <TaskDetailsView taskId={params.taskId} />
    </BreadcrumbProvider>
  );
}

