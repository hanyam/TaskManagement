import { useTranslation } from "react-i18next";

import type { TaskStatus } from "@/features/tasks/value-objects";
import { getTaskStatusString } from "@/features/tasks/value-objects";
import { cn } from "@/ui/utils/cn";

interface TaskStatusBadgeProps {
  status: number; // Numeric enum from backend
  managerRating?: number | null | undefined; // If present and status is Accepted, show "Accepted by Manager"
}

const STATUS_STYLES: Record<TaskStatus, string> = {
  Created: "bg-secondary text-secondary-foreground",
  Assigned: "bg-sky-100 text-sky-800 dark:bg-sky-900/60 dark:text-sky-100",
  UnderReview: "bg-amber-100 text-amber-800 dark:bg-amber-900/60 dark:text-amber-100",
  Accepted: "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/60 dark:text-emerald-100",
  Rejected: "bg-rose-100 text-rose-800 dark:bg-rose-900/60 dark:text-rose-100",
  Completed: "bg-primary text-primary-foreground",
  Cancelled: "bg-muted text-muted-foreground",
  PendingManagerReview: "bg-orange-100 text-orange-800 dark:bg-orange-900/60 dark:text-orange-100",
  RejectedByManager: "bg-red-100 text-red-900 dark:bg-red-900/60 dark:text-red-100"
};

export function TaskStatusBadge({ status, managerRating }: TaskStatusBadgeProps) {
  const { t } = useTranslation("common");
  const statusString = getTaskStatusString(status);
  
  // If status is Accepted (3) and has managerRating, show "Accepted by Manager"
  const displayStatus = status === 3 && managerRating != null 
    ? "acceptedByManager" 
    : statusString;
  
  return (
    <span
      className={cn(
        "inline-flex min-w-[6.5rem] items-center justify-center rounded-full px-3 py-1 text-xs font-semibold uppercase tracking-wide",
        STATUS_STYLES[statusString]
      )}
    >
      {displayStatus === "acceptedByManager" 
        ? t("taskStatus.acceptedByManager")
        : t(`taskStatus.${statusString.charAt(0).toLowerCase()}${statusString.slice(1) as string}`)}
    </span>
  );
}

