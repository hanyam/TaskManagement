"use client";

import { ArrowPathIcon, StarIcon } from "@heroicons/react/24/outline";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { useAuth } from "@/core/auth/AuthProvider";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import {
  useAssignTaskMutation,
  useAcceptTaskMutation,
  useMarkTaskCompletedMutation,
  useRejectTaskMutation,
  useReassignTaskMutation,
  useTaskDetailsQuery,
  useUpdateTaskMutation,
  useTaskAttachmentsQuery,
  useUploadAttachmentMutation,
  useDeleteAttachmentMutation,
  useCancelTaskMutation,
  downloadAttachment
} from "@/features/tasks/api/queries";
import { FileAttachmentList } from "@/features/tasks/components/FileAttachmentList";
import { ReviewCompletedTaskModal } from "@/features/tasks/components/ReviewCompletedTaskModal";
import { TaskStatusBadge } from "@/features/tasks/components/TaskStatusBadge";
import { TaskEditForm, type EditTaskFormValues } from "@/features/tasks/components/TaskEditForm";
import { TaskMetadata } from "@/features/tasks/components/TaskMetadata";
import { TaskActionButtons } from "@/features/tasks/components/TaskActionButtons";
import { AssignTaskDialog } from "@/features/tasks/components/dialogs/AssignTaskDialog";
import { UpdateProgressDialog } from "@/features/tasks/components/dialogs/UpdateProgressDialog";
import { RequestExtensionDialog } from "@/features/tasks/components/dialogs/RequestExtensionDialog";
import { RequestMoreInfoDialog } from "@/features/tasks/components/dialogs/RequestMoreInfoDialog";
import { ApproveExtensionDialog } from "@/features/tasks/components/dialogs/ApproveExtensionDialog";
import { CancelTaskDialog } from "@/features/tasks/components/dialogs/CancelTaskDialog";
import { DeleteAttachmentDialog } from "@/features/tasks/components/dialogs/DeleteAttachmentDialog";
import { canViewAttachment, canUploadAttachment, canDeleteAttachment } from "@/features/tasks/utils/attachmentAccess";
import { getTaskPriorityString, AttachmentTypeEnum, TaskPriorityEnum } from "@/features/tasks/value-objects";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { formatDate } from "@/features/tasks/utils/dateFormatting";
import { Button } from "@/ui/components/Button";
import { FileUpload, type FileUploadItem } from "@/ui/components/FileUpload";
import { Input } from "@/ui/components/Input";
import { Spinner } from "@/ui/components/Spinner";

interface TaskDetailsViewProps {
  taskId: string;
}

const editTaskSchema = z.object({
  title: z.string().min(1, "validation:required"),
  description: z.string().max(2000).optional().or(z.literal("")),
  priority: z.enum(["Low", "Medium", "High", "Critical"]),
  dueDate: z
    .string()
    .optional()
    .refine((value) => !value || !Number.isNaN(Date.parse(value)), {
      message: "validation:futureDate"
    }),
  assignedUserId: z.string().optional().or(z.literal(""))
});

