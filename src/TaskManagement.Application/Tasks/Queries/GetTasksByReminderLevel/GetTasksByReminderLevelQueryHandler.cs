using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Queries.GetTasksByReminderLevel;

/// <summary>
///     Handler for getting tasks filtered by reminder level.
/// </summary>
public class GetTasksByReminderLevelQueryHandler : IRequestHandler<GetTasksByReminderLevelQuery, GetTasksResponse>
{
    private readonly TaskManagementDbContext _context;
    private readonly IReminderCalculationService _reminderCalculationService;

    public GetTasksByReminderLevelQueryHandler(
        TaskManagementDbContext context,
        IReminderCalculationService reminderCalculationService)
    {
        _context = context;
        _reminderCalculationService = reminderCalculationService;
    }

    public async Task<Result<GetTasksResponse>> Handle(GetTasksByReminderLevelQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination
        if (request.Page < 1)
        {
            errors.Add(TaskErrors.InvalidPageNumber);
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            errors.Add(TaskErrors.InvalidPageSize);
        }

        if (errors.Any())
        {
            return Result<GetTasksResponse>.Failure(errors);
        }

        var query = _context.Tasks
            .Include(t => t.AssignedUser)
            .Include(t => t.Assignments)
            .AsQueryable();

        // Filter by user if provided
        if (request.UserId.HasValue)
        {
            query = query.Where(t => t.AssignedUserId == request.UserId.Value ||
                                   t.CreatedById == request.UserId.Value ||
                                   t.Assignments.Any(a => a.UserId == request.UserId.Value));
        }

        // Get tasks and calculate reminder levels
        var tasks = await query
            .Where(t => t.DueDate.HasValue && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled)
            .ToListAsync(cancellationToken);

        // Filter by calculated reminder level
        var filteredTasks = tasks
            .Where(t =>
            {
                var calculatedLevel = _reminderCalculationService.CalculateReminderLevel(t.DueDate, t.CreatedAt);
                return calculatedLevel == request.ReminderLevel;
            })
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
            .ToList();

        var totalCount = tasks.Count(t =>
        {
            var calculatedLevel = _reminderCalculationService.CalculateReminderLevel(t.DueDate, t.CreatedAt);
            return calculatedLevel == request.ReminderLevel;
        });

        return new GetTasksResponse
        {
            Tasks = filteredTasks,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

