using TaskManagement.Domain.Entities;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Interface for Task EF Core command repository.
/// </summary>
public interface ITaskEfCommandRepository : ICommandRepository<Task>
{
    /// <summary>
    ///     Gets the last accepted progress history entry for a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The last accepted progress history entry, or null if none exists.</returns>
    Task<TaskProgressHistory?> GetLastAcceptedProgressAsync(Guid taskId, CancellationToken cancellationToken = default);
}








