using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a history entry for task progress updates.
/// </summary>
public class TaskProgressHistory : BaseEntity
{
    private TaskProgressHistory()
    {
    }

    public TaskProgressHistory(Guid taskId, Guid updatedById, int progressPercentage, string? notes = null)
    {
        TaskId = taskId;
        UpdatedById = updatedById;
        ProgressPercentage = progressPercentage;
        Notes = notes;
        Status = ProgressStatus.Pending;
        // Note: CreatedAt from BaseEntity represents when this progress entry was created
        // UpdatedAt will be set via SetUpdatedBy() if the entry is later modified
    }

    public Guid TaskId { get; private set; }
    public Guid UpdatedById { get; private set; }
    public int ProgressPercentage { get; private set; }
    public string? Notes { get; private set; }
    public ProgressStatus Status { get; private set; }
    public Guid? AcceptedById { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    // Navigation properties
    public Task? Task { get; private set; }
    public User? UpdatedByUser { get; private set; }
    public User? AcceptedByUser { get; private set; }

    public void Accept(Guid acceptedById)
    {
        Status = ProgressStatus.Accepted;
        AcceptedById = acceptedById;
        AcceptedAt = DateTime.UtcNow;
    }

    public void Reject(Guid rejectedById)
    {
        Status = ProgressStatus.Rejected;
        AcceptedById = rejectedById;
        AcceptedAt = DateTime.UtcNow;
    }
}

/// <summary>
///     Represents the status of a progress update.
/// </summary>
public enum ProgressStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

