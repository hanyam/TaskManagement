using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.Tasks;

/// <summary>
///     Centralized error definitions for Task-related operations.
/// </summary>
public static class TaskErrors
{
    // Task not found errors
    public static Error NotFound => Error.Create("NOT_FOUND", "Task not found", "Id", "Errors.Tasks.NotFound");
    
    public static Error NotFoundById(Guid taskId) => Error.Create("NOT_FOUND", $"Task with ID '{taskId}' not found", "Id", "Errors.Tasks.NotFoundById");

    // Task validation errors
    public static Error TitleRequired => Error.Create("VALIDATION_ERROR", "Title is required", "Title", "Errors.Tasks.TitleRequired");
    public static Error TitleTooLong => Error.Create("VALIDATION_ERROR", "Title cannot exceed 200 characters", "Title", "Errors.Tasks.TitleTooLong");

    public static Error DescriptionTooLong =>
        Error.Create("VALIDATION_ERROR", "Description cannot exceed 1000 characters", "Description", "Errors.Tasks.DescriptionTooLong");

    public static Error DueDateInPast => Error.Create("VALIDATION_ERROR", "Due date cannot be in the past", "DueDate", "Errors.Tasks.DueDateInPast");
    public static Error InvalidPriority => Error.Create("VALIDATION_ERROR", "Invalid priority value", "Priority", "Errors.Tasks.InvalidPriority");
    public static Error InvalidStatus => Error.Create("VALIDATION_ERROR", "Invalid status value", "Status", "Errors.Tasks.InvalidStatus");

    // Task business logic errors
    public static Error CannotUpdateCompletedTask => Error.Create("VALIDATION_ERROR", "Cannot update a completed task", "Status", "Errors.Tasks.CannotUpdateCompletedTask");
    public static Error CannotDeleteCompletedTask => Error.Create("VALIDATION_ERROR", "Cannot delete a completed task", "Status", "Errors.Tasks.CannotDeleteCompletedTask");
    public static Error CannotAcceptPassedDueDateTask => Error.Create("VALIDATION_ERROR", "Cannot accept a passed due date task", "Status", "Errors.Tasks.CannotAcceptPassedDueDateTask");
    public static Error CannotRejectPassedDueDateTask => Error.Create("VALIDATION_ERROR", "Cannot reject a passed due date task", "Status", "Errors.Tasks.CannotRejectPassedDueDateTask");
    public static Error TaskAlreadyCompleted => Error.Create("CONFLICT", "Task is already completed", "Status", "Errors.Tasks.TaskAlreadyCompleted");
    public static Error TaskAlreadyAcceptedByManager => Error.Create("CONFLICT", "Task has already been accepted by manager and cannot be modified", "Status", "Errors.Tasks.TaskAlreadyAcceptedByManager");
    public static Error TaskRejectedByManager => Error.Create("CONFLICT", "Task has been rejected by manager and cannot be modified", "Status", "Errors.Tasks.TaskRejectedByManager");
    public static Error TaskNotAssigned => Error.Create("VALIDATION_ERROR", "Task must be assigned to a user", "AssignedUserId", "Errors.Tasks.TaskNotAssigned");

    // Task assignment errors
    public static Error AssignedUserNotFound => Error.Create("NOT_FOUND", "Assigned user not found", "AssignedUserId", "Errors.Tasks.AssignedUserNotFound");
    public static Error AssignedUserInactive => Error.Create("VALIDATION_ERROR", "Assigned user is inactive", "AssignedUserId", "Errors.Tasks.AssignedUserInactive");
    public static Error CannotAssignToSelf => Error.Create("VALIDATION_ERROR", "Cannot assign task to yourself", "AssignedUserId", "Errors.Tasks.CannotAssignToSelf");

    public static Error AssignerMustBeManagerOfAssignee =>
        Error.Create("FORBIDDEN", "You must be a manager of the assigned employee to assign tasks to them", null, "Errors.Tasks.AssignerMustBeManagerOfAssignee");

    // Task pagination errors
    public static Error InvalidPageNumber => Error.Create("VALIDATION_ERROR", "Page must be greater than 0", "Page", "Errors.Tasks.InvalidPageNumber");
    public static Error InvalidPageSize => Error.Create("VALIDATION_ERROR", "Page size must be between 1 and 100", "PageSize", "Errors.Tasks.InvalidPageSize");

    public static Error InvalidDateRange =>
        Error.Create("VALIDATION_ERROR", "Due date from cannot be greater than due date to", "DueDateFrom", "Errors.Tasks.InvalidDateRange");

    // Task creation errors
    public static Error CreatedByRequired => Error.Create("VALIDATION_ERROR", "Created by user is required", "CreatedBy", "Errors.Tasks.CreatedByRequired");
    public static Error CreatedByNotFound => Error.Create("NOT_FOUND", "Created by user not found", "CreatedBy", "Errors.Tasks.CreatedByNotFound");
    public static Error CreatedByInactive => Error.Create("VALIDATION_ERROR", "Created by user is inactive", "CreatedBy", "Errors.Tasks.CreatedByInactive");

    // Task update errors
    public static Error CannotUpdateOtherUserTask => Error.Create("FORBIDDEN", "Cannot update task created by another user", null, "Errors.Tasks.CannotUpdateOtherUserTask");
    public static Error CannotDeleteOtherUserTask => Error.Create("FORBIDDEN", "Cannot delete task created by another user", null, "Errors.Tasks.CannotDeleteOtherUserTask");
    public static Error TaskUpdateConflict => Error.Create("CONFLICT", "Task was modified by another user", "Id", "Errors.Tasks.TaskUpdateConflict");
    public static Error CannotCancelReviewedTask =>
        Error.Create("CONFLICT", "Task has already been reviewed or completed and cannot be cancelled", "Status", "Errors.Tasks.CannotCancelReviewedTask");

    public static Error TaskAlreadyCancelled =>
        Error.Create("VALIDATION_ERROR", "Task is already cancelled", "Status", "Errors.Tasks.TaskAlreadyCancelled");

    // Task query errors
    public static Error InvalidTaskId => Error.Create("VALIDATION_ERROR", "Task ID is required", "Id", "Errors.Tasks.InvalidTaskId");
    public static Error InvalidUserId => Error.Create("VALIDATION_ERROR", "User ID is required", "UserId", "Errors.Tasks.InvalidUserId");
    public static Error InvalidFilterParameters => Error.Create("VALIDATION_ERROR", "Invalid filter parameters provided", "Filters", "Errors.Tasks.InvalidFilterParameters");

    // Task access control errors
    public static Error AccessDenied =>
        Error.Create("FORBIDDEN",
            "You do not have access to this task. Tasks can only be accessed by the creator, assigned user, or users in the assignment chain.", null, "Errors.Tasks.AccessDenied");

    // Task attachment errors
    public static Error FileSizeExceeded => Error.Create("VALIDATION_ERROR", "File size exceeds the maximum allowed size", "File", "Errors.Tasks.FileSizeExceeded");
    public static Error FileUploadFailed => Error.Create("INTERNAL_ERROR", "Failed to upload file", null, "Errors.Tasks.FileUploadFailed");
    public static Error FileNotFound => Error.Create("NOT_FOUND", "File attachment not found", "AttachmentId", "Errors.Tasks.FileNotFound");
    public static Error UnauthorizedFileAccess => Error.Create("FORBIDDEN", "You do not have permission to access this file attachment", null, "Errors.Tasks.UnauthorizedFileAccess");
    public static Error InvalidFileName => Error.Create("VALIDATION_ERROR", "File name is invalid", "FileName", "Errors.Tasks.InvalidFileName");
    public static Error AttachmentNotFound => Error.Create("NOT_FOUND", "Attachment not found", "AttachmentId", "Errors.Tasks.AttachmentNotFound");
}