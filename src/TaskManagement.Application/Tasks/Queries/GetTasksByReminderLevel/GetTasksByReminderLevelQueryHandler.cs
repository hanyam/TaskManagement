using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Application.Tasks.Queries.GetTasksByReminderLevel;

/// <summary>
///     Handler for getting tasks filtered by reminder level.
///     Follows CQRS pattern: Queries use Dapper only (no EF Core).
///     Note: Reminder level calculation is done in memory as it requires business logic.
/// </summary>
public class GetTasksByReminderLevelQueryHandler(
    TaskDapperRepository taskRepository,
    IReminderCalculationService reminderCalculationService)
    : IRequestHandler<GetTasksByReminderLevelQuery, GetTasksResponse>
{
    private readonly IReminderCalculationService _reminderCalculationService = reminderCalculationService;
    private readonly TaskDapperRepository _taskRepository = taskRepository;

    public async Task<Result<GetTasksResponse>> Handle(GetTasksByReminderLevelQuery request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination
        if (request.Page < 1) errors.Add(TaskErrors.InvalidPageNumber);

        if (request.PageSize < 1 || request.PageSize > 100) errors.Add(TaskErrors.InvalidPageSize);

        if (errors.Any()) return Result<GetTasksResponse>.Failure(errors);

        // Get tasks using Dapper (optimized query)
        // Reminder level calculation requires business logic, so we filter in memory
        var tasks = await _taskRepository.GetTasksForReminderLevelCalculationAsync(
            request.UserId,
            cancellationToken);

        // Filter by calculated reminder level (business logic in memory)
        var filteredTasks = tasks
            .Where(t =>
            {
                var calculatedLevel = _reminderCalculationService.CalculateReminderLevel(t.DueDate, t.CreatedAt);
                return calculatedLevel == request.ReminderLevel;
            })
            .OrderByDescending(t => t.CreatedAt)
            .ToList();

        var totalCount = filteredTasks.Count;

        // Apply pagination
        var pagedTasks = filteredTasks
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result<GetTasksResponse>.Success(new GetTasksResponse
        {
            Tasks = pagedTasks,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}