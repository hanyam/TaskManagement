using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Domain.DTOs;

/// <summary>
///     Data Transfer Object for Task entity.
/// </summary>
public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? OriginalDueDate { get; set; }
    public DateTime? ExtendedDueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserEmail { get; set; }
    public string? AssignedUserDisplayName { get; set; }
    public TaskType Type { get; set; }
    public ReminderLevel ReminderLevel { get; set; }
    public int? ProgressPercentage { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? ManagerRating { get; set; }
    public string? ManagerFeedback { get; set; }
    public bool IsManager { get; set; } // Indicates if the current user is the creator (manager) of this task
    public Guid CurrentUserId { get; set; } // Current user's ID from backend (for impersonation support)
    public List<TaskAssignmentDto> Assignments { get; set; } = new();
    public List<TaskProgressDto> RecentProgressHistory { get; set; } = new();
}

/// <summary>
///     Request DTO for creating a new task.
/// </summary>
public record CreateTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;
    public DateTime? DueDate { get; init; }
    public Guid? AssignedUserId { get; init; }
    public TaskType Type { get; init; } = TaskType.Simple;
}

/// <summary>
///     Request DTO for updating a task.
/// </summary>
public record UpdateTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssignedUserId { get; init; }
}

/// <summary>
///     Request DTO for updating task status.
/// </summary>
public record UpdateTaskStatusRequest
{
    public TaskStatus Status { get; init; }
}

/// <summary>
///     Request DTO for filtering tasks.
/// </summary>
public record GetTasksRequest
{
    public TaskStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public Guid? AssignedUserId { get; init; }
    public DateTime? DueDateFrom { get; init; }
    public DateTime? DueDateTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string Filter { get; init; } = "created";
}

/// <summary>
///     Data Transfer Object for task progress information.
/// </summary>
public class TaskProgressDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UpdatedById { get; set; }
    public string? UpdatedByEmail { get; set; }
    public int ProgressPercentage { get; set; }
    public string? Notes { get; set; }
    public ProgressStatus Status { get; set; }
    public Guid? AcceptedById { get; set; }
    public string? AcceptedByEmail { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
///     Data Transfer Object for task assignment.
/// </summary>
public class TaskAssignmentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserDisplayName { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime AssignedAt { get; set; } // Maps to CreatedAt from BaseEntity
}

/// <summary>
///     Data Transfer Object for dashboard statistics.
/// </summary>
public class DashboardStatsDto
{
    public int TasksCreatedByUser { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksNearDueDate { get; set; }
    public int TasksDelayed { get; set; }
    public int TasksInProgress { get; set; }
    public int TasksUnderReview { get; set; }
    public int TasksPendingAcceptance { get; set; }
}

/// <summary>
///     Data Transfer Object for delegation information.
/// </summary>
public class DelegationDto
{
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public List<TaskAssignmentDto> Assignments { get; set; } = new();
    public DateTime DelegatedAt { get; set; }
    public Guid DelegatedById { get; set; }
    public string? DelegatedByEmail { get; set; }
}

/// <summary>
///     Data Transfer Object for deadline extension request.
/// </summary>
public class ExtensionRequestDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public Guid RequestedById { get; set; }
    public string? RequestedByEmail { get; set; }
    public DateTime RequestedDueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public ExtensionRequestStatus Status { get; set; }
    public Guid? ReviewedById { get; set; }
    public string? ReviewedByEmail { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
///     Request DTO for assigning a task.
/// </summary>
public record AssignTaskRequest
{
    public List<Guid> UserIds { get; init; } = new();
}

/// <summary>
///     Request DTO for updating task progress.
/// </summary>
public record UpdateTaskProgressRequest
{
    public int ProgressPercentage { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
///     Request DTO for accepting task progress.
/// </summary>
public record AcceptTaskProgressRequest
{
    public Guid ProgressHistoryId { get; init; }
}

/// <summary>
///     Request DTO for rejecting task progress.
/// </summary>
public record RejectTaskProgressRequest
{
    public Guid ProgressHistoryId { get; init; }
}

/// <summary>
///     Request DTO for rejecting a task.
/// </summary>
public record RejectTaskRequest
{
    public string? Reason { get; init; }
}

/// <summary>
///     Request DTO for requesting more information.
/// </summary>
public record RequestMoreInfoRequest
{
    public string RequestMessage { get; init; } = string.Empty;
}

/// <summary>
///     Request DTO for reassigning a task.
/// </summary>
public record ReassignTaskRequest
{
    public List<Guid> NewUserIds { get; init; } = new();
}

/// <summary>
///     Request DTO for requesting deadline extension.
/// </summary>
public record RequestDeadlineExtensionRequest
{
    public DateTime RequestedDueDate { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
///     Request DTO for approving extension request.
/// </summary>
public record ApproveExtensionRequestRequest
{
    public string? ReviewNotes { get; init; }
}