"use client";

import { ArrowPathIcon } from "@heroicons/react/24/outline";
import { useRouter } from "next/navigation";
import { useTranslation } from "react-i18next";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { useDashboardStatsQuery } from "@/features/tasks/api/queries";
import { Button } from "@/ui/components/Button";
import { Spinner } from "@/ui/components/Spinner";

export function DashboardView() {
  const { t } = useTranslation(["dashboard", "common"]);
  const { data, isLoading, error, refetch } = useDashboardStatsQuery();
  const router = useRouter();
  const locale = useCurrentLocale();

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-2">
        <h1 className="font-heading text-2xl text-foreground">{t("dashboard:title")}</h1>
        <p className="text-sm text-muted-foreground">{t("dashboard:subtitle")}</p>
      </div>
      {isLoading ? (
        <div className="flex h-60 items-center justify-center">
          <Spinner size="lg" />
        </div>
      ) : error ? (
        <div className="rounded-xl border border-border bg-background p-6 text-center">
          <p className="text-sm text-destructive">{t("common:states.error")}</p>
          <Button variant="outline" className="mt-4" onClick={() => refetch()} icon={<ArrowPathIcon />}>
            {t("common:actions.retry")}
          </Button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <StatCard
            title={t("dashboard:widgets.created.title")}
            description={t("dashboard:widgets.created.description")}
            value={data?.tasksCreatedByUser ?? 0}
            onClick={() => router.push(`/${locale}/tasks?filter=created`)}
          />
          <StatCard
            title={t("dashboard:widgets.completed.title")}
            description={t("dashboard:widgets.completed.description")}
            value={data?.tasksCompleted ?? 0}
            onClick={() => router.push(`/${locale}/tasks?status=Accepted`)}
          />
          <StatCard
            title={t("dashboard:widgets.nearDue.title")}
            description={t("dashboard:widgets.nearDue.description")}
            value={data?.tasksNearDueDate ?? 0}
            onClick={() => router.push(`/${locale}/tasks`)}
          />
          <StatCard
            title={t("dashboard:widgets.delayed.title")}
            description={t("dashboard:widgets.delayed.description")}
            value={data?.tasksDelayed ?? 0}
            onClick={() => router.push(`/${locale}/tasks`)}
          />
          <StatCard
            title={t("dashboard:widgets.inProgress.title")}
            description={t("dashboard:widgets.inProgress.description")}
            value={data?.tasksInProgress ?? 0}
            onClick={() => router.push(`/${locale}/tasks?status=Assigned`)}
          />
          <StatCard
            title={t("dashboard:widgets.underReview.title")}
            description={t("dashboard:widgets.underReview.description")}
            value={data?.tasksUnderReview ?? 0}
            onClick={() => router.push(`/${locale}/tasks?status=PendingManagerReview`)}
          />
          <StatCard
            title={t("dashboard:widgets.pendingAcceptance.title")}
            description={t("dashboard:widgets.pendingAcceptance.description")}
            value={data?.tasksPendingAcceptance ?? 0}
            onClick={() => router.push(`/${locale}/tasks?status=UnderReview`)}
          />
        </div>
      )}
    </div>
  );
}

interface StatCardProps {
  title: string;
  description: string;
  value: number;
  onClick?: () => void;
}

function StatCard({ title, description, value, onClick }: StatCardProps) {
  return (
    <div
      className={`rounded-xl border border-border bg-background p-5 shadow-sm transition hover:border-primary/60 ${
        onClick ? "cursor-pointer" : ""
      }`}
      onClick={onClick}
      role={onClick ? "button" : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={
        onClick
          ? (e) => {
              if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                onClick();
              }
            }
          : undefined
      }
    >
      <div className="text-sm font-medium text-muted-foreground">{description}</div>
      <div className="mt-3 text-3xl font-bold text-foreground">{value}</div>
      <div className="mt-1 text-sm text-muted-foreground">{title}</div>
    </div>
  );
}

