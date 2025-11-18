using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
///     EF-based task query repository for testing purposes.
/// </summary>
public class TaskEfQueryRepository : IQueryRepository<DomainTask>
{
    private readonly TaskManagementDbContext _context;

    public TaskEfQueryRepository(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<DomainTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.AssignedUser)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.AssignedUser)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainTask>> FindAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        // For testing purposes, we'll implement common query patterns
        // In a real scenario, you'd parse the SQL and convert it to EF queries

        if (sql.Contains("WHERE") && sql.Contains("AssignedUserId"))
            // Extract AssignedUserId from parameters if available
            if (param != null)
            {
                var assignedUserId = GetPropertyValue(param, "AssignedUserId");
                if (assignedUserId is Guid userId)
                    return await _context.Tasks
                        .Include(t => t.AssignedUser)
                        .Where(t => t.AssignedUserId == userId)
                        .ToListAsync(cancellationToken);
            }

        if (sql.Contains("WHERE") && sql.Contains("Status"))
            // Extract Status from parameters if available
            if (param != null)
            {
                var status = GetPropertyValue(param, "Status");
                if (status is DomainTaskStatus taskStatus)
                    return await _context.Tasks
                        .Include(t => t.AssignedUser)
                        .Where(t => t.Status == taskStatus)
                        .ToListAsync(cancellationToken);
            }

        if (sql.Contains("WHERE") && sql.Contains("CreatedBy"))
            // Extract CreatedBy from parameters if available
            if (param != null)
            {
                var createdBy = GetPropertyValue(param, "CreatedBy");
                if (createdBy is string createdByValue)
                    return await _context.Tasks
                        .Include(t => t.AssignedUser)
                        .Where(t => t.CreatedBy == createdByValue)
                        .ToListAsync(cancellationToken);
            }

        // Default: return all tasks
        return await _context.Tasks
            .Include(t => t.AssignedUser)
            .ToListAsync(cancellationToken);
    }

    public async Task<DomainTask?> FirstOrDefaultAsync(string sql, object? param = null,
        CancellationToken cancellationToken = default)
    {
        var results = await FindAsync(sql, param, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<int> CountAsync(string sql, object? param = null, CancellationToken cancellationToken = default)
    {
        var results = await FindAsync(sql, param, cancellationToken);
        return results.Count();
    }

    // Additional helper methods for testing
    public async Task<IEnumerable<DomainTask>> GetTasksByAssignedUserAsync(Guid? assignedUserId,
        CancellationToken cancellationToken = default)
    {
        if (!assignedUserId.HasValue)
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == null)
                .ToListAsync(cancellationToken);

        return await _context.Tasks
            .Include(t => t.AssignedUser)
            .Where(t => t.AssignedUserId == assignedUserId.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainTask>> GetTasksByStatusAsync(DomainTaskStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.AssignedUser)
            .Where(t => t.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainTask>> GetTasksByCreatedByAsync(string createdBy,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.AssignedUser)
            .Where(t => t.CreatedBy == createdBy)
            .ToListAsync(cancellationToken);
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        return property?.GetValue(obj);
    }
}