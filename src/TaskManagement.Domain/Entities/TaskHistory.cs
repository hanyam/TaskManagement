using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a history entry for task status changes and key events.
/// </summary>
public class TaskHistory : BaseEntity
{
    private TaskHistory()
    {
    }

    public TaskHistory(
        Guid taskId,
        TaskStatus fromStatus,
        TaskStatus toStatus,
        string action,
        Guid performedById,
        string? notes = null)
    {
        TaskId = taskId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Action = action;
        PerformedById = performedById;
        Notes = notes;
    }

    public Guid TaskId { get; private set; }
    public TaskStatus FromStatus { get; private set; }
    public TaskStatus ToStatus { get; private set; }
    public string Action { get; private set; } = string.Empty; // e.g., "Created", "Assigned", "Accepted", "Rejected", "Completed", "Reviewed"
    public Guid PerformedById { get; private set; }
    public string? Notes { get; private set; } // Optional notes about the action

    // Navigation properties
    public Task? Task { get; private set; }
    public User? PerformedByUser { get; private set; }
}


