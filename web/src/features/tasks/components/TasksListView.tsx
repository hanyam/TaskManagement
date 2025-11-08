"use client";

import Link from "next/link";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { useTasksQuery } from "@/features/tasks/api/queries";
import { TasksTable } from "@/features/tasks/components/tables/TasksTable";
import type { TaskListFilters, TaskPriority, TaskStatus } from "@/features/tasks/types";
import { Button } from "@/ui/components/Button";
import { Input } from "@/ui/components/Input";

const STATUS_OPTIONS: TaskStatus[] = ["Created", "Assigned", "UnderReview", "Accepted", "Rejected", "Completed", "Cancelled"];
const PRIORITY_OPTIONS: TaskPriority[] = ["Low", "Medium", "High", "Critical"];

const PAGE_SIZE_OPTIONS = [10, 25, 50];

export function TasksListView() {
  const { t } = useTranslation(["tasks", "common"]);
  const locale = useCurrentLocale();
  const [filters, setFilters] = useState<TaskListFilters>({
    page: 1,
    pageSize: 25
  });

  const { data, isLoading, refetch } = useTasksQuery(filters);

  function handleFilterChange<T extends keyof TaskListFilters>(key: T, value: TaskListFilters[T]) {
    setFilters((current) => ({
      ...current,
      [key]: value,
      page: key === "pageSize" ? 1 : current.page
    }));
  }

  const totalPages = data?.totalPages ?? 1;
  const currentPage = filters.page ?? 1;

  const paginationLabel = useMemo(() => {
    if (!data) {
      return "";
    }

    const start = (currentPage - 1) * (filters.pageSize ?? 0) + 1;
    const end = Math.min(start + (filters.pageSize ?? 0) - 1, data.totalCount);

    return t("tasks:list.summary", {
      start,
      end,
      total: data.totalCount
    });
  }, [data, currentPage, filters.pageSize, t]);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 rounded-xl border border-border bg-background p-5 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h1 className="font-heading text-xl text-foreground">{t("tasks:list.title")}</h1>
            <p className="text-sm text-muted-foreground">{t("tasks:list.subtitle")}</p>
          </div>
          <div className="flex items-center gap-2">
            <Button asChild>
              <Link href={`/${locale}/tasks/create`}>{t("common:actions.create")}</Link>
            </Button>
            <Button variant="outline" onClick={() => refetch()}>
              {t("common:actions.refresh")}
            </Button>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <div className="flex flex-col gap-2">
            <label htmlFor="status-filter" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.status")}
            </label>
            <select
              id="status-filter"
              className="h-10 rounded-md border border-input bg-background px-3 text-sm text-foreground"
              value={filters.status ?? ""}
              onChange={(event) =>
                handleFilterChange("status", event.target.value ? (event.target.value as TaskStatus) : undefined)
              }
            >
              <option value="">{t("common:forms.select")}</option>
              {STATUS_OPTIONS.map((option) => (
                <option key={option} value={option}>
                  {t(`common:taskStatus.${option.charAt(0).toLowerCase()}${option.slice(1)}`)}
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-2">
            <label htmlFor="priority-filter" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.priority")}
            </label>
            <select
              id="priority-filter"
              className="h-10 rounded-md border border-input bg-background px-3 text-sm text-foreground"
              value={filters.priority ?? ""}
              onChange={(event) =>
                handleFilterChange(
                  "priority",
                  event.target.value ? (event.target.value as TaskPriority) : undefined
                )
              }
            >
              <option value="">{t("common:forms.select")}</option>
              {PRIORITY_OPTIONS.map((option) => (
                <option key={option} value={option}>
                  {t(`common:priority.${option.toLowerCase()}`)}
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-2">
            <label htmlFor="dueDateFrom" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.dueDate")} — {t("common:date.today")}
            </label>
            <Input
              id="dueDateFrom"
              type="date"
              value={filters.dueDateFrom ?? ""}
              onChange={(event) => handleFilterChange("dueDateFrom", event.target.value || undefined)}
            />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="dueDateTo" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.dueDate")} — {t("common:date.overdueBy", { count: 0 })}
            </label>
            <Input
              id="dueDateTo"
              type="date"
              value={filters.dueDateTo ?? ""}
              onChange={(event) => handleFilterChange("dueDateTo", event.target.value || undefined)}
            />
          </div>
        </div>
      </div>

      <TasksTable data={data?.tasks ?? []} isLoading={isLoading} />

      <div className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-background px-4 py-3 shadow-sm">
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">{paginationLabel}</span>
        </div>
        <div className="flex items-center gap-3">
          <select
            className="h-10 rounded-md border border-input bg-background px-3 text-sm text-foreground"
            value={filters.pageSize}
            onChange={(event) => handleFilterChange("pageSize", Number(event.target.value))}
          >
            {PAGE_SIZE_OPTIONS.map((size) => (
              <option key={size} value={size}>
                {t("common:forms.perPage", { count: size })}
              </option>
            ))}
          </select>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage <= 1}
              onClick={() => handleFilterChange("page", Math.max(1, currentPage - 1))}
            >
              {t("common:actions.previous")}
            </Button>
            <span className="text-sm text-muted-foreground">
              {currentPage} / {totalPages}
            </span>
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage >= totalPages}
              onClick={() => handleFilterChange("page", Math.min(totalPages, currentPage + 1))}
            >
              {t("common:actions.next")}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}

