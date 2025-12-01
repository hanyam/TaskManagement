using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskAssignment = TaskManagement.Domain.Entities.TaskAssignment;
using TaskProgressHistory = TaskManagement.Domain.Entities.TaskProgressHistory;
using DeadlineExtensionRequest = TaskManagement.Domain.Entities.DeadlineExtensionRequest;
using ExtensionRequestStatus = TaskManagement.Domain.Entities.ExtensionRequestStatus;
using DomainTaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Tests.Unit.TestHelpers;

/// <summary>
///     Wrapper that implements the same interface as TaskDapperRepository but uses EF Core internally.
///     This allows the original query handlers to work with EF Core in tests.
/// </summary>
public class TaskDapperRepositoryWrapper : TaskDapperRepository
{
    private readonly TaskManagementDbContext _context;
    private readonly TaskEfQueryRepository _efRepository;

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
            AssignedUserDisplayName = t.AssignedUser?.DisplayName,
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

    public override async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var nearDueDateThreshold = now.AddDays(3);

        // Use EF Core to calculate stats (for testing purposes)
        var tasksCreatedByUser = await _context.Tasks
            .CountAsync(t => t.CreatedById == userId, cancellationToken);

        var tasksCompleted = await _context.Tasks
            .CountAsync(t => t.Status == DomainTaskStatus.Completed &&
                             (t.AssignedUserId == userId || t.CreatedById == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)),
                cancellationToken);

        var tasksNearDueDate = await _context.Tasks
            .CountAsync(t => t.DueDate.HasValue &&
                             t.DueDate.Value >= now &&
                             t.DueDate.Value <= nearDueDateThreshold &&
                             t.Status != DomainTaskStatus.Completed &&
                             t.Status != DomainTaskStatus.Cancelled &&
                             (t.AssignedUserId == userId || t.CreatedById == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)),
                cancellationToken);

        var tasksDelayed = await _context.Tasks
            .CountAsync(t => t.DueDate.HasValue &&
                             t.DueDate.Value < now &&
                             t.Status != DomainTaskStatus.Completed &&
                             t.Status != DomainTaskStatus.Cancelled &&
                             (t.AssignedUserId == userId || t.CreatedById == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)),
                cancellationToken);

        var tasksInProgress = await _context.Tasks
            .CountAsync(t => (t.Status == DomainTaskStatus.Assigned || t.Status == DomainTaskStatus.Accepted) &&
                             (t.AssignedUserId == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)),
                cancellationToken);

        var tasksUnderReview = await _context.Tasks
            .CountAsync(t =>
                (t.Status == DomainTaskStatus.UnderReview || t.Status == DomainTaskStatus.PendingManagerReview) &&
                (t.AssignedUserId == userId ||
                 _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId) ||
                 t.CreatedById == userId), cancellationToken);

        var tasksPendingAcceptance = await _context.Tasks
            .CountAsync(t => t.Status == DomainTaskStatus.Created &&
                             (t.AssignedUserId == userId ||
                              _context.Set<TaskAssignment>().Any(a => a.TaskId == t.Id && a.UserId == userId)),
                cancellationToken);

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

    public override async Task<bool> HasUserAccessToTaskAsync(Guid taskId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Use EF Core for testing purposes
        var task = await _efRepository.GetByIdAsync(taskId, cancellationToken);
        if (task == null) return false;

        // Check if user created the task
        if (task.CreatedById == userId) return true;

        // Check if user is assigned to the task
        if (task.AssignedUserId.HasValue && task.AssignedUserId.Value == userId) return true;

        // Check if user is in the assignment chain
        var isInAssignmentChain = await _context.Set<TaskAssignment>()
            .AnyAsync(ta => ta.TaskId == taskId && ta.UserId == userId, cancellationToken);

        return isInAssignmentChain;
    }

    public override async Task<(IEnumerable<TaskDto> Tasks, int TotalCount)> GetAssignedTasksAsync(
        Guid userId,
        DomainTaskStatus? status = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assignments)
            .Where(t => t.AssignedUserId == userId ||
                        t.Assignments.Any(a => a.UserId == userId));

        if (status.HasValue) query = query.Where(t => t.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                OriginalDueDate = t.OriginalDueDate,
                ExtendedDueDate = t.ExtendedDueDate,
                AssignedUserId = t.AssignedUserId,
                AssignedUserEmail = t.AssignedUser != null ? t.AssignedUser.Email : null,
                Type = t.Type,
                ReminderLevel = t.ReminderLevel,
                ProgressPercentage = t.ProgressPercentage,
                CreatedById = t.CreatedById,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return (tasks, totalCount);
    }

    public override async Task<IEnumerable<TaskProgressDto>> GetTaskProgressHistoryAsync(
        Guid taskId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var progressHistory = await _context.Set<TaskProgressHistory>()
            .Include(ph => ph.UpdatedByUser)
            .Include(ph => ph.AcceptedByUser)
            .Where(ph => ph.TaskId == taskId)
            .OrderByDescending(ph => ph.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ph => new TaskProgressDto
            {
                Id = ph.Id,
                TaskId = ph.TaskId,
                UpdatedById = ph.UpdatedById,
                UpdatedByEmail = ph.UpdatedByUser != null ? ph.UpdatedByUser.Email : null,
                ProgressPercentage = ph.ProgressPercentage,
                Notes = ph.Notes,
                Status = ph.Status,
                AcceptedById = ph.AcceptedById,
                AcceptedByEmail = ph.AcceptedByUser != null ? ph.AcceptedByUser.Email : null,
                AcceptedAt = ph.AcceptedAt,
                UpdatedAt = ph.UpdatedAt ?? ph.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return progressHistory;
    }

    public override async Task<bool> TaskExistsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks.AnyAsync(t => t.Id == taskId, cancellationToken);
    }

    public override async Task<IEnumerable<ExtensionRequestDto>> GetExtensionRequestsAsync(
        Guid? taskId = null,
        ExtensionRequestStatus? status = null,
        Guid? userId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<DeadlineExtensionRequest>()
            .Include(er => er.Task)
            .Include(er => er.RequestedBy)
            .Include(er => er.ReviewedBy)
            .AsQueryable();

        if (taskId.HasValue) query = query.Where(er => er.TaskId == taskId.Value);
        if (status.HasValue) query = query.Where(er => er.Status == status.Value);
        if (userId.HasValue) query = query.Where(er => er.RequestedById == userId.Value);

        var extensionRequests = await query
            .OrderByDescending(er => er.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(er => new ExtensionRequestDto
            {
                Id = er.Id,
                TaskId = er.TaskId,
                TaskTitle = er.Task != null ? er.Task.Title : string.Empty,
                RequestedById = er.RequestedById,
                RequestedByEmail = er.RequestedBy != null ? er.RequestedBy.Email : null,
                RequestedDueDate = er.RequestedDueDate,
                Reason = er.Reason,
                Status = er.Status,
                ReviewedById = er.ReviewedById,
                ReviewedByEmail = er.ReviewedBy != null ? er.ReviewedBy.Email : null,
                ReviewedAt = er.ReviewedAt,
                ReviewNotes = er.ReviewNotes,
                CreatedAt = er.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return extensionRequests;
    }

    public override async Task<IEnumerable<TaskDto>> GetTasksForReminderLevelCalculationAsync(
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .Include(t => t.AssignedUser)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(t => t.AssignedUserId == userId.Value ||
                                     t.CreatedById == userId.Value ||
                                     t.Assignments.Any(a => a.UserId == userId.Value));

        var tasks = await query
            .Where(t => t.DueDate.HasValue && t.Status != DomainTaskStatus.Completed &&
                        t.Status != DomainTaskStatus.Cancelled)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                OriginalDueDate = t.OriginalDueDate,
                ExtendedDueDate = t.ExtendedDueDate,
                AssignedUserId = t.AssignedUserId,
                AssignedUserEmail = t.AssignedUser != null ? t.AssignedUser.Email : null,
                Type = t.Type,
                ReminderLevel = t.ReminderLevel,
                ProgressPercentage = t.ProgressPercentage,
                CreatedById = t.CreatedById,
                CreatedBy = t.CreatedBy,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return tasks;
    }
}