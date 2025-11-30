"use client";

import { ArrowPathIcon, CheckIcon, PencilIcon, StarIcon, StopIcon, XMarkIcon } from "@heroicons/react/24/outline";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { debugLog, debugComponent, debugGroup } from "@/core/debug/logger";

import {
  useAssignTaskMutation,
  useReassignTaskMutation,
  downloadAttachment
} from "@/features/tasks/api/queries";
import { FileAttachmentList } from "@/features/tasks/components/FileAttachmentList";
import { TaskStatusBadge } from "@/features/tasks/components/TaskStatusBadge";
import { TaskEditForm } from "@/features/tasks/components/TaskEditForm";
import { TaskMetadata } from "@/features/tasks/components/TaskMetadata";
import { ReviewCompletedTaskModal } from "@/features/tasks/components/ReviewCompletedTaskModal";
import { TaskHistoryTimeline } from "@/features/tasks/components/TaskHistoryTimeline";
import { AssignTaskDialog } from "@/features/tasks/components/dialogs/AssignTaskDialog";
import { ApproveExtensionDialog } from "@/features/tasks/components/dialogs/ApproveExtensionDialog";
import { CancelTaskDialog } from "@/features/tasks/components/dialogs/CancelTaskDialog";
import { DeleteAttachmentDialog } from "@/features/tasks/components/dialogs/DeleteAttachmentDialog";
import { useTaskDetails } from "@/features/tasks/hooks/useTaskDetails";
import { getTaskPriorityString } from "@/features/tasks/value-objects";
import { AttachmentTypeEnum } from "@/features/tasks/value-objects";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { formatDate } from "@/features/tasks/utils/dateFormatting";
import { canDeleteAttachment } from "@/features/tasks/utils/attachmentAccess";
import { Button } from "@/ui/components/Button";
import { FileUpload } from "@/ui/components/FileUpload";
import { Input } from "@/ui/components/Input";
import { Spinner } from "@/ui/components/Spinner";

interface TaskDetailsViewManagerProps {
  taskId: string;
}

