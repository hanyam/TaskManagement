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
    public TaskType Type { get; set; }
    public ReminderLevel ReminderLevel { get; set; }
    public int? ProgressPercentage { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TaskAssignmentDto> Assignments { get; set; } = new();
    public List<TaskProgressDto> RecentProgressHistory { get; set; } = new();
}

/// <summary>
///     Request DTO for creating a new task.
/// </summary>
public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
    public TaskType Type { get; set; } = TaskType.Simple;
}

/// <summary>
///     Request DTO for updating a task.
/// </summary>
public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
}

/// <summary>
///     Request DTO for updating task status.
/// </summary>
public class UpdateTaskStatusRequest
{
    public TaskStatus Status { get; set; }
}

/// <summary>
///     Request DTO for filtering tasks.
/// </summary>
public class TaskFilterRequest
{
    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? AssignedUserId { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
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
public class AssignTaskRequest
{
    public List<Guid> UserIds { get; set; } = new();
}

/// <summary>
///     Request DTO for updating task progress.
/// </summary>
public class UpdateTaskProgressRequest
{
    public int ProgressPercentage { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
///     Request DTO for accepting task progress.
/// </summary>
public class AcceptTaskProgressRequest
{
    public Guid ProgressHistoryId { get; set; }
}

/// <summary>
///     Request DTO for rejecting a task.
/// </summary>
public class RejectTaskRequest
{
    public string? Reason { get; set; }
}

/// <summary>
///     Request DTO for requesting more information.
/// </summary>
public class RequestMoreInfoRequest
{
    public string RequestMessage { get; set; } = string.Empty;
}

/// <summary>
///     Request DTO for reassigning a task.
/// </summary>
public class ReassignTaskRequest
{
    public List<Guid> NewUserIds { get; set; } = new();
}

/// <summary>
///     Request DTO for requesting deadline extension.
/// </summary>
public class RequestDeadlineExtensionRequest
{
    public DateTime RequestedDueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
///     Request DTO for approving extension request.
/// </summary>
public class ApproveExtensionRequestRequest
{
    public string? ReviewNotes { get; set; }
}