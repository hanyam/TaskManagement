using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using DomainTask = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Infrastructure.Data.Repositories;

/// <summary>
///     Specialized EF Core command repository for Task entities.
/// </summary>
public class TaskEfCommandRepository(TaskManagementDbContext context)
    : EfCommandRepository<DomainTask>(context), ITaskEfCommandRepository
{
    /// <summary>
    ///     Gets the last accepted progress history entry for a task.
    /// </summary>
    public async Task<TaskProgressHistory?> GetLastAcceptedProgressAsync(Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<TaskProgressHistory>()
            .Where(ph => ph.TaskId == taskId && ph.Status == ProgressStatus.Accepted && ph.AcceptedAt != null)
            .OrderByDescending(ph => ph.AcceptedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}