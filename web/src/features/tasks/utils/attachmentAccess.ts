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

  // Check if task is in "Accepted by Manager" state (Accepted status with ManagerRating set)
  const isAcceptedByManager = taskStatus === "Accepted" && task.managerRating != null;

  // Managers have special rules:
  if (isManager) {
    if (attachmentType === "ManagerUploaded") {
      // Manager-uploaded files: always visible to managers
      return true;
    }

    if (attachmentType === "EmployeeUploaded") {
      // Employee-uploaded files: visible to manager during review phase (PendingManagerReview) and after manager accepts (Accepted with ManagerRating)
      // Managers should see employee attachments while reviewing and after accepting
      return taskStatus === "PendingManagerReview" || isAcceptedByManager;
    }
  }

  // Employee / other roles:
  // Manager-uploaded files: visible if task is Accepted (employee accepted) or later, but NOT in Created status
  if (attachmentType === "ManagerUploaded") {
    return (
      taskStatus !== "Created" &&
      (taskStatus === "Accepted" ||
        taskStatus === "UnderReview" ||
        taskStatus === "PendingManagerReview" ||
        taskStatus === "Completed")
    );
  }

  // Employee-uploaded files: visible if task is Accepted (employee accepted, no ManagerRating) or later
  // Employees should be able to see their own uploaded attachments immediately after uploading (Accepted status without ManagerRating)
  if (attachmentType === "EmployeeUploaded") {
    // Employee can see their own attachments in Accepted (employee accepted), UnderReview, PendingManagerReview, or Completed
    // But NOT in Accepted by Manager state (unless it's also in other statuses)
    return (
      (taskStatus === "Accepted" && !isAcceptedByManager) ||
      taskStatus === "UnderReview" ||
      taskStatus === "PendingManagerReview" ||
      taskStatus === "Completed" ||
      isAcceptedByManager // Also visible in Accepted by Manager state
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

  // Employees can upload attachments after they accept the task and while the task is still in progress.
  // Allowed statuses: Accepted (employee accepted, no ManagerRating), UnderReview.
  // Not allowed: Created, Assigned (must accept first), PendingManagerReview (marked complete), Completed, Cancelled, RejectedByManager, Accepted by Manager.
  if (roleLower === "employee") {
    const isAcceptedByManager = taskStatus === "Accepted" && task.managerRating != null;
    return (
      (taskStatus === "Accepted" && !isAcceptedByManager) ||
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

  const roleLower = userRole?.toLowerCase() || "";
  const taskStatus = getTaskStatusString(task.status);
  const isAdmin = roleLower === "admin";
  const isManager = roleLower === "manager";
  const isEmployee = roleLower === "employee";

  // Admins can always delete
  if (isAdmin) {
    return true;
  }

  // Check if user is the uploader or task creator
  const isUploader = attachment.uploadedById === currentUserId;
  const isCreator = task.createdById === currentUserId;

  if (!isUploader && !isCreator) {
    return false;
  }

  // Employees cannot delete attachments once task is pending manager review, completed, or accepted by manager
  // They can delete in Accepted (employee accepted, no ManagerRating) or UnderReview statuses
  if (isEmployee && !isCreator) {
    const isAcceptedByManager = taskStatus === "Accepted" && task.managerRating != null;
    // Employee uploader can delete in Accepted (employee accepted) or UnderReview, but not PendingManagerReview, Completed, or Accepted by Manager
    return (
      (taskStatus === "Accepted" && !isAcceptedByManager) ||
      taskStatus === "UnderReview"
    );
  }

  // Managers and task creators can delete (subject to backend validation)
  // Managers can delete their own uploads, creators can delete any attachment
  if (isManager || isCreator) {
    return true;
  }

  return false;
}

