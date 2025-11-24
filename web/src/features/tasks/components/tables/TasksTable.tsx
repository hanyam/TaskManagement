import { EyeIcon } from "@heroicons/react/24/outline";
import { createColumnHelper, flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import { useVirtualizer } from "@tanstack/react-virtual";
import { useMemo, useRef } from "react";
import { useTranslation } from "react-i18next";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { TaskStatusBadge } from "@/features/tasks/components/TaskStatusBadge";
import type { TaskDto } from "@/features/tasks/types";
import { getTaskPriorityString, getReminderLevelString } from "@/features/tasks/value-objects";
import { Button } from "@/ui/components/Button";
import { cn } from "@/ui/utils/cn";

const columnHelper = createColumnHelper<TaskDto>();

interface TasksTableProps {
  data: TaskDto[];
  isLoading?: boolean;
  onRowClick?: (task: TaskDto) => void;
  selectedTaskId?: string | undefined;
  onSelectTask?: ((taskId: string | undefined) => void) | undefined;
}

function formatDate(value?: string | null, locale?: string): string {
  if (!value) {
    return "—";
  }

  try {
    return new Intl.DateTimeFormat(locale ?? "en", {
      dateStyle: "medium",
      timeZone: "UTC"
    }).format(new Date(value));
  } catch {
    return value;
  }
}

export function TasksTable({
  data,
  isLoading,
  onRowClick,
  selectedTaskId,
  onSelectTask
}: TasksTableProps) {
  const { t } = useTranslation(["tasks", "common"]);
  const locale = useCurrentLocale();
  const containerRef = useRef<HTMLDivElement | null>(null);

  const columns = useMemo(
    () => {
      const cols = [
      columnHelper.accessor("title", {
        header: () => t("tasks:list.table.columns.title"),
        cell: (info) => (
          <div className="flex flex-col">
            <span className="font-medium text-foreground">{info.getValue()}</span>
            <span className="text-xs text-muted-foreground">
              {info.row.original.assignedUserEmail ?? t("tasks:list.table.columns.assignedTo")}
            </span>
          </div>
        )
      }),
      columnHelper.accessor("status", {
        header: () => t("tasks:list.table.columns.status"),
        cell: (info) => (
          <TaskStatusBadge status={info.getValue()} managerRating={info.row.original.managerRating ?? null} />
        )
      }),
      columnHelper.accessor("priority", {
        header: () => t("tasks:list.table.columns.priority"),
        cell: (info) => {
          const priorityString = getTaskPriorityString(info.getValue());
          return t(`common:priority.${priorityString.toLowerCase()}`);
        }
      }),
      columnHelper.accessor("dueDate", {
        header: () => t("tasks:list.table.columns.dueDate"),
        cell: (info) => formatDate(info.getValue(), locale)
      }),
      columnHelper.accessor("reminderLevel", {
        header: () => t("tasks:list.table.columns.reminderLevel"),
        cell: (info) => {
          const reminderString = getReminderLevelString(info.getValue());
          return t(`common:reminderLevel.${reminderString.toLowerCase()}`);
        }
      }),
      columnHelper.display({
        id: "actions",
        header: () => t("tasks:list.table.columns.actions"),
        cell: (info) => (
          <Button
            variant="outline"
            size="sm"
            icon={<EyeIcon />}
            onClick={(e) => {
              e.stopPropagation();
              if (onRowClick) {
                onRowClick(info.row.original);
              }
            }}
          >
            {t("common:actions.viewDetails")}
          </Button>
        )
      })
    ];
      return cols;
    },
    [locale, t, onRowClick]
  );

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel()
  });

  const rows = table.getRowModel().rows;

  const rowVirtualizer = useVirtualizer({
    count: rows.length,
    getScrollElement: () => containerRef.current,
    estimateSize: () => 72,
    overscan: 8
  });

  const virtualRows = rowVirtualizer.getVirtualItems();
  const totalSize = rowVirtualizer.getTotalSize();

  const columnTemplate = "minmax(220px,2.5fr) minmax(160px,1fr) minmax(120px,1fr) minmax(140px,1fr) minmax(150px,1fr) minmax(120px,0.8fr)";

  return (
    <div className="rounded-xl border border-border bg-background shadow-sm">
      <div
        className="sticky top-0 z-10 grid items-center gap-2 border-b border-border bg-muted/60 px-4 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground"
        style={{ gridTemplateColumns: columnTemplate }}
      >
        {table.getHeaderGroups()[0]?.headers.map((header) => (
          <div key={header.id}>
            {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
          </div>
        ))}
      </div>

      <div ref={containerRef} className="max-h-[540px] overflow-auto rounded-b-xl" aria-busy={isLoading}>
        <div style={{ height: `${totalSize}px`, position: "relative" }}>
          {virtualRows.map((virtualRow) => {
            const row = rows[virtualRow.index];
            const isSelected = selectedTaskId === row.original.id;
            const handleRowClick = () => {
              if (onSelectTask) {
                onSelectTask(isSelected ? undefined : row.original.id);
              }
            };

            return (
              <div
                key={row.id}
                data-index={virtualRow.index}
                role="row"
                className={cn(
                  "absolute inset-x-0 grid cursor-pointer border-b border-border px-4 py-4 text-sm transition-colors",
                  isSelected
                    ? "bg-primary/10 hover:bg-primary/15 border-primary/20"
                    : "bg-background hover:bg-secondary/40"
                )}
                style={{
                  transform: `translateY(${virtualRow.start}px)`,
                  gridTemplateColumns: columnTemplate
                }}
                tabIndex={0}
                onClick={handleRowClick}
                onKeyDown={(event) => {
                  if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    handleRowClick();
                  }
                }}
              >
                {row.getVisibleCells().map((cell) => {
                  const renderCell = cell.column.columnDef.cell;
                  const isActionCell = cell.column.id === "actions";
                  return (
                    <div
                      key={cell.id}
                      className="flex items-center"
                      role={isActionCell ? undefined : "gridcell"}
                      onClick={(e) => {
                        // Prevent row selection when clicking on action button
                        if (isActionCell) {
                          e.stopPropagation();
                        }
                      }}
                      onKeyDown={(e) => {
                        // Prevent row selection when interacting with action button
                        if (isActionCell) {
                          e.stopPropagation();
                        }
                      }}
                    >
                      {typeof renderCell === "function" ? renderCell(cell.getContext()) : renderCell ?? null}
                    </div>
                  );
                })}
              </div>
            );
          })}
          {!virtualRows.length && !isLoading ? (
            <div className="absolute inset-x-0 top-0 flex h-40 items-center justify-center text-sm text-muted-foreground">
              {t("tasks:list.table.empty")}
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}

