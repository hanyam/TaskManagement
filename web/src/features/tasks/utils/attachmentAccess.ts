import type { TaskAttachmentDto, TaskDto } from "@/features/tasks/types";
import { getAttachmentTypeString, getTaskStatusString } from "@/features/tasks/value-objects";

/**
 * Checks if a user can view a specific attachment based on task status and attachment type.
 */
export function canViewAttachment(
  attachment: TaskAttachmentDto,
  task: TaskDto,
  userRole: string
): boolean {
  // Managers and Admins can always view attachments (case-insensitive check)
  const roleLower = userRole?.toLowerCase() || "";
  if (roleLower === "manager" || roleLower === "admin") {
    return true;
  }

  const taskStatus = getTaskStatusString(task.status);
  const attachmentType = getAttachmentTypeString(attachment.type);

  // Manager-uploaded files: visible if task is Accepted, UnderReview, PendingManagerReview, or Completed
  if (attachmentType === "ManagerUploaded") {
    return (
      taskStatus === "Accepted" ||
      taskStatus === "UnderReview" ||
      taskStatus === "PendingManagerReview" ||
      taskStatus === "Completed"
    );
  }

  // Employee-uploaded files: visible if task is UnderReview, PendingManagerReview, or Completed
  if (attachmentType === "EmployeeUploaded") {
    return (
      taskStatus === "UnderReview" ||
      taskStatus === "PendingManagerReview" ||
      taskStatus === "Completed"
    );
  }

  return false;
}

/**
 * Checks if a user can upload attachments to a task.
 */
export function canUploadAttachment(task: TaskDto, userRole: string, currentUserId: string): boolean {
  const taskStatus = getTaskStatusString(task.status);
  const roleLower = userRole?.toLowerCase() || "";

  // Managers can upload when creating/editing task (Created, Assigned statuses)
  if (roleLower === "manager" || roleLower === "admin") {
    return taskStatus === "Created" || taskStatus === "Assigned";
  }

  // Employees can upload when task is Accepted or UnderReview
  if (roleLower === "employee") {
    return (
      taskStatus === "Accepted" ||
      taskStatus === "UnderReview" ||
      (task.assignedUserId === currentUserId && taskStatus === "Assigned")
    );
  }

  return false;
}

/**
 * Checks if a user can delete a specific attachment.
 */
export function canDeleteAttachment(
  attachment: TaskAttachmentDto,
  task: TaskDto,
  currentUserId: string,
  userRole: string
): boolean {
  // Only uploader or task creator can delete (case-insensitive role check)
  const roleLower = userRole?.toLowerCase() || "";
  return (
    attachment.uploadedById === currentUserId ||
    task.createdById === currentUserId ||
    roleLower === "admin"
  );
}