export function TaskDetailsView({ taskId }: TaskDetailsViewProps) {
  const { t } = useTranslation(["tasks", "common", "navigation"]);
  const router = useRouter();
  const locale = useCurrentLocale();
  const { session } = useAuth();
  const { data: response, isLoading, error, refetch } = useTaskDetailsQuery(taskId, Boolean(taskId));
  const { data: attachments = [], refetch: refetchAttachments } = useTaskAttachmentsQuery(taskId);

  const task = response?.data;
  const links = response?.links ?? [];

  const currentUserId = session?.user?.id || "";
  const userRole = session?.user?.role || "";

  // Filter attachments based on access control
  const accessibleAttachments = task
    ? attachments.filter((attachment) => canViewAttachment(attachment, task, userRole))
    : [];

  const uploadMutation = useUploadAttachmentMutation(taskId);
  const deleteAttachmentMutation = useDeleteAttachmentMutation(taskId);
  const cancelTaskMutation = useCancelTaskMutation(taskId);
  const updateTaskMutation = useUpdateTaskMutation(taskId);
  const [uploadFiles, setUploadFiles] = useState<FileUploadItem[]>([]);
  const [isDeleteAttachmentOpen, setDeleteAttachmentOpen] = useState(false);
  const [attachmentToDelete, setAttachmentToDelete] = useState<string | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [attachmentsToDelete, setAttachmentsToDelete] = useState<Set<string>>(new Set());

  const editForm = useForm<EditTaskFormValues>({
    resolver: zodResolver(editTaskSchema),
    defaultValues: {
      title: task?.title || "",
      description: task?.description || "",
      priority: task ? getTaskPriorityString(task.priority) : "Medium",
      dueDate: task?.dueDate ? new Date(task.dueDate).toISOString().split("T")[0] : "",
      assignedUserId: task?.assignedUserId || ""
    }
  });

  // Reset form when task data changes or entering edit mode
  useEffect(() => {
    if (task && isEditMode) {
      editForm.reset({
        title: task.title,
        description: task.description || "",
        priority: getTaskPriorityString(task.priority),
        dueDate: task.dueDate ? new Date(task.dueDate).toISOString().split("T")[0] : "",
        assignedUserId: task.assignedUserId || ""
      });
    }
  }, [task, isEditMode, editForm]);

  const acceptMutation = useAcceptTaskMutation(taskId);
  const assignMutation = useAssignTaskMutation(taskId);
  const reassignMutation = useReassignTaskMutation(taskId);
  const rejectMutation = useRejectTaskMutation(taskId);
  const markCompleteMutation = useMarkTaskCompletedMutation(taskId);

  const [isCancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [isAssignOpen, setAssignOpen] = useState(false);
  const [isReassignOpen, setReassignOpen] = useState(false);
  const [isProgressOpen, setProgressOpen] = useState(false);
  const [isExtensionOpen, setExtensionOpen] = useState(false);
  const [isMoreInfoOpen, setMoreInfoOpen] = useState(false);
  const [isApproveExtensionOpen, setApproveExtensionOpen] = useState(false);
  const [isReviewCompletedOpen, setReviewCompletedOpen] = useState(false);

  // Display query errors via toast
  useEffect(() => {
    if (error) {
      displayApiError(error, t("tasks:errors.loadFailed"));
    }
  }, [error, t]);

  const handleDeleteAttachment = (attachmentId: string) => {
    if (isEditMode) {
      setAttachmentsToDelete((prev) => new Set(prev).add(attachmentId));
    } else {
      setAttachmentToDelete(attachmentId);
      setDeleteAttachmentOpen(true);
    }
  };

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

  async function handleAcceptTask() {
    try {
      await acceptMutation.mutateAsync();
      toast.success(t("tasks:details.actions.accept"));
      refetch();
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

  function handleEditTask() {
    if (task) {
      editForm.reset({
        title: task.title,
        description: task.description || "",
        priority: getTaskPriorityString(task.priority),
        dueDate: task.dueDate ? new Date(task.dueDate).toISOString().split("T")[0] : "",
        assignedUserId: task.assignedUserId || ""
      });
    }
    setIsEditMode(true);
    setAttachmentsToDelete(new Set());
    setUploadFiles([]);
  }

  function handleCancelEdit() {
    setIsEditMode(false);
    setAttachmentsToDelete(new Set());
    setUploadFiles([]);
  }

  async function handleSaveChanges(values: EditTaskFormValues) {
    if (!task) return;

    try {
      await updateTaskMutation.mutateAsync({
        title: values.title,
        description: values.description || null,
        priority: TaskPriorityEnum[values.priority],
        dueDate: values.dueDate ? new Date(values.dueDate).toISOString() : null,
        assignedUserId: values.assignedUserId && values.assignedUserId.trim() !== "" ? values.assignedUserId : null
      });

      // Delete marked attachments
      for (const attachmentId of Array.from(attachmentsToDelete)) {
        try {
          await deleteAttachmentMutation.mutateAsync(attachmentId);
        } catch (error) {
          console.error(`Failed to delete attachment ${attachmentId}:`, error);
        }
      }

      // Upload new files
      if (uploadFiles.length > 0) {
        const roleLower = userRole?.toLowerCase() || "";
        const attachmentType =
          roleLower === "manager" || roleLower === "admin"
            ? AttachmentTypeEnum.ManagerUploaded
            : AttachmentTypeEnum.EmployeeUploaded;

        for (const fileItem of uploadFiles) {
          if (!fileItem.error) {
            try {
              await uploadMutation.mutateAsync({
                file: fileItem.file,
                type: attachmentType
              });
            } catch (error) {
              console.error(`Failed to upload file ${fileItem.file.name}:`, error);
            }
          }
        }
      }

      await refetch();
      await refetchAttachments();
      setIsEditMode(false);
      setAttachmentsToDelete(new Set());
      setUploadFiles([]);
      toast.success(t("common:actions.saved"));
    } catch (error) {
      displayApiError(error, t("tasks:errors.updateFailed"));
    }
  }

  async function handleCancelTask() {
    try {
      await cancelTaskMutation.mutateAsync();
      setCancelDialogOpen(false);
      toast.success(t("tasks:details.notifications.cancelled"));
      router.push(`/${locale}/tasks`);
    } catch (error) {
      displayApiError(error, t("tasks:errors.cancelFailed"));
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

          <TaskActionButtons
            task={task}
            links={links}
            isEditMode={isEditMode}
            onEdit={handleEditTask}
            onCancelEdit={handleCancelEdit}
            onSave={editForm.handleSubmit(handleSaveChanges)}
            onAccept={handleAcceptTask}
            onReject={handleRejectTask}
            onMarkCompleted={handleMarkCompleted}
            onAssign={() => setAssignOpen(true)}
            onReassign={() => setReassignOpen(true)}
            onUpdateProgress={() => setProgressOpen(true)}
            onRequestExtension={() => setExtensionOpen(true)}
            onApproveExtension={() => setApproveExtensionOpen(true)}
            onRequestMoreInfo={() => setMoreInfoOpen(true)}
            onReview={() => setReviewCompletedOpen(true)}
            onCancel={() => setCancelDialogOpen(true)}
            isSaving={updateTaskMutation.isPending}
            isAccepting={acceptMutation.isPending}
            isRejecting={rejectMutation.isPending}
            isMarkingCompleted={markCompleteMutation.isPending}
            isCancelling={cancelTaskMutation.isPending}
          />
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

            {accessibleAttachments.length > 0 && (
              <FileAttachmentList
                attachments={isEditMode ? accessibleAttachments : accessibleAttachments.filter((a) => !attachmentsToDelete.has(a.id))}
                onDownload={async (attachmentId) => {
                  try {
                    const blob = await downloadAttachment(taskId, attachmentId);
                    const url = window.URL.createObjectURL(blob);
                    const a = document.createElement("a");
                    a.href = url;
                    const attachment = accessibleAttachments.find((a) => a.id === attachmentId);
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

            {isEditMode && task && canUploadAttachment(task, userRole, currentUserId) && (
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
                            const roleLower = userRole?.toLowerCase() || "";
                            const attachmentType =
                              roleLower === "manager" || roleLower === "admin"
                                ? AttachmentTypeEnum.ManagerUploaded
                                : AttachmentTypeEnum.EmployeeUploaded;
                            await uploadMutation.mutateAsync({
                              file: fileItem.file,
                              type: attachmentType
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

            {accessibleAttachments.length === 0 && task && !canUploadAttachment(task, userRole, currentUserId) && (
              <p className="text-sm text-muted-foreground">{t("tasks:attachments.empty")}</p>
            )}
          </div>
        </section>

        <TaskMetadata task={task} />
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
      <UpdateProgressDialog open={isProgressOpen} onOpenChange={setProgressOpen} taskId={task.id} />
      <RequestExtensionDialog open={isExtensionOpen} onOpenChange={setExtensionOpen} taskId={task.id} />
      <RequestMoreInfoDialog open={isMoreInfoOpen} onOpenChange={setMoreInfoOpen} taskId={task.id} />
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

