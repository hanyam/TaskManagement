"use client";

import { CheckCircleIcon, ClockIcon, XCircleIcon, ArrowPathIcon, DocumentCheckIcon, UserIcon } from "@heroicons/react/24/outline";
import { useTranslation } from "react-i18next";
import { formatDistanceToNow } from "date-fns";

import { useTaskHistoryQuery } from "@/features/tasks/api/queries";
import { Spinner } from "@/ui/components/Spinner";
import { cn } from "@/ui/utils/cn";

interface TaskHistoryTimelineProps {
  taskId: string;
}


// Get icon and color for each action type
function getActionIcon(action: string) {
  const lowerAction = action.toLowerCase();
  
  if (lowerAction.includes("created")) {
    return { icon: DocumentCheckIcon, color: "text-blue-500", bgColor: "bg-blue-100 dark:bg-blue-900/30" };
  }
  if (lowerAction.includes("accepted") || lowerAction.includes("accept")) {
    return { icon: CheckCircleIcon, color: "text-green-500", bgColor: "bg-green-100 dark:bg-green-900/30" };
  }
  if (lowerAction.includes("rejected") || lowerAction.includes("reject")) {
    return { icon: XCircleIcon, color: "text-red-500", bgColor: "bg-red-100 dark:bg-red-900/30" };
  }
  if (lowerAction.includes("assigned") || lowerAction.includes("assign")) {
    return { icon: UserIcon, color: "text-purple-500", bgColor: "bg-purple-100 dark:bg-purple-900/30" };
  }
  if (lowerAction.includes("reviewed") || lowerAction.includes("review")) {
    return { icon: CheckCircleIcon, color: "text-indigo-500", bgColor: "bg-indigo-100 dark:bg-indigo-900/30" };
  }
  if (lowerAction.includes("sent back") || lowerAction.includes("rework")) {
    return { icon: ArrowPathIcon, color: "text-orange-500", bgColor: "bg-orange-100 dark:bg-orange-900/30" };
  }
  if (lowerAction.includes("completed") || lowerAction.includes("complete")) {
    return { icon: CheckCircleIcon, color: "text-emerald-500", bgColor: "bg-emerald-100 dark:bg-emerald-900/30" };
  }
  
  return { icon: ClockIcon, color: "text-gray-500", bgColor: "bg-gray-100 dark:bg-gray-800" };
}

export function TaskHistoryTimeline({ taskId }: TaskHistoryTimelineProps) {
  const { t } = useTranslation(["tasks", "common"]);
  const { data: history, isLoading, error } = useTaskHistoryQuery(taskId);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Spinner size="md" />
      </div>
    );
  }

  if (error || !history || history.length === 0) {
    return (
      <div className="rounded-lg border border-gray-200 bg-white p-6 dark:border-gray-700 dark:bg-gray-800">
        <p className="text-center text-sm text-gray-500 dark:text-gray-400">
          {error ? t("tasks:history.errorLoading") : t("tasks:history.noHistory")}
        </p>
      </div>
    );
  }

  return (
    <div className="rounded-lg border border-gray-200 bg-white dark:border-gray-700 dark:bg-gray-800">
      <div className="border-b border-gray-200 px-6 py-4 dark:border-gray-700">
        <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
          {t("tasks:history.title")}
        </h3>
        <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
          {t("tasks:history.description")}
        </p>
      </div>
      
      <div className="p-6">
        <div className="relative">
          {/* Timeline line */}
          <div className="absolute left-6 top-0 bottom-0 w-0.5 bg-gray-200 dark:bg-gray-700" />
          
          <div className="space-y-6">
            {history.map((entry) => {
              const { icon: Icon, color, bgColor } = getActionIcon(entry.action);
              const fromStatusKey = `tasks:history.statusNames.${entry.fromStatus}`;
              const toStatusKey = `tasks:history.statusNames.${entry.toStatus}`;
              const fromStatusName = t(fromStatusKey) !== fromStatusKey 
                ? t(fromStatusKey) 
                : t("tasks:history.statusFallback", { status: entry.fromStatus.toString() });
              const toStatusName = t(toStatusKey) !== toStatusKey 
                ? t(toStatusKey) 
                : t("tasks:history.statusFallback", { status: entry.toStatus.toString() });
              const statusChanged = entry.fromStatus !== entry.toStatus;
              
              return (
                <div key={entry.id} className="relative flex items-start gap-4">
                  {/* Icon */}
                  <div className={cn(
                    "relative z-10 flex h-12 w-12 shrink-0 items-center justify-center rounded-full",
                    bgColor
                  )}>
                    <Icon className={cn("h-6 w-6", color)} />
                  </div>
                  
                  {/* Content */}
                  <div className="min-w-0 flex-1 pb-6">
                    <div className="flex items-start justify-between gap-4">
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center gap-2">
                          <p className="font-semibold text-gray-900 dark:text-gray-100">
                            {entry.action}
                          </p>
                          {statusChanged && (
                            <span className="text-xs text-gray-500 dark:text-gray-400">
                              {fromStatusName} → {toStatusName}
                            </span>
                          )}
                        </div>
                        
                        <div className="mt-1 flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                          <span>
                            {entry.performedByName || entry.performedByEmail || t("common:unknown")}
                          </span>
                          {entry.performedByEmail && entry.performedByName && (
                            <span className="text-gray-400">•</span>
                          )}
                          {entry.performedByEmail && (
                            <span className="text-gray-500">{entry.performedByEmail}</span>
                          )}
                        </div>
                        
                        {entry.notes && (
                          <div className="mt-2 rounded-md bg-gray-50 p-3 dark:bg-gray-900/50">
                            <p className="text-sm text-gray-700 dark:text-gray-300">{entry.notes}</p>
                          </div>
                        )}
                      </div>
                      
                      <div className="shrink-0 text-right">
                        <time
                          className="text-xs text-gray-500 dark:text-gray-400"
                          dateTime={entry.createdAt}
                        >
                          {formatDistanceToNow(new Date(entry.createdAt), { addSuffix: true })}
                        </time>
                        <p className="mt-1 text-xs text-gray-400 dark:text-gray-500">
                          {new Date(entry.createdAt).toLocaleString()}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}

