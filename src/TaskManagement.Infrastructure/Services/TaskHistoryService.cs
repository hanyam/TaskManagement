using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Infrastructure.Services;

/// <summary>
///     Service for tracking task history.
/// </summary>
public class TaskHistoryService(TaskManagementDbContext context) : ITaskHistoryService
{
    private readonly TaskManagementDbContext _context = context;

    public async System.Threading.Tasks.Task RecordStatusChangeAsync(
        Guid taskId,
        TaskStatus fromStatus,
        TaskStatus toStatus,
        string action,
        Guid performedById,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var history = new TaskHistory(taskId, fromStatus, toStatus, action, performedById, notes);
        await _context.Set<TaskHistory>().AddAsync(history, cancellationToken);
        // Note: Don't call SaveChangesAsync here - let the caller handle it in a transaction
    }
}

