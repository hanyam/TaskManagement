using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Queries.GetAssignedTasks;

/// <summary>
///     Handler for getting tasks assigned to a user.
/// </summary>
public class GetAssignedTasksQueryHandler : IRequestHandler<GetAssignedTasksQuery, GetTasksResponse>
{
    private readonly ApplicationDbContext _context;

    public GetAssignedTasksQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetTasksResponse>> Handle(GetAssignedTasksQuery request, CancellationToken cancellationToken)
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
            .Where(t => t.AssignedUserId == request.UserId ||
                       t.Assignments.Any(a => a.UserId == request.UserId));

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
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
            .ToListAsync(cancellationToken);

        return new GetTasksResponse
        {
            Tasks = tasks,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

