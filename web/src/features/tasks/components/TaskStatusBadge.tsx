import { useTranslation } from "react-i18next";

import type { TaskStatus } from "@/features/tasks/value-objects";
import { cn } from "@/ui/utils/cn";

interface TaskStatusBadgeProps {
  status: TaskStatus;
}

const STATUS_STYLES: Record<TaskStatus, string> = {
  Created: "bg-secondary text-secondary-foreground",
  Assigned: "bg-sky-100 text-sky-800 dark:bg-sky-900/60 dark:text-sky-100",
  UnderReview: "bg-amber-100 text-amber-800 dark:bg-amber-900/60 dark:text-amber-100",
  Accepted: "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/60 dark:text-emerald-100",
  Rejected: "bg-rose-100 text-rose-800 dark:bg-rose-900/60 dark:text-rose-100",
  Completed: "bg-primary text-primary-foreground",
  Cancelled: "bg-muted text-muted-foreground"
};

export function TaskStatusBadge({ status }: TaskStatusBadgeProps) {
  const { t } = useTranslation("common");
  return (
    <span
      className={cn(
        "inline-flex min-w-[6.5rem] items-center justify-center rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-wide",
        STATUS_STYLES[status]
      )}
    >
      {t(`taskStatus.${status.charAt(0).toLowerCase()}${status.slice(1) as string}`)}
    </span>
  );
}

