import { useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useTranslation } from "react-i18next";

import { useAuth } from "@/core/auth/AuthProvider";
import { useCurrentLocale } from "@/core/routing/useCurrentLocale";
import {
  useTaskDetailsQuery,
  useTaskAttachmentsQuery,
  useUpdateTaskMutation,
  useDeleteAttachmentMutation,
  useUploadAttachmentMutation,
  useCancelTaskMutation
} from "@/features/tasks/api/queries";
import { canViewAttachment, canUploadAttachment } from "@/features/tasks/utils/attachmentAccess";
import { getTaskPriorityString } from "@/features/tasks/value-objects";
import { TaskPriorityEnum, AttachmentTypeEnum } from "@/features/tasks/value-objects";
import { displayApiError } from "@/features/tasks/utils/errorHandling";
import { debugError } from "@/core/debug/logger";
import type { FileUploadItem } from "@/ui/components/FileUpload";

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

export type EditTaskFormValues = z.infer<typeof editTaskSchema>;

export function useTaskDetails(taskId: string) {
  const { t } = useTranslation(["tasks", "common", "validation"]);
  const router = useRouter();
  const locale = useCurrentLocale();
  const { session } = useAuth();
  const { data: response, isLoading, error, refetch } = useTaskDetailsQuery(taskId, Boolean(taskId));
  const { data: attachments = [], refetch: refetchAttachments } = useTaskAttachmentsQuery(taskId);

  const task = response?.data;
  const links = response?.links ?? [];
  const currentUserId = session?.user?.id || "";
  const userRole = session?.user?.role || "";
  const [isMounted, setIsMounted] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [uploadFiles, setUploadFiles] = useState<FileUploadItem[]>([]);
  const [attachmentsToDelete, setAttachmentsToDelete] = useState<Set<string>>(new Set());

  const updateTaskMutation = useUpdateTaskMutation(taskId);
  const uploadMutation = useUploadAttachmentMutation(taskId);
  const deleteAttachmentMutation = useDeleteAttachmentMutation(taskId);
  const cancelTaskMutation = useCancelTaskMutation(taskId);

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

  // Prevent hydration mismatch
  useEffect(() => {
    setIsMounted(true);
  }, []);

  // Filter attachments based on access control
  const accessibleAttachments = useMemo(() => {
    if (!isMounted || !task) {
      return [];
    }
    return attachments.filter((attachment) => canViewAttachment(attachment, task, userRole));
  }, [isMounted, task, attachments, userRole]);

  // Check if upload should be visible
  const canUpload = useMemo(() => {
    if (!isMounted || isLoading || !task || task.status === undefined) {
      return false;
    }
    return canUploadAttachment(task, userRole);
  }, [isMounted, isLoading, task, task?.status, userRole]);

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

  // Display query errors via toast
  useEffect(() => {
    if (error) {
      displayApiError(error, t("tasks:errors.loadFailed"));
    }
  }, [error, t]);

  const handleEditTask = () => {
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
  };

  const handleCancelEdit = () => {
    setIsEditMode(false);
    setAttachmentsToDelete(new Set());
    setUploadFiles([]);
  };

  const handleDeleteAttachment = (attachmentId: string) => {
    if (isEditMode) {
      setAttachmentsToDelete((prev) => new Set(prev).add(attachmentId));
    }
  };

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
          debugError(`Failed to delete attachment ${attachmentId}`, error);
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
              debugError(`Failed to upload file ${fileItem.file.name}`, error);
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
      toast.success(t("tasks:details.notifications.cancelled"));
      router.push(`/${locale}/tasks`);
    } catch (error) {
      displayApiError(error, t("tasks:errors.cancelFailed"));
    }
  }

  return {
    task,
    links,
    attachments: accessibleAttachments,
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
    updateTaskMutation,
    uploadMutation,
    deleteAttachmentMutation,
    cancelTaskMutation
  };
}

