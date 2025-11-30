import type { TaskAttachmentDto, TaskDto } from "@/features/tasks/types";
import { getAttachmentTypeString, getTaskStatusString } from "@/features/tasks/value-objects";

/**
 * Checks if a user can view a specific attachment based on task status and attachment type.
 * Uses backend-provided isManager property instead of client-side role for task creator relationship.
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

  // Use backend-provided isManager property (indicates if current user is task creator)
  const isManager = task.isManager ?? false;
  const isAdmin = roleLower === "admin";

  // Admins can always view all attachments regardless of status
  if (isAdmin) {
    return true;
  }

  // Check if task is in "Accepted by Manager" state (Accepted status with ManagerRating set)
  const isAcceptedByManager = taskStatus === "Accepted" && task.managerRating != null;

  // Task creators (managers) have special rules:
  if (isManager) {
    if (attachmentType === "ManagerUploaded") {
      // Manager-uploaded files: always visible to task creators
      return true;
    }

    if (attachmentType === "EmployeeUploaded") {
      // Employee-uploaded files: visible to task creator during review phase (PendingManagerReview) and after manager accepts (Accepted with ManagerRating)
      // Task creators should see employee attachments while reviewing and after accepting
      return taskStatus === "PendingManagerReview" || isAcceptedByManager;
    }
  }

  // Assigned users (employees, not task creators):
  // Manager-uploaded files: visible if task is Assigned, Accepted (employee accepted) or later, but NOT in Created status
  if (attachmentType === "ManagerUploaded") {
    return (
      taskStatus !== "Created" &&
      (taskStatus === "Assigned" ||
      taskStatus === "Accepted" ||
      taskStatus === "UnderReview" ||
      taskStatus === "PendingManagerReview" ||
        taskStatus === "Completed")
    );
  }

  // Employee-uploaded files: visible if task is Assigned, Accepted (employee accepted, no ManagerRating) or later
  // Employees should be able to see their own uploaded attachments in Assigned status and after accepting (Accepted status without ManagerRating)
  if (attachmentType === "EmployeeUploaded") {
    // Employee can see their own attachments in Assigned, Accepted (employee accepted), UnderReview, PendingManagerReview, or Completed
    // Also visible in Accepted by Manager state
    return (
      taskStatus === "Assigned" ||
      (taskStatus === "Accepted" && !isAcceptedByManager) ||
      taskStatus === "UnderReview" ||
      taskStatus === "PendingManagerReview" ||
      taskStatus === "Completed" ||
      isAcceptedByManager
    );
  }

  return false;
}

/**
 * Checks if a user can upload attachments to a task.
 * Uses backend-provided isManager property instead of client-side role.
 * Note: This is a UI-level check. Backend will enforce actual authorization.
 */
export function canUploadAttachment(task: TaskDto, userRole: string): boolean {
  // Safety check: ensure task and status exist
  if (!task || task.status === undefined || task.status === null) {
    return false;
  }

  const taskStatus = getTaskStatusString(task.status);
  const roleLower = userRole?.toLowerCase() || "";
  const isAdmin = roleLower === "admin";
  
  // Use backend-provided isManager property (indicates if current user is task creator)
  const isManager = task.isManager ?? false;

  // Admins can always upload
  if (isAdmin) {
    return taskStatus === "Created" || taskStatus === "Assigned";
  }

  // Task creators (managers) can upload when creating/editing task (Created, Assigned statuses)
  if (isManager) {
    return taskStatus === "Created" || taskStatus === "Assigned";
  }

  // Employees (assigned users, not creators) can upload attachments when task is Assigned, Accepted (employee accepted, no ManagerRating), or UnderReview.
  // Allowed statuses: Assigned, Accepted (employee accepted, no ManagerRating), UnderReview.
  // NOT allowed: Created, PendingManagerReview, Completed, Cancelled, RejectedByManager, Accepted by Manager.
  if (taskStatus === "Created") {
    return false;
  }
  
  const isAcceptedByManager = taskStatus === "Accepted" && task.managerRating != null;
  return (
    taskStatus === "Assigned" ||
    (taskStatus === "Accepted" && !isAcceptedByManager) ||
    taskStatus === "UnderReview"
  );
}

/**
 * Checks if a user can delete a specific attachment.
 * Uses backend-provided isManager property instead of client-side role for task creator relationship.
 */
export function canDeleteAttachment(
  attachment: TaskAttachmentDto,
  task: TaskDto,
  currentUserId: string, // Keep for backward compatibility, but prefer task.currentUserId
  userRole: string
): boolean {
  // Safety check: ensure task exists
  if (!task) {
    return false;
  }

  // Use backend-provided currentUserId (supports impersonation) or fallback to cached session
  const backendCurrentUserId = task.currentUserId || currentUserId;
  if (!backendCurrentUserId) {
    return false;
  }

  const roleLower = userRole?.toLowerCase()?.trim() || "";
  const taskStatus = getTaskStatusString(task.status);
  const isAdmin = roleLower === "admin";
  
  // Use backend-provided isManager property (indicates if current user is task creator)
  const isManager = task.isManager ?? false;

  // Check if user is the uploader or task creator (only if IDs are valid)
  // Use backend currentUserId for accurate comparison (supports impersonation)
  const isUploader = Boolean(attachment.uploadedById && backendCurrentUserId && attachment.uploadedById === backendCurrentUserId);
  const isCreator = Boolean(task.createdById && backendCurrentUserId && task.createdById === backendCurrentUserId);
  
  // Check attachment type
  const attachmentType = getAttachmentTypeString(attachment.type);
  
  // Determine if user is an employee
  // CRITICAL: Use task.isManager from backend as the PRIMARY source of truth
  // If task.isManager is false, the user is an employee (even if they created the task, they're viewing as employee)
  // The backend sets isManager based on the current user's relationship to the task
  const isEmployee = !isManager;
  
  // Admins can always delete any attachment
  if (isAdmin) {
    return true;
  }

  // ============================================
  // MANAGER ATTACHMENTS - Separate validation
  // ============================================
  if (attachmentType === "ManagerUploaded") {
    // CRITICAL: Employees can NEVER delete manager attachments
    // Use task.isManager from backend as the ONLY source of truth
    // If isManager is false, user is an employee (even if isCreator is true)
    if (!isManager) {
      return false;
    }
    
    // Only managers (task.isManager = true from backend) can delete manager attachments
    // They can delete if they uploaded it OR created the task
    if (isManager && (isUploader || isCreator)) {
      return true;
    }
    
    // All other cases: cannot delete manager attachments
    return false;
  }

  // ============================================
  // EMPLOYEE ATTACHMENTS - Separate validation
  // ============================================
  if (attachmentType === "EmployeeUploaded") {
    // Employees can delete their own employee attachments (with status restrictions)
    if (isEmployee) {
      // Must be the uploader
      if (!isUploader) {
        return false;
      }
      
      // Check task status - employees can delete in Assigned, Accepted (employee accepted) or UnderReview
      const isAcceptedByManager = taskStatus === "Accepted" && task.managerRating != null;
      return (
        taskStatus === "Assigned" ||
        (taskStatus === "Accepted" && !isAcceptedByManager) ||
        taskStatus === "UnderReview"
      );
    }
    
    // Task creators (managers) can delete employee attachments if they uploaded it or created the task
    // Only allow if isManager is true (from backend)
    if (isManager && (isUploader || isCreator)) {
      return true;
    }
    
    // All other cases: cannot delete employee attachments
    return false;
  }

  // Unknown attachment type - cannot delete
  return false;
}

