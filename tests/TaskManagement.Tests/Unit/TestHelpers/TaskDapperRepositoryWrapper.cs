using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
///     Wrapper that implements the same interface as TaskDapperRepository but uses EF Core internally.
///     This allows the original query handlers to work with EF Core in tests.
/// </summary>
public class TaskDapperRepositoryWrapper : TaskDapperRepository
{
    private readonly TaskEfQueryRepository _efRepository;
    private readonly TaskManagementDbContext _context;

    public TaskDapperRepositoryWrapper(TaskEfQueryRepository efRepository, TaskManagementDbContext context)
        : base(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb;Trusted_Connection=true;"
            })
            .Build()) // Dummy config with connection string
    {
        _efRepository = efRepository;
        _context = context;
    }

    public override async Task<TaskDto?> GetTaskWithUserAsync(Guid id, CancellationToken cancellationToken = default)
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
            CreatedById = task.CreatedById,
            OriginalDueDate = task.OriginalDueDate,
            ExtendedDueDate = task.ExtendedDueDate,
            Type = task.Type,
            ReminderLevel = task.ReminderLevel,
            ProgressPercentage = task.ProgressPercentage
        };
    }

    public override async Task<(IEnumerable<TaskDto> Tasks, int TotalCount)> GetTasksWithPaginationAsync(
        DomainTaskStatus? status,
        TaskPriority? priority,
        Guid? assignedUserId,
        Guid? createdById,
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
            CreatedById = t.CreatedById,
            OriginalDueDate = t.OriginalDueDate,
            ExtendedDueDate = t.ExtendedDueDate,
            Type = t.Type,
            ReminderLevel = t.ReminderLevel,
            ProgressPercentage = t.ProgressPercentage
        });

        // Apply filters
        var filteredTasks = taskDtos.AsQueryable();

        if (status.HasValue)
            filteredTasks = filteredTasks.Where(t => t.Status == status.Value);

        if (priority.HasValue)
            filteredTasks = filteredTasks.Where(t => t.Priority == priority.Value);

        if (assignedUserId.HasValue)
            filteredTasks = filteredTasks.Where(t => t.AssignedUserId == assignedUserId.Value);

        if (createdById.HasValue)
            filteredTasks = filteredTasks.Where(t => t.CreatedById == createdById.Value);

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

    public override async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var nearDueDateThreshold = now.AddDays(3);

        // Use EF Core to calculate stats (for testing purposes)
        var tasksCreatedByUser = await _context.Tasks
            .CountAsync(t => t.CreatedById == userId, cancellationToken);

        var tasksCompleted = await _context.Tasks
            .CountAsync(t => t.Status == DomainTaskStatus.Completed &&
                             (t.AssignedUserId == userId || t.CreatedById == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)), cancellationToken);

        var tasksNearDueDate = await _context.Tasks
            .CountAsync(t => t.DueDate.HasValue &&
                             t.DueDate.Value >= now &&
                             t.DueDate.Value <= nearDueDateThreshold &&
                             t.Status != DomainTaskStatus.Completed &&
                             t.Status != DomainTaskStatus.Cancelled &&
                             (t.AssignedUserId == userId || t.CreatedById == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)), cancellationToken);

        var tasksDelayed = await _context.Tasks
            .CountAsync(t => t.DueDate.HasValue &&
                             t.DueDate.Value < now &&
                             t.Status != DomainTaskStatus.Completed &&
                             t.Status != DomainTaskStatus.Cancelled &&
                             (t.AssignedUserId == userId || t.CreatedById == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)), cancellationToken);

        var tasksInProgress = await _context.Tasks
            .CountAsync(t => (t.Status == DomainTaskStatus.Assigned || t.Status == DomainTaskStatus.Accepted) &&
                             (t.AssignedUserId == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)), cancellationToken);

        var tasksUnderReview = await _context.Tasks
            .CountAsync(t => (t.Status == DomainTaskStatus.UnderReview || t.Status == DomainTaskStatus.PendingManagerReview) &&
                             (t.AssignedUserId == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId) ||
                              t.CreatedById == userId), cancellationToken);

        var tasksPendingAcceptance = await _context.Tasks
            .CountAsync(t => t.Status == DomainTaskStatus.Created &&
                             (t.AssignedUserId == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)), cancellationToken);

        return new DashboardStatsDto
        {
            TasksCreatedByUser = tasksCreatedByUser,
            TasksCompleted = tasksCompleted,
            TasksNearDueDate = tasksNearDueDate,
            TasksDelayed = tasksDelayed,
            TasksInProgress = tasksInProgress,
            TasksUnderReview = tasksUnderReview,
            TasksPendingAcceptance = tasksPendingAcceptance
        };
    }
}