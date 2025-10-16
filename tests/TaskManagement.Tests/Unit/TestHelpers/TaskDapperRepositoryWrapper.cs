using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using DomainTask = TaskManagement.Domain.Entities.Task;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
/// Wrapper that implements the same interface as TaskDapperRepository but uses EF Core internally.
/// This allows the original query handlers to work with EF Core in tests.
/// </summary>
public class TaskDapperRepositoryWrapper
{
    private readonly TaskEfQueryRepository _efRepository;

    public TaskDapperRepositoryWrapper(TaskEfQueryRepository efRepository)
    {
        _efRepository = efRepository;
    }

    public async Task<TaskDto?> GetTaskWithUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _efRepository.GetByIdAsync(id, cancellationToken);
        if (task == null) return null;

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            AssignedUserId = task.AssignedUserId,
            AssignedUserEmail = task.AssignedUser?.Email,
            CreatedBy = task.CreatedBy,
            CreatedAt = task.CreatedAt,
            LastModifiedBy = task.LastModifiedBy,
            LastModifiedAt = task.LastModifiedAt
        };
    }

    public async Task<(IEnumerable<TaskDto> Tasks, int TotalCount)> GetTasksWithPaginationAsync(
        DomainTaskStatus? status,
        TaskPriority? priority,
        Guid? assignedUserId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // For testing purposes, we'll implement a simple version
        // In a real scenario, you'd need to implement the full filtering logic
        var allTasks = await _efRepository.GetAllAsync(cancellationToken);
        
        // Convert to DTOs
        var taskDtos = allTasks.Select(t => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            DueDate = t.DueDate,
            AssignedUserId = t.AssignedUserId,
            AssignedUserEmail = t.AssignedUser?.Email,
            CreatedBy = t.CreatedBy,
            CreatedAt = t.CreatedAt,
            LastModifiedBy = t.LastModifiedBy,
            LastModifiedAt = t.LastModifiedAt
        });
        
        // Apply filters
        var filteredTasks = taskDtos.AsQueryable();
        
        if (status.HasValue)
            filteredTasks = filteredTasks.Where(t => t.Status == status.Value);
        
        if (priority.HasValue)
            filteredTasks = filteredTasks.Where(t => t.Priority == priority.Value);
        
        if (assignedUserId.HasValue)
            filteredTasks = filteredTasks.Where(t => t.AssignedUserId == assignedUserId.Value);
        
        if (dueDateFrom.HasValue)
            filteredTasks = filteredTasks.Where(t => t.DueDate >= dueDateFrom.Value);
        
        if (dueDateTo.HasValue)
            filteredTasks = filteredTasks.Where(t => t.DueDate <= dueDateTo.Value);
        
        var totalCount = filteredTasks.Count();
        var pagedTasks = filteredTasks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        return (pagedTasks, totalCount);
    }
}
