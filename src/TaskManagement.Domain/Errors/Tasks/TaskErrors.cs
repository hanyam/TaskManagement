using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Errors.Tasks;

/// <summary>
///     Centralized error definitions for Task-related operations.
/// </summary>
public static class TaskErrors
{
    // Task not found errors
    public static Error NotFound => Error.NotFound("Task", "Id");

    // Task validation errors
    public static Error TitleRequired => Error.Validation("Title is required", "Title");
    public static Error TitleTooLong => Error.Validation("Title cannot exceed 200 characters", "Title");

    public static Error DescriptionTooLong =>
        Error.Validation("Description cannot exceed 1000 characters", "Description");

    public static Error DueDateInPast => Error.Validation("Due date cannot be in the past", "DueDate");
    public static Error InvalidPriority => Error.Validation("Invalid priority value", "Priority");
    public static Error InvalidStatus => Error.Validation("Invalid status value", "Status");

    // Task business logic errors
    public static Error CannotUpdateCompletedTask => Error.Validation("Cannot update a completed task", "Status");
    public static Error CannotDeleteCompletedTask => Error.Validation("Cannot delete a completed task", "Status");
    public static Error CannotAcceptPassedDueDateTask => Error.Validation("Cannot accept a passed due date task", "Status");
    public static Error CannotRejectPassedDueDateTask => Error.Validation("Cannot reject a passed due date task", "Status");
    public static Error TaskAlreadyCompleted => Error.Conflict("Task is already completed", "Status");
    public static Error TaskNotAssigned => Error.Validation("Task must be assigned to a user", "AssignedUserId");

    // Task assignment errors
    public static Error AssignedUserNotFound => Error.NotFound("Assigned user", "AssignedUserId");
    public static Error AssignedUserInactive => Error.Validation("Assigned user is inactive", "AssignedUserId");
    public static Error CannotAssignToSelf => Error.Validation("Cannot assign task to yourself", "AssignedUserId");

    public static Error AssignerMustBeManagerOfAssignee =>
        Error.Forbidden("You must be a manager of the assigned employee to assign tasks to them");

    // Task pagination errors
    public static Error InvalidPageNumber => Error.Validation("Page must be greater than 0", "Page");
    public static Error InvalidPageSize => Error.Validation("Page size must be between 1 and 100", "PageSize");

    public static Error InvalidDateRange =>
        Error.Validation("Due date from cannot be greater than due date to", "DueDateFrom");

    // Task creation errors
    public static Error CreatedByRequired => Error.Validation("Created by user is required", "CreatedBy");
    public static Error CreatedByNotFound => Error.NotFound("Created by user", "CreatedBy");
    public static Error CreatedByInactive => Error.Validation("Created by user is inactive", "CreatedBy");

    // Task update errors
    public static Error CannotUpdateOtherUserTask => Error.Forbidden("Cannot update task created by another user");
    public static Error CannotDeleteOtherUserTask => Error.Forbidden("Cannot delete task created by another user");
    public static Error TaskUpdateConflict => Error.Conflict("Task was modified by another user", "Id");
    public static Error CannotCancelReviewedTask =>
        Error.Conflict("Task has already been reviewed or completed and cannot be cancelled", "Status");

    public static Error TaskAlreadyCancelled =>
        Error.Validation("Task is already cancelled", "Status");

    // Task query errors
    public static Error InvalidTaskId => Error.Validation("Task ID is required", "Id");
    public static Error InvalidUserId => Error.Validation("User ID is required", "UserId");
    public static Error InvalidFilterParameters => Error.Validation("Invalid filter parameters provided", "Filters");

    // Task access control errors
    public static Error AccessDenied =>
        Error.Forbidden(
            "You do not have access to this task. Tasks can only be accessed by the creator, assigned user, or users in the assignment chain.");

    public static Error NotFoundById(Guid id) => Error.NotFound($"Task with ID '{id}'", "Id");

    // Task attachment errors
    public static Error FileSizeExceeded => Error.Validation("File size exceeds the maximum allowed size", "File");
    public static Error FileUploadFailed => Error.Internal("Failed to upload file");
    public static Error FileNotFound => Error.NotFound("File attachment", "AttachmentId");
    public static Error UnauthorizedFileAccess => Error.Forbidden("You do not have permission to access this file attachment");
    public static Error InvalidFileName => Error.Validation("File name is invalid", "FileName");
    public static Error AttachmentNotFound => Error.NotFound("Attachment", "AttachmentId");
}