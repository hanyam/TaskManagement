"use client";

import { ArrowPathIcon, StarIcon } from "@heroicons/react/24/outline";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { debugLog, debugWarn, debugGroup, debugComponent } from "@/core/debug/logger";

import {
  useAcceptTaskMutation,
  useRejectTaskMutation,
  useMarkTaskCompletedMutation,
  downloadAttachment
} from "@/features/tasks/api/queries";
import { FileAttachmentList } from "@/features/tasks/components/FileAttachmentList";
import { TaskStatusBadge } from "@/features/tasks/components/TaskStatusBadge";
import { TaskMetadata } from "@/features/tasks/components/TaskMetadata";
import { UpdateProgressDialog } from "@/features/tasks/components/dialogs/UpdateProgressDialog";
import { RequestExtensionDialog } from "@/features/tasks/components/dialogs/RequestExtensionDialog";
import { RequestMoreInfoDialog } from "@/features/tasks/components/dialogs/RequestMoreInfoDialog";
import { useTaskDetails } from "@/features/tasks/hooks/useTaskDetails";
import { getTaskPriorityString } from "@/features/tasks/value-objects";
import { AttachmentTypeEnum } from "@/features/tasks/value-objects";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { formatDate } from "@/features/tasks/utils/dateFormatting";
import { Button } from "@/ui/components/Button";
import { FileUpload } from "@/ui/components/FileUpload";
import { Spinner } from "@/ui/components/Spinner";

interface TaskDetailsViewEmployeeProps {
  taskId: string;
}

