"use client";

import { useTranslation } from "react-i18next";

import { useDashboardStatsQuery } from "@/features/tasks/api/queries";
import { Button } from "@/ui/components/Button";
import { Spinner } from "@/ui/components/Spinner";

export function DashboardView() {
  const { t } = useTranslation(["dashboard", "common"]);
  const { data, isLoading, error, refetch } = useDashboardStatsQuery();

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
          <Button variant="outline" className="mt-4" onClick={() => refetch()}>
            {t("common:actions.retry")}
          </Button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <StatCard
            title={t("dashboard:widgets.created.title")}
            description={t("dashboard:widgets.created.description")}
            value={data?.tasksCreatedByUser ?? 0}
          />
          <StatCard
            title={t("dashboard:widgets.completed.title")}
            description={t("dashboard:widgets.completed.description")}
            value={data?.tasksCompleted ?? 0}
          />
          <StatCard
            title={t("dashboard:widgets.nearDue.title")}
            description={t("dashboard:widgets.nearDue.description")}
            value={data?.tasksNearDueDate ?? 0}
          />
          <StatCard
            title={t("dashboard:widgets.delayed.title")}
            description={t("dashboard:widgets.delayed.description")}
            value={data?.tasksDelayed ?? 0}
          />
          <StatCard
            title={t("dashboard:widgets.inProgress.title")}
            description={t("dashboard:widgets.inProgress.description")}
            value={data?.tasksInProgress ?? 0}
          />
          <StatCard
            title={t("dashboard:widgets.underReview.title")}
            description={t("dashboard:widgets.underReview.description")}
            value={data?.tasksUnderReview ?? 0}
          />
          <StatCard
            title={t("dashboard:widgets.pendingAcceptance.title")}
            description={t("dashboard:widgets.pendingAcceptance.description")}
            value={data?.tasksPendingAcceptance ?? 0}
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
}

function StatCard({ title, description, value }: StatCardProps) {
  return (
    <div className="rounded-xl border border-border bg-background p-5 shadow-sm transition hover:border-primary/60">
      <div className="text-sm font-medium text-muted-foreground">{description}</div>
      <div className="mt-3 text-3xl font-bold text-foreground">{value}</div>
      <div className="mt-1 text-sm text-muted-foreground">{title}</div>
    </div>
  );
}

