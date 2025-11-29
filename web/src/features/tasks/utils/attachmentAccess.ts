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
  // Safety check: ensure task and status exist
  if (!task || task.status === undefined || task.status === null) {
    return false;
  }

  const roleLower = userRole?.toLowerCase() || "";
  const taskStatus = getTaskStatusString(task.status);
  const attachmentType = getAttachmentTypeString(attachment.type);

  const isManager = roleLower === "manager";
  const isAdmin = roleLower === "admin";

  // Admins can always view all attachments regardless of status
  if (isAdmin) {
    return true;
  }

  // Managers have special rules:
  if (isManager) {
    if (attachmentType === "ManagerUploaded") {
      // Manager-uploaded files: always visible to managers
      return true;
    }

    if (attachmentType === "EmployeeUploaded") {
      // Employee-uploaded files: visible to manager only when employee has marked
      // the task as completed (PendingManagerReview or Completed)
      return taskStatus === "PendingManagerReview" || taskStatus === "Completed";
    }
  }

  // Employee / other roles:
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
 * Note: This is a UI-level check. Backend will enforce actual authorization.
 */
export function canUploadAttachment(task: TaskDto, userRole: string): boolean {
  // Safety check: ensure task and status exist
  if (!task || task.status === undefined || task.status === null) {
    return false;
  }

  const taskStatus = getTaskStatusString(task.status);
  const roleLower = userRole?.toLowerCase() || "";

  // Managers can upload when creating/editing task (Created, Assigned statuses)
  if (roleLower === "manager" || roleLower === "admin") {
    return taskStatus === "Created" || taskStatus === "Assigned";
  }

  // Employees can upload only after they accept the task and before manager review is completed.
  // Allowed statuses: Accepted, UnderReview.
  // Not allowed: Created, Assigned (must accept first), PendingManagerReview, Completed, Cancelled, etc.
  if (roleLower === "employee") {
    return (
      taskStatus === "Accepted" ||
      taskStatus === "UnderReview"
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
  // Safety check: ensure task exists
  if (!task) {
    return false;
  }

  // Only uploader or task creator can delete (case-insensitive role check)
  const roleLower = userRole?.toLowerCase() || "";
  return (
    attachment.uploadedById === currentUserId ||
    task.createdById === currentUserId ||
    roleLower === "admin"
  );
}

