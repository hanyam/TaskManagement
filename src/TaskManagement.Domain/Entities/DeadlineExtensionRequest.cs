using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a request for extending a task deadline.
/// </summary>
public class DeadlineExtensionRequest : BaseEntity
{
    private DeadlineExtensionRequest()
    {
    }

    public DeadlineExtensionRequest(Guid taskId, Guid requestedById, DateTime requestedDueDate, string reason)
    {
        TaskId = taskId;
        RequestedById = requestedById;
        RequestedDueDate = requestedDueDate;
        Reason = reason;
        Status = ExtensionRequestStatus.Pending;
    }

    public Guid TaskId { get; private set; }
    public Guid RequestedById { get; private set; }
    public DateTime RequestedDueDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ExtensionRequestStatus Status { get; private set; }
    public Guid? ReviewedById { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewNotes { get; private set; }

    // Navigation properties
    public Task? Task { get; private set; }
    public User? RequestedBy { get; private set; }
    public User? ReviewedBy { get; private set; }

    public void Approve(Guid reviewedById, string? reviewNotes = null)
    {
        Status = ExtensionRequestStatus.Approved;
        ReviewedById = reviewedById;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = reviewNotes;
    }

    public void Reject(Guid reviewedById, string? reviewNotes = null)
    {
        Status = ExtensionRequestStatus.Rejected;
        ReviewedById = reviewedById;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = reviewNotes;
    }
}

