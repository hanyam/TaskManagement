using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a task in the system.
/// </summary>
public class Task : BaseEntity
{
    private Task()
    {
    }

    public Task(string title, string? description, TaskPriority priority, DateTime? dueDate, Guid assignedUserId)
    {
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        AssignedUserId = assignedUserId;
        Status = TaskStatus.Pending;
    }

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public Guid AssignedUserId { get; private set; }
    public User? AssignedUser { get; private set; }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdatePriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void UpdateDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
    }

    public void AssignToUser(Guid userId)
    {
        AssignedUserId = userId;
    }

    public void Start()
    {
        if (Status != TaskStatus.Pending)
            throw new InvalidOperationException("Only pending tasks can be started");

        Status = TaskStatus.InProgress;
    }

    public void Complete()
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Task is already completed");

        Status = TaskStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed task");

        Status = TaskStatus.Cancelled;
    }
}

/// <summary>
///     Represents the status of a task.
/// </summary>
public enum TaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>
///     Represents the priority of a task.
/// </summary>
public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}