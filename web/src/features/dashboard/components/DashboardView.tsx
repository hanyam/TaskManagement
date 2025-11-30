"use client";

import {
  ArrowPathIcon,
  CheckCircleIcon,
  ClockIcon,
  ExclamationCircleIcon,
  DocumentPlusIcon,
  EyeIcon,
  UserIcon,
  PencilIcon,
  CheckIcon,
  XMarkIcon
} from "@heroicons/react/24/outline";
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent
} from "@dnd-kit/core";
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  rectSortingStrategy
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { useEffect, useState, useMemo } from "react";
import { useRouter } from "next/navigation";
import { useTranslation } from "react-i18next";

import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import { useDashboardStatsQuery } from "@/features/tasks/api/queries";
import { Button } from "@/ui/components/Button";
import { Spinner } from "@/ui/components/Spinner";
import { cn } from "@/ui/utils/cn";

const DASHBOARD_CARDS_STORAGE_KEY = "dashboard-cards-order";

type CardId = "created" | "completed" | "nearDue" | "delayed" | "inProgress" | "underReview" | "pendingAcceptance";

interface CardConfig {
  id: CardId;
  titleKey: string;
  descriptionKey: string;
  value: number;
  icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
  iconColor: string;
  iconBgColor: string;
  onClick: () => void;
}

const defaultCardOrder: CardId[] = [
  "created",
  "completed",
  "nearDue",
  "delayed",
  "inProgress",
  "underReview",
  "pendingAcceptance"
];

export function DashboardView() {
  const { t } = useTranslation(["dashboard", "common"]);
  const { data, isLoading, error, refetch } = useDashboardStatsQuery();
  const router = useRouter();
  const locale = useCurrentLocale();
  const [isEditMode, setIsEditMode] = useState(false);
  const [cardOrder, setCardOrder] = useState<CardId[]>(defaultCardOrder);

  // Load saved order from localStorage on mount
  useEffect(() => {
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem(DASHBOARD_CARDS_STORAGE_KEY);
      if (saved) {
        try {
          const parsed = JSON.parse(saved) as CardId[];
          // Validate that all cards are present
          if (Array.isArray(parsed) && parsed.length === defaultCardOrder.length) {
            const isValid = defaultCardOrder.every((id) => parsed.includes(id));
            if (isValid) {
              setCardOrder(parsed);
            }
          }
        } catch {
          // Invalid JSON, use default order
        }
      }
    }
  }, []);

  // Save order to localStorage when edit mode is turned off
  const handleSaveOrder = () => {
    if (typeof window !== "undefined") {
      localStorage.setItem(DASHBOARD_CARDS_STORAGE_KEY, JSON.stringify(cardOrder));
    }
    setIsEditMode(false);
  };

  // Cancel edit mode and restore saved order
  const handleCancelEdit = () => {
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem(DASHBOARD_CARDS_STORAGE_KEY);
      if (saved) {
        try {
          const parsed = JSON.parse(saved) as CardId[];
          if (Array.isArray(parsed) && parsed.length === defaultCardOrder.length) {
            const isValid = defaultCardOrder.every((id) => parsed.includes(id));
            if (isValid) {
              setCardOrder(parsed);
            }
          }
        } catch {
          // Invalid JSON, use default
        }
      } else {
        setCardOrder([...defaultCardOrder]);
      }
    }
    setIsEditMode(false);
  };

  // Configure drag sensors
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8 // Require 8px of movement before drag starts
      }
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates
    })
  );

  // Handle drag end
  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && active.id !== over.id) {
      setCardOrder((items) => {
        const oldIndex = items.indexOf(active.id as CardId);
        const newIndex = items.indexOf(over.id as CardId);
        return arrayMove(items, oldIndex, newIndex);
      });
    }
  };

  // Build card configurations
  const cardConfigs = useMemo<Record<CardId, CardConfig>>(() => {
    if (!data) {
      return {} as Record<CardId, CardConfig>;
    }

    return {
      created: {
        id: "created",
        titleKey: t("dashboard:widgets.created.title"),
        descriptionKey: t("dashboard:widgets.created.description"),
        value: data.tasksCreatedByUser ?? 0,
        icon: DocumentPlusIcon,
        iconColor: "text-blue-600 dark:text-blue-400",
        iconBgColor: "bg-blue-100 dark:bg-blue-900/30",
        onClick: () => router.push(`/${locale}/tasks?filter=created`)
      },
      completed: {
        id: "completed",
        titleKey: t("dashboard:widgets.completed.title"),
        descriptionKey: t("dashboard:widgets.completed.description"),
        value: data.tasksCompleted ?? 0,
        icon: CheckCircleIcon,
        iconColor: "text-emerald-600 dark:text-emerald-400",
        iconBgColor: "bg-emerald-100 dark:bg-emerald-900/30",
        onClick: () => router.push(`/${locale}/tasks?status=Accepted`)
      },
      nearDue: {
        id: "nearDue",
        titleKey: t("dashboard:widgets.nearDue.title"),
        descriptionKey: t("dashboard:widgets.nearDue.description"),
        value: data.tasksNearDueDate ?? 0,
        icon: ClockIcon,
        iconColor: "text-amber-600 dark:text-amber-400",
        iconBgColor: "bg-amber-100 dark:bg-amber-900/30",
        onClick: () => router.push(`/${locale}/tasks`)
      },
      delayed: {
        id: "delayed",
        titleKey: t("dashboard:widgets.delayed.title"),
        descriptionKey: t("dashboard:widgets.delayed.description"),
        value: data.tasksDelayed ?? 0,
        icon: ExclamationCircleIcon,
        iconColor: "text-rose-600 dark:text-rose-400",
        iconBgColor: "bg-rose-100 dark:bg-rose-900/30",
        onClick: () => router.push(`/${locale}/tasks`)
      },
      inProgress: {
        id: "inProgress",
        titleKey: t("dashboard:widgets.inProgress.title"),
        descriptionKey: t("dashboard:widgets.inProgress.description"),
        value: data.tasksInProgress ?? 0,
        icon: ArrowPathIcon,
        iconColor: "text-sky-600 dark:text-sky-400",
        iconBgColor: "bg-sky-100 dark:bg-sky-900/30",
        onClick: () => router.push(`/${locale}/tasks?status=Assigned`)
      },
      underReview: {
        id: "underReview",
        titleKey: t("dashboard:widgets.underReview.title"),
        descriptionKey: t("dashboard:widgets.underReview.description"),
        value: data.tasksUnderReview ?? 0,
        icon: EyeIcon,
        iconColor: "text-purple-600 dark:text-purple-400",
        iconBgColor: "bg-purple-100 dark:bg-purple-900/30",
        onClick: () => router.push(`/${locale}/tasks?status=PendingManagerReview`)
      },
      pendingAcceptance: {
        id: "pendingAcceptance",
        titleKey: t("dashboard:widgets.pendingAcceptance.title"),
        descriptionKey: t("dashboard:widgets.pendingAcceptance.description"),
        value: data.tasksPendingAcceptance ?? 0,
        icon: UserIcon,
        iconColor: "text-indigo-600 dark:text-indigo-400",
        iconBgColor: "bg-indigo-100 dark:bg-indigo-900/30",
        onClick: () => router.push(`/${locale}/tasks?status=UnderReview`)
      }
    };
  }, [data, t, locale, router]);

  // Get ordered cards
  const orderedCards = useMemo(() => {
    return cardOrder.map((id) => cardConfigs[id]).filter(Boolean);
  }, [cardOrder, cardConfigs]);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex flex-col gap-2">
          <h1 className="font-heading text-2xl text-foreground">{t("dashboard:title")}</h1>
          <p className="text-sm text-muted-foreground">{t("dashboard:subtitle")}</p>
        </div>
        {!isLoading && !error && (
          <div className="flex items-center gap-2">
            {isEditMode ? (
              <>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleCancelEdit}
                  icon={<XMarkIcon />}
                >
                  {t("common:actions.cancel")}
                </Button>
                <Button
                  variant="primary"
                  size="sm"
                  onClick={handleSaveOrder}
                  icon={<CheckIcon />}
                >
                  {t("common:actions.save")}
                </Button>
              </>
            ) : (
              <Button
                variant="outline"
                size="sm"
                onClick={() => setIsEditMode(true)}
                icon={<PencilIcon />}
              >
                {t("dashboard:actions.reorder")}
              </Button>
            )}
          </div>
        )}
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
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <SortableContext
            items={cardOrder}
            strategy={rectSortingStrategy}
          >
            <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
              {orderedCards.map((config) => (
                <SortableStatCard
                  key={config.id}
                  id={config.id}
                  title={config.titleKey}
                  description={config.descriptionKey}
                  value={config.value}
                  icon={config.icon}
                  iconColor={config.iconColor}
                  iconBgColor={config.iconBgColor}
                  isEditMode={isEditMode}
                  onClick={isEditMode ? undefined : config.onClick}
                  dragToReorderText={t("dashboard:actions.dragToReorder")}
                />
              ))}
            </div>
          </SortableContext>
        </DndContext>
      )}
    </div>
  );
}

interface SortableStatCardProps {
  id: CardId;
  title: string;
  description: string;
  value: number;
  icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
  iconColor: string;
  iconBgColor: string;
  isEditMode: boolean;
  onClick?: (() => void) | undefined;
  dragToReorderText: string;
}

function SortableStatCard({
  id,
  title,
  description,
  value,
  icon: Icon,
  iconColor,
  iconBgColor,
  isEditMode,
  onClick,
  dragToReorderText
}: SortableStatCardProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging
  } = useSortable({ id, disabled: !isEditMode });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1
  };

  // Extract role and tabIndex from attributes if in edit mode to avoid conflicts
  const { role: _role, tabIndex: _tabIndex, ...dragAttributes } = attributes;

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={cn(
        "group relative overflow-hidden rounded-xl border border-border bg-background p-6 shadow-sm transition-all duration-200",
        isEditMode && "cursor-grab active:cursor-grabbing",
        !isEditMode && onClick && "cursor-pointer hover:border-primary/60 hover:shadow-md",
        isDragging && "z-50 scale-105 shadow-lg ring-2 ring-primary/20"
      )}
      onClick={!isEditMode ? onClick : undefined}
      role={!isEditMode && onClick ? "button" : isEditMode ? undefined : undefined}
      tabIndex={!isEditMode && onClick ? 0 : isEditMode ? -1 : undefined}
      onKeyDown={
        !isEditMode && onClick
          ? (e) => {
              if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                onClick();
              }
            }
          : undefined
      }
      {...(isEditMode ? { ...dragAttributes, ...listeners } : {})}
    >
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">{description}</p>
          <p className="mt-2 text-3xl font-bold text-foreground">{value}</p>
          <p className="mt-1 text-sm font-medium text-foreground">{title}</p>
        </div>
        <div className={cn(
          "flex h-12 w-12 shrink-0 items-center justify-center rounded-lg transition-transform duration-200",
          iconBgColor,
          !isEditMode && onClick && "group-hover:scale-110",
          isEditMode && "cursor-grab"
        )}>
          <Icon className={cn("h-6 w-6", iconColor)} />
        </div>
      </div>
      {isEditMode && (
        <div className="absolute inset-0 flex items-center justify-center bg-background/80 backdrop-blur-sm opacity-0 transition-opacity duration-200 group-hover:opacity-100">
          <div className="flex items-center gap-2 rounded-lg bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary">
            <svg
              className="h-4 w-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M7 16V4m0 0L3 8m4-4l4 4m6 0v12m0 0l4-4m-4 4l-4-4"
              />
            </svg>
            {dragToReorderText}
          </div>
        </div>
      )}
    </div>
  );
}