export function TaskDetailsViewEmployee({ taskId }: TaskDetailsViewEmployeeProps) {
  // Direct console.log to verify component is rendering (always shows)
  // eslint-disable-next-line no-console
  console.log("🔴 TaskDetailsViewEmployee RENDERING", { taskId });
  
  debugComponent("TaskDetailsViewEmployee", "mount", { taskId });
  const { t } = useTranslation(["tasks", "common"]);
  const router = useRouter();

  const {
    task,
    links,
    attachments,
    isLoading,
    error,
    isMounted,
    canUpload,
    uploadFiles,
    setUploadFiles,
    refetch,
    refetchAttachments,
    uploadMutation
  } = useTaskDetails(taskId);

  // Debug: Log response structure
  useEffect(() => {
    debugGroup("TaskDetailsViewEmployee - Data", () => {
      debugLog("Task loaded", { taskId: task?.id, status: task?.status, title: task?.title });
      debugLog("Links received", {
        links,
        linksType: typeof links,
        linksIsArray: Array.isArray(links),
        linksLength: links?.length,
        linkRels: links?.map((l) => l.rel) || []
      });
      debugLog("Attachments", { count: attachments?.length || 0 });
      debugLog("State", { isLoading, error: error?.message, isMounted, canUpload });
    });
  }, [task, links, attachments, isLoading, error, isMounted, canUpload]);

  const acceptMutation = useAcceptTaskMutation(taskId);
  const rejectMutation = useRejectTaskMutation(taskId);
  const markCompleteMutation = useMarkTaskCompletedMutation(taskId);

  const [isProgressOpen, setProgressOpen] = useState(false);
  const [isExtensionOpen, setExtensionOpen] = useState(false);
  const [isMoreInfoOpen, setMoreInfoOpen] = useState(false);

  const hasLink = (rel: string) => {
    if (!links || !Array.isArray(links)) {
      debugWarn("Links is not an array", { links, type: typeof links });
      return false;
    }
    const found = links.some((link) => link.rel === rel);
    debugLog(`Checking link: ${rel}`, {
      found,
      linksCount: links.length,
      linkRels: links.map((l) => l.rel),
      allLinks: links
    });
    return found;
  };
  const isFinalReviewedState = task?.status === 3 && task?.managerRating != null;
  const isCancelled = task?.status === 8;

  // Immediate debug log to verify component is rendering
  // eslint-disable-next-line no-console
  console.log("🔴 TaskDetailsViewEmployee render check", {
    taskId,
    hasTask: !!task,
    hasLinks: !!links,
    linksCount: links?.length || 0,
    taskStatus: task?.status,
    isCancelled
  });
  
  debugLog("TaskDetailsViewEmployee render check", {
    taskId,
    hasTask: !!task,
    hasLinks: !!links,
    linksCount: links?.length || 0,
    taskStatus: task?.status,
    isCancelled
  });

  // Debug: Log mark-completed button state whenever relevant data changes
  useEffect(() => {
    debugLog("🔍 Mark Completed Button useEffect triggered", {
      hasTask: !!task,
      hasLinks: !!links,
      linksIsArray: Array.isArray(links),
      taskId: task?.id,
      taskStatus: task?.status,
      linksCount: links?.length || 0
    });

    if (task && links && Array.isArray(links)) {
      const hasMarkCompletedLink = links.some((link) => link.rel === "mark-completed");
      const shouldShow = hasMarkCompletedLink && !isCancelled;
      
      debugGroup("Mark Completed Button Render Check", () => {
        debugLog("Link Check Result", {
          hasMarkCompletedLink,
          isCancelled,
          shouldShow,
          taskStatus: task?.status,
          managerRating: task?.managerRating,
          linksArray: links,
          linksCount: links?.length || 0,
          markCompletedLink: links.find((l) => l.rel === "mark-completed"),
          allLinkRels: links.map((l) => l.rel)
        });
        if (shouldShow) {
          debugLog("✅ SHOULD Render Mark Completed Button");
        } else {
          debugLog("❌ SHOULD NOT Render Mark Completed Button", {
            reason: !hasMarkCompletedLink ? "Link not found" : isCancelled ? "Task is cancelled" : "Unknown"
          });
        }
      });
    } else {
      debugWarn("Mark Completed Button useEffect: Missing data", {
        hasTask: !!task,
        hasLinks: !!links,
        linksIsArray: Array.isArray(links)
      });
    }
  }, [task, links, isCancelled]);

  async function handleAcceptTask() {
    try {
      await acceptMutation.mutateAsync();
      toast.success(t("tasks:details.actions.accept"));
      await refetch();
      await refetchAttachments();
      router.refresh();
    } catch (error) {
      displayApiError(error, t("validation:server.INTERNAL_ERROR"));
    }
  }

  async function handleRejectTask() {
    try {
      await rejectMutation.mutateAsync({ reason: "" });
      toast.success(t("tasks:details.actions.reject"));
      refetch();
    } catch (error) {
      displayApiError(error, t("validation:server.INTERNAL_ERROR"));
    }
  }

  async function handleMarkCompleted() {
    try {
      await markCompleteMutation.mutateAsync();
      toast.success(t("tasks:details.actions.markCompleted"));
      router.refresh();
    } catch (error) {
      displayApiError(error, t("validation:server.INTERNAL_ERROR"));
    }
  }

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error || !task) {
    return (
      <div className="rounded-xl border border-border bg-background p-6 text-center">
        <p className="text-sm text-destructive">{t("common:states.error")}</p>
        <Button variant="outline" className="mt-4" onClick={() => refetch()} icon={<ArrowPathIcon />}>
          {t("common:actions.retry")}
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-col gap-4 rounded-xl border border-border bg-background p-6 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex flex-col gap-2 flex-1">
            <div className="flex items-center gap-3">
              <h1 className="font-heading text-2xl text-foreground">{task.title}</h1>
              <TaskStatusBadge status={task.status} managerRating={task.managerRating ?? null} />
            </div>
            <div className="flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
              <span>
                {t("common:filters.priority")}: {t(`common:priority.${getTaskPriorityString(task.priority).toLowerCase()}`)}
              </span>
              <span>
                {t("common:filters.assignedTo")}: {task.assignedUserEmail ?? "—"}
              </span>
              <span>
                {t("common:filters.dueDate")}: {task.dueDate ? formatDate(task.dueDate) : t("common:date.noDueDate")}
              </span>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            {hasLink("accept") && !isFinalReviewedState && !isCancelled && (
              <Button variant="primary" onClick={handleAcceptTask} disabled={acceptMutation.isPending}>
                {t("tasks:details.actions.accept")}
              </Button>
            )}

            {hasLink("reject") && !isFinalReviewedState && !isCancelled && (
              <Button variant="outline" onClick={handleRejectTask} disabled={rejectMutation.isPending}>
                {t("tasks:details.actions.reject")}
              </Button>
            )}

            {hasLink("update-progress") && !isFinalReviewedState && !isCancelled && (
              <Button variant="secondary" onClick={() => setProgressOpen(true)}>
                {t("tasks:details.actions.updateProgress")}
              </Button>
            )}

            {(() => {
              const hasMarkCompletedLink = hasLink("mark-completed");
              const shouldShow = hasMarkCompletedLink && !isCancelled;
              // Debug logging is now in useEffect above - this just renders the button
              return shouldShow ? (
                <Button variant="destructive" onClick={handleMarkCompleted} disabled={markCompleteMutation.isPending}>
                  {t("tasks:details.actions.markCompleted")}
                </Button>
              ) : null;
            })()}

            {hasLink("request-extension") && !isFinalReviewedState && !isCancelled && (
              <Button variant="outline" onClick={() => setExtensionOpen(true)}>
                {t("tasks:details.actions.requestExtension")}
              </Button>
            )}

            {hasLink("request-more-info") && !isFinalReviewedState && !isCancelled && (
              <Button variant="outline" onClick={() => setMoreInfoOpen(true)}>
                {t("tasks:details.actions.requestInfo")}
              </Button>
            )}
          </div>
        </div>
      </header>

      <div className="grid gap-6 lg:grid-cols-3">
        <section className="space-y-4 rounded-xl border border-border bg-background p-6 shadow-sm lg:col-span-2">
          <h2 className="text-lg font-semibold text-foreground">{t("tasks:details.sections.description")}</h2>
          <p className="text-sm text-muted-foreground">{task.description ?? t("common:states.empty")}</p>

          {/* Manager Review Section */}
          {(task.managerRating != null || task.managerFeedback) && (
            <div className="space-y-3 rounded-lg border border-border bg-muted/20 p-4">
              <h3 className="text-base font-semibold text-foreground">{t("tasks:details.sections.managerReview")}</h3>
              {task.managerRating != null && (
                <div className="flex items-center gap-2">
                  <span className="text-sm text-muted-foreground">{t("tasks:details.managerReview.rating")}:</span>
                  <div className="flex items-center gap-1">
                    {[1, 2, 3, 4, 5].map((star) => (
                      <StarIcon
                        key={star}
                        className={`h-5 w-5 ${
                          star <= task.managerRating!
                            ? "text-warning fill-warning"
                            : "text-muted-foreground"
                        }`}
                      />
                    ))}
                    <span className="ml-1 text-sm font-medium text-foreground">{task.managerRating}/5</span>
                  </div>
                </div>
              )}
              {task.managerFeedback && (
                <div>
                  <span className="text-sm text-muted-foreground">{t("tasks:details.managerReview.feedback")}:</span>
                  <p className="mt-1 text-sm text-foreground">{task.managerFeedback}</p>
                </div>
              )}
            </div>
          )}

          <h3 className="text-base font-semibold text-foreground">{t("tasks:details.sections.progressHistory")}</h3>
          <div className="space-y-3 rounded-lg border border-border bg-muted/20 p-3">
            {task.recentProgressHistory?.length ? (
              task.recentProgressHistory.map((progress) => (
                <div key={progress.id} className="flex flex-col gap-1 rounded-md border border-border/80 bg-background px-3 py-2">
                  <div className="flex justify-between text-xs text-muted-foreground">
                    <span>{formatDate(progress.updatedAt)}</span>
                    <span>{progress.updatedByEmail ?? progress.updatedById}</span>
                  </div>
                  <div className="text-sm text-foreground">
                    {progress.progressPercentage}% — {progress.notes ?? t("common:states.empty")}
                  </div>
                </div>
              ))
            ) : (
              <p className="text-sm text-muted-foreground">{t("common:states.empty")}</p>
            )}
          </div>

          {/* Attachments Section */}
          <div className="space-y-4">
            <h3 className="text-base font-semibold text-foreground">{t("tasks:details.sections.attachments")}</h3>

            {isMounted && attachments.length > 0 && (
              <FileAttachmentList
                attachments={attachments}
                onDownload={async (attachmentId) => {
                  try {
                    const blob = await downloadAttachment(taskId, attachmentId);
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement("a");
                    a.href = url;
                    const attachment = attachments.find((a) => a.id === attachmentId);
                    a.download = attachment?.originalFileName || "download";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);
                  } catch (error) {
                    toast.error(t("tasks:attachments.actions.download"));
                  }
                }}
              />
            )}

            {isMounted && canUpload && (
              <div className="space-y-2">
                <FileUpload
                  files={uploadFiles}
                  onFilesChange={setUploadFiles}
                  maxSize={50 * 1024 * 1024}
                />
                {uploadFiles.length > 0 && (
                  <Button
                    type="button"
                    onClick={async () => {
                      for (const fileItem of uploadFiles) {
                        if (!fileItem.error) {
                          try {
                            await uploadMutation.mutateAsync({
                              file: fileItem.file,
                              type: AttachmentTypeEnum.EmployeeUploaded
                            });
                            toast.success(t("tasks:attachments.upload.success"));
                          } catch (error) {
                            displayApiError(error, t("tasks:attachments.upload.error"));
                          }
                        }
                      }
                      setUploadFiles([]);
                      await refetchAttachments();
                    }}
                    disabled={uploadMutation.isPending}
                  >
                    {t("tasks:attachments.upload.title")}
                  </Button>
                )}
              </div>
            )}

            {isMounted && attachments.length === 0 && !canUpload && (
              <p className="text-sm text-muted-foreground">{t("tasks:attachments.empty")}</p>
            )}
          </div>
        </section>

        <TaskMetadata task={task} />
      </div>

      {/* Dialogs */}
      <UpdateProgressDialog open={isProgressOpen} onOpenChange={setProgressOpen} taskId={task.id} />
      <RequestExtensionDialog open={isExtensionOpen} onOpenChange={setExtensionOpen} taskId={task.id} />
      <RequestMoreInfoDialog open={isMoreInfoOpen} onOpenChange={setMoreInfoOpen} taskId={task.id} />
    </div>
  );
}