export function TaskDetailsViewManager({ taskId }: TaskDetailsViewManagerProps) {
  const { t } = useTranslation(["tasks", "common"]);
  
  // Log mount only once using useEffect
  useEffect(() => {
    // eslint-disable-next-line no-console
    console.log("🔴 TaskDetailsViewManager RENDERING", { taskId });
    debugComponent("TaskDetailsViewManager", "mount", { taskId });
  }, [taskId]);
  const router = useRouter();

  const {
    task,
    links,
    attachments,
    isLoading,
    error,
    isEditMode,
    isMounted,
    canUpload,
    userRole,
    currentUserId,
    editForm,
    uploadFiles,
    setUploadFiles,
    attachmentsToDelete,
    refetch,
    refetchAttachments,
    handleEditTask,
    handleCancelEdit,
    handleSaveChanges,
    handleDeleteAttachment,
    handleCancelTask,
    uploadMutation,
    deleteAttachmentMutation,
    updateTaskMutation,
    cancelTaskMutation
  } = useTaskDetails(taskId);

  // Track last logged state to avoid duplicate logs
  const lastDataLogRef = useRef<string>("");
  const hasLoggedInitialDataRef = useRef<boolean>(false);

  // Debug: Log data only once when task loads, and again only if task/links/attachments actually change
  useEffect(() => {
    // Skip if still loading or no task
    if (isLoading || !task) {
      return;
    }

    // Create a key from meaningful data (task ID, status, links count, attachments count)
    const dataKey = `${task.id}-${task.status}-${links?.length || 0}-${attachments?.length || 0}`;
    
    // Only log if:
    // 1. This is the first time we have complete data, OR
    // 2. The data actually changed (task status, links, or attachments count changed)
    const shouldLog = !hasLoggedInitialDataRef.current || lastDataLogRef.current !== dataKey;
    
    if (shouldLog) {
      hasLoggedInitialDataRef.current = true;
      lastDataLogRef.current = dataKey;
      debugGroup("TaskDetailsViewManager - Data", () => {
        debugLog("Task loaded", { taskId: task?.id, status: task?.status, title: task?.title });
        debugLog("Links received", {
          links,
          linksType: typeof links,
          linksIsArray: Array.isArray(links),
          linksLength: links?.length,
          linkRels: links?.map((l) => l.rel) || []
        });
        debugLog("Attachments", { count: attachments?.length || 0 });
        debugLog("State", { isLoading, error: error?.message, isMounted, canUpload, userRole });
      });
    }
  }, [task, task?.id, task?.status, links, links?.length, attachments, attachments?.length, isLoading, error, isMounted, canUpload, userRole]);

  const assignMutation = useAssignTaskMutation(taskId);
  const reassignMutation = useReassignTaskMutation(taskId);

  const [isCancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [isAssignOpen, setAssignOpen] = useState(false);
  const [isReassignOpen, setReassignOpen] = useState(false);
  const [isApproveExtensionOpen, setApproveExtensionOpen] = useState(false);
  const [isReviewCompletedOpen, setReviewCompletedOpen] = useState(false);
  const [isDeleteAttachmentOpen, setDeleteAttachmentOpen] = useState(false);
  const [attachmentToDelete, setAttachmentToDelete] = useState<string | null>(null);

  const hasLink = (rel: string) => links.some((link) => link.rel === rel);
  const isFinalReviewedState = task?.status === 3 && task?.managerRating != null;
  const isCancelled = task?.status === 8;

  const confirmDeleteAttachment = async () => {
    if (!attachmentToDelete) return;
    try {
      await deleteAttachmentMutation.mutateAsync(attachmentToDelete);
      await refetchAttachments();
      toast.success(t("common:actions.deleted"));
      setDeleteAttachmentOpen(false);
      setAttachmentToDelete(null);
    } catch (error) {
      displayApiError(error, t("tasks:attachments.actions.delete"));
    }
  };

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
              {isEditMode ? (
                <Input
                  className="font-heading text-2xl font-semibold"
                  {...editForm.register("title")}
                  placeholder={t("tasks:forms.create.fields.titlePlaceholder")}
                />
              ) : (
                <h1 className="font-heading text-2xl text-foreground">{task.title}</h1>
              )}
              <TaskStatusBadge status={task.status} managerRating={task.managerRating ?? null} />
            </div>
            {!isEditMode && (
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
            )}
          </div>

          <div className="flex flex-wrap items-center gap-2">
            {/* Edit/Save/Cancel buttons */}
            {hasLink("update") && !isFinalReviewedState && !isCancelled && (
              <>
                {!isEditMode ? (
                  <Button variant="secondary" onClick={handleEditTask} icon={<PencilIcon />}>
                    {t("common:actions.edit")}
                  </Button>
                ) : (
                  <>
                    <Button variant="outline" onClick={handleCancelEdit} icon={<XMarkIcon />}>
                      {t("common:actions.cancel")}
                    </Button>
                    <Button
                      variant="primary"
                      onClick={editForm.handleSubmit(handleSaveChanges)}
                      disabled={updateTaskMutation.isPending}
                      icon={<CheckIcon />}
                    >
                      {t("common:actions.save")}
                    </Button>
                  </>
                )}
              </>
            )}

            {!isEditMode && hasLink("cancel") && !isFinalReviewedState && !isCancelled && (
              <Button variant="destructive" onClick={() => setCancelDialogOpen(true)} icon={<StopIcon />} disabled={cancelTaskMutation.isPending}>
                {t("common:actions.cancel")}
              </Button>
            )}

            {!isEditMode && hasLink("assign") && !isFinalReviewedState && !isCancelled && (
              <Button variant="outline" onClick={() => setAssignOpen(true)}>
                {t("tasks:forms.assign.title")}
              </Button>
            )}

            {!isEditMode && hasLink("reassign") && !isFinalReviewedState && !isCancelled && (
              <Button variant="outline" onClick={() => setReassignOpen(true)}>
                {t("tasks:forms.assign.title")}
              </Button>
            )}

            {!isEditMode && hasLink("approve-extension") && !isFinalReviewedState && !isCancelled && (
              <Button variant="outline" onClick={() => setApproveExtensionOpen(true)}>
                {t("tasks:details.actions.approveExtension")}
              </Button>
            )}

            {hasLink("review-completed") && (
              <Button variant="primary" onClick={() => setReviewCompletedOpen(true)}>
                {t("tasks:details.actions.review")}
              </Button>
            )}
          </div>
        </div>
      </header>

      <div className="grid gap-6 lg:grid-cols-3">
        <section className="space-y-4 rounded-xl border border-border bg-background p-6 shadow-sm lg:col-span-2">
          {isEditMode ? (
            <TaskEditForm form={editForm} onSubmit={handleSaveChanges} />
          ) : (
            <>
              <h2 className="text-lg font-semibold text-foreground">{t("tasks:details.sections.description")}</h2>
              <p className="text-sm text-muted-foreground">{task.description ?? t("common:states.empty")}</p>
            </>
          )}

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

          <h3 className="text-base font-semibold text-foreground">{t("tasks:details.sections.assignments")}</h3>
          <div className="space-y-2 text-sm">
            <div className="flex flex-col rounded-lg border border-border bg-muted/30 p-3">
              <span className="font-medium text-foreground">
                {t("common:filters.assignedTo")}: {task.assignedUserEmail ?? "—"}
              </span>
              <span className="text-xs text-muted-foreground">{task.assignedUserId}</span>
            </div>
          </div>

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
                attachments={isEditMode ? attachments : attachments.filter((a) => !attachmentsToDelete.has(a.id))}
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
                {...(isEditMode && { onDelete: handleDeleteAttachment })}
                canDelete={(attachment) =>
                  isEditMode && task ? canDeleteAttachment(attachment, task, currentUserId, userRole) : false
                }
                {...(isEditMode && attachmentsToDelete.size > 0 && { markedForDeletion: attachmentsToDelete })}
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
                              type: AttachmentTypeEnum.ManagerUploaded
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

      {/* Task History Timeline */}
      <div className="mt-6">
        <TaskHistoryTimeline taskId={taskId} />
      </div>

      {/* Dialogs */}
      <AssignTaskDialog
        open={isAssignOpen}
        onOpenChange={setAssignOpen}
        mutation={{ mutateAsync: assignMutation.mutateAsync, isPending: assignMutation.isPending }}
      />
      <AssignTaskDialog
        open={isReassignOpen}
        onOpenChange={setReassignOpen}
        mutation={{ mutateAsync: reassignMutation.mutateAsync, isPending: reassignMutation.isPending }}
        mode="reassign"
      />
      <ApproveExtensionDialog open={isApproveExtensionOpen} onOpenChange={setApproveExtensionOpen} taskId={task.id} />
      {task && (
        <ReviewCompletedTaskModal
          taskId={task.id}
          taskTitle={task.title}
          isOpen={isReviewCompletedOpen}
          onClose={() => {
            setReviewCompletedOpen(false);
            router.refresh();
          }}
        />
      )}
      <CancelTaskDialog
        open={isCancelDialogOpen}
        onOpenChange={setCancelDialogOpen}
        onConfirm={handleCancelTask}
        isPending={cancelTaskMutation.isPending}
      />
      <DeleteAttachmentDialog
        open={isDeleteAttachmentOpen}
        onOpenChange={(open) => {
          setDeleteAttachmentOpen(open);
          if (!open) {
            setAttachmentToDelete(null);
          }
        }}
        onConfirm={confirmDeleteAttachment}
        isPending={deleteAttachmentMutation.isPending}
      />
    </div>
  );
}

