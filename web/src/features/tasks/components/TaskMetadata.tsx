"use client";

import { useTranslation } from "react-i18next";

import type { TaskDto } from "@/features/tasks/types";
import { getTaskTypeString } from "@/features/tasks/value-objects";
import { formatDate } from "@/features/tasks/utils/dateFormatting";

interface TaskMetadataProps {
  task: TaskDto;
}

export function TaskMetadata({ task }: TaskMetadataProps) {
  const { t } = useTranslation(["tasks", "common"]);

  const metadata = [
    {
      label: t("tasks:details.metadata.createdBy"),
      value: task.createdBy
    },
    {
      label: t("tasks:details.metadata.createdAt"),
      value: formatDate(task.createdAt)
    },
    {
      label: t("tasks:details.metadata.updatedAt"),
      value: task.updatedAt ? formatDate(task.updatedAt) : "—"
    },
    {
      label: t("tasks:details.metadata.originalDueDate"),
      value: task.originalDueDate ? formatDate(task.originalDueDate) : "—"
    },
    {
      label: t("tasks:details.metadata.extendedDueDate"),
      value: task.extendedDueDate ? formatDate(task.extendedDueDate) : "—"
    },
    {
      label: t("tasks:details.metadata.type"),
      value: (() => {
        const typeString = getTaskTypeString(task.type);
        return t(`common:taskType.${typeString.charAt(0).toLowerCase()}${typeString.slice(1)}`);
      })()
    },
    {
      label: t("tasks:details.metadata.progress"),
      value: task.progressPercentage != null ? `${task.progressPercentage}%` : "—"
    }
  ];

  return (
    <aside className="space-y-3 rounded-xl border border-border bg-background p-6 shadow-sm">
      <h3 className="text-lg font-semibold text-foreground">{t("tasks:details.title")}</h3>
      <dl className="space-y-3 text-sm">
        {metadata.map((item) => (
          <div key={item.label} className="flex flex-col">
            <dt className="text-xs text-muted-foreground">{item.label}</dt>
            <dd className="font-medium text-foreground">{item.value}</dd>
          </div>
        ))}
      </dl>
    </aside>
  );
}

