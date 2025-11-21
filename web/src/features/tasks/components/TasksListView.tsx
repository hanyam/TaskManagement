"use client";

import {
  ArrowLeftIcon,
  ArrowPathIcon,
  ArrowRightIcon,
  PlusIcon,
  UserGroupIcon,
  UserIcon
} from "@heroicons/react/24/outline";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { useTasksQuery } from "@/features/tasks/api/queries";
import { TasksTable } from "@/features/tasks/components/tables/TasksTable";
import type { TaskListFilters, TaskListViewFilter } from "@/features/tasks/types";
import type { TaskPriority, TaskStatus } from "@/features/tasks/value-objects";
import { Button } from "@/ui/components/Button";
import { DatePicker } from "@/ui/components/DatePicker";
import { Select } from "@/ui/components/Select";

const STATUS_OPTIONS: TaskStatus[] = ["Created", "Assigned", "UnderReview", "Accepted", "Rejected", "Completed", "Cancelled", "PendingManagerReview", "RejectedByManager"];
const PRIORITY_OPTIONS: TaskPriority[] = ["Low", "Medium", "High", "Critical"];

const PAGE_SIZE_OPTIONS = [10, 25, 50];

export function TasksListView() {
  const { t } = useTranslation(["tasks", "common"]);
  const locale = useCurrentLocale();
  const router = useRouter();
  const searchParams = useSearchParams();
  const [selectedTaskId, setSelectedTaskId] = useState<string | undefined>(undefined);
  const filterParam = (searchParams.get("filter") as TaskListViewFilter | null) ?? "created";
  const [filters, setFilters] = useState<TaskListFilters>({
    page: 1,
    pageSize: 25,
    filter: filterParam
  });

  useEffect(() => {
    setFilters((current) => {
      if (current.filter === filterParam) {
        return current;
      }

      return {
        ...current,
        filter: filterParam,
        page: 1
      };
    });
  }, [filterParam]);

  const { data, isLoading, refetch } = useTasksQuery(filters);

  function handleFilterChange<T extends keyof TaskListFilters>(key: T, value: TaskListFilters[T]) {
  setFilters((current) => {
    const next: TaskListFilters = { ...current };

    if (value === undefined) {
      delete next[key];
    } else {
      next[key] = value as TaskListFilters[T];
    }

    if (key === "pageSize") {
      next.page = 1;
    }

    return next;
  });
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

  const currentFilter = filters.filter ?? "created";

  function handleFilterToggle(nextFilter: TaskListViewFilter) {
    if (nextFilter === "assigned") {
      router.push(`/${locale}/tasks?filter=assigned`);
    } else {
      router.push(`/${locale}/tasks`);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 rounded-xl border border-border bg-background p-5 shadow-sm">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="space-y-1">
            <h1 className="font-heading text-xl text-foreground">{t("tasks:list.title")}</h1>
            <p className="text-sm text-muted-foreground">{t("tasks:list.subtitle")}</p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <div className="flex items-center gap-1 rounded-md border border-border bg-muted p-1">
              <Button
                size="sm"
                variant={currentFilter === "created" ? "primary" : "ghost"}
                onClick={() => handleFilterToggle("created")}
                icon={<UserIcon />}
              >
                {t("tasks:list.filters.createdByMe")}
              </Button>
              <Button
                size="sm"
                variant={currentFilter === "assigned" ? "primary" : "ghost"}
                onClick={() => handleFilterToggle("assigned")}
                icon={<UserGroupIcon />}
              >
                {t("tasks:list.filters.assignedToMe")}
              </Button>
            </div>
            <Button onClick={() => router.push(`/${locale}/tasks/create`)} icon={<PlusIcon />}>
              {t("common:actions.create")}
            </Button>
            <Button variant="outline" onClick={() => refetch()} icon={<ArrowPathIcon />}>
              {t("common:actions.refresh")}
            </Button>
          </div>
        </div>

        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <div className="flex flex-col gap-2">
            <label htmlFor="status-filter" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.status")}
            </label>
            <Select
              id="status-filter"
              options={[
                { value: "", label: t("common:forms.select") },
                ...STATUS_OPTIONS.map((option) => ({
                  value: option,
                  label: t(`common:taskStatus.${option.charAt(0).toLowerCase()}${option.slice(1)}`)
                }))
              ]}
              value={filters.status ?? ""}
              onChange={(value) =>
                handleFilterChange("status", value ? (value as TaskStatus) : undefined)
              }
            />
          </div>

          <div className="flex flex-col gap-2">
            <label htmlFor="priority-filter" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.priority")}
            </label>
            <Select
              id="priority-filter"
              options={[
                { value: "", label: t("common:forms.select") },
                ...PRIORITY_OPTIONS.map((option) => ({
                  value: option,
                  label: t(`common:priority.${option.toLowerCase()}`)
                }))
              ]}
              value={filters.priority ?? ""}
              onChange={(value) =>
                handleFilterChange("priority", value ? (value as TaskPriority) : undefined)
              }
            />
          </div>

          <div className="flex flex-col gap-2">
            <label htmlFor="dueDateFrom" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.dueDate")} — {t("common:date.today")}
            </label>
            <DatePicker
              id="dueDateFrom"
              value={filters.dueDateFrom ?? ""}
              onChange={(value) => handleFilterChange("dueDateFrom", value || undefined)}
              placeholder={t("common:filters.dueDate")}
            />
          </div>
          <div className="flex flex-col gap-2">
            <label htmlFor="dueDateTo" className="text-xs font-medium text-muted-foreground">
              {t("common:filters.dueDate")} — {t("common:date.overdueBy", { count: 0 })}
            </label>
            <DatePicker
              id="dueDateTo"
              value={filters.dueDateTo ?? ""}
              onChange={(value) => handleFilterChange("dueDateTo", value || undefined)}
              placeholder={t("common:filters.dueDate")}
            />
          </div>
        </div>
      </div>

      <TasksTable
        data={data?.tasks ?? []}
        isLoading={isLoading}
        selectedTaskId={selectedTaskId}
        onSelectTask={setSelectedTaskId}
        onRowClick={(task) => {
          router.push(`/${locale}/tasks/${task.id}`);
        }}
      />

      <div className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-background px-4 py-3 shadow-sm">
        <div className="flex items-center gap-2">
          <span className="text-sm text-muted-foreground">{paginationLabel}</span>
        </div>
        <div className="flex items-center gap-3">
          <Select<number>
            options={PAGE_SIZE_OPTIONS.map((size) => ({
              value: size,
              label: t("common:forms.perPage", { count: size })
            }))}
            value={filters.pageSize ?? PAGE_SIZE_OPTIONS[0]}
            onChange={(value) => handleFilterChange("pageSize", value)}
            className="min-w-[120px]"
          />
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={currentPage <= 1}
              onClick={() => handleFilterChange("page", Math.max(1, currentPage - 1))}
              icon={<ArrowLeftIcon />}
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
              icon={<ArrowRightIcon />}
              iconPosition="right"
            >
              {t("common:actions.next")}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}

