using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Service for tracking task history.
/// </summary>
public interface ITaskHistoryService
{
    /// <summary>
    ///     Records a status change in task history.
    /// </summary>
    /// <param name="taskId">The task ID</param>
    /// <param name="fromStatus">Previous status</param>
    /// <param name="toStatus">New status</param>
    /// <param name="action">Action description (e.g., "Created", "Assigned", "Accepted")</param>
    /// <param name="performedById">User who performed the action</param>
    /// <param name="notes">Optional notes about the action</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RecordStatusChangeAsync(
        Guid taskId,
        TaskStatus fromStatus,
        TaskStatus toStatus,
        string action,
        Guid performedById,
        string? notes = null,
        CancellationToken cancellationToken = default);
}