import { createColumnHelper, flexRender, getCoreRowModel, useReactTable } from "@tanstack/react-table";
import { useVirtualizer } from "@tanstack/react-virtual";
import Link from "next/link";
import { useMemo, useRef } from "react";
import { useTranslation } from "react-i18next";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { TaskStatusBadge } from "@/features/tasks/components/TaskStatusBadge";
import type { TaskDto } from "@/features/tasks/types";

const columnHelper = createColumnHelper<TaskDto>();

interface TasksTableProps {
  data: TaskDto[];
  isLoading?: boolean;
  onRowClick?: (task: TaskDto) => void;
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

export function TasksTable({ data, isLoading, onRowClick }: TasksTableProps) {
  const { t } = useTranslation(["tasks", "common"]);
  const locale = useCurrentLocale();
  const containerRef = useRef<HTMLDivElement | null>(null);

  const columns = useMemo(
    () => [
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
        cell: (info) => <TaskStatusBadge status={info.getValue()} />
      }),
      columnHelper.accessor("priority", {
        header: () => t("tasks:list.table.columns.priority"),
        cell: (info) => t(`common:priority.${info.getValue().toLowerCase()}`)
      }),
      columnHelper.accessor("dueDate", {
        header: () => t("tasks:list.table.columns.dueDate"),
        cell: (info) => formatDate(info.getValue(), locale)
      }),
      columnHelper.accessor("reminderLevel", {
        header: () => t("tasks:list.table.columns.reminderLevel"),
        cell: (info) => t(`common:reminderLevel.${info.getValue().toLowerCase()}`)
      }),
      columnHelper.display({
        id: "actions",
        header: () => t("tasks:list.table.columns.actions"),
        cell: (info) => (
          <Link
            href={`/${locale}/tasks/${info.row.original.id}`}
            className="text-sm font-medium text-primary hover:underline"
          >
            {t("common:actions.viewDetails")}
          </Link>
        )
      })
    ],
    [locale, t]
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
            const handleActivate = () => {
              if (onRowClick) {
                onRowClick(row.original);
              }
            };

            return (
              <div
                key={row.id}
                data-index={virtualRow.index}
                role="row"
                className="absolute inset-x-0 grid cursor-pointer border-b border-border bg-background px-4 py-4 text-sm transition-colors hover:bg-secondary/40"
                style={{
                  transform: `translateY(${virtualRow.start}px)`,
                  gridTemplateColumns: columnTemplate
                }}
                tabIndex={onRowClick ? 0 : -1}
                onClick={handleActivate}
                onKeyDown={(event) => {
                  if (event.key === "Enter" || event.key === " ") {
                    event.preventDefault();
                    handleActivate();
                  }
                }}
              >
                {row.getVisibleCells().map((cell) => {
                  const renderCell = cell.column.columnDef.cell;
                  return (
                    <div key={cell.id} className="flex items-center" role="gridcell">
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

