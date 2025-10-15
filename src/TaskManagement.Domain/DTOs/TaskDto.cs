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
    public Guid AssignedUserId { get; set; }
    public string? AssignedUserEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
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
    public Guid AssignedUserId { get; set; }
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
    public Guid AssignedUserId { get; set; }
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