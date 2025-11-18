using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a task assignment to a user (junction entity for many-to-many relationship).
/// </summary>
public class TaskAssignment : BaseEntity
{
    private TaskAssignment()
    {
    }

    public TaskAssignment(Guid taskId, Guid userId, bool isPrimary)
    {
        TaskId = taskId;
        UserId = userId;
        IsPrimary = isPrimary;
        // Note: AssignedAt is the same as CreatedAt from BaseEntity
    }

    public Guid TaskId { get; private set; }
    public Guid UserId { get; private set; }

    public bool IsPrimary { get; private set; }
    // AssignedAt is now represented by CreatedAt from BaseEntity

    // Navigation properties
    public Task? Task { get; private set; }
    public User? User { get; private set; }

    public void UpdatePrimaryStatus(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}