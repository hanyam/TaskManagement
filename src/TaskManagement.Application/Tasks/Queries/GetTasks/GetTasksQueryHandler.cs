using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Application.Tasks.Queries.GetTasks;

/// <summary>
///     Handler for getting a list of tasks with filtering and pagination using Dapper for optimized querying.
/// </summary>
public class GetTasksQueryHandler(TaskDapperRepository taskRepository)
    : IRequestHandler<GetTasksQuery, GetTasksResponse>
{
    private readonly TaskDapperRepository _taskRepository = taskRepository;

    public async Task<Result<GetTasksResponse>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination parameters
        if (request.Page < 1) errors.Add(TaskErrors.InvalidPageNumber);

        if (request.PageSize < 1 || request.PageSize > 100) errors.Add(TaskErrors.InvalidPageSize);

        // Validate date range
        if (request.DueDateFrom.HasValue && request.DueDateTo.HasValue && request.DueDateFrom > request.DueDateTo)
            errors.Add(TaskErrors.InvalidDateRange);

        // If there are any validation errors, return them all
        if (errors.Any()) return Result<GetTasksResponse>.Failure(errors);

        var (tasks, totalCount) = await _taskRepository.GetTasksWithPaginationAsync(
            request.Status,
            request.Priority,
            request.AssignedUserId,
            request.CreatedById,
            request.DueDateFrom,
            request.DueDateTo,
            request.Page,
            request.PageSize,
            cancellationToken);

        return new GetTasksResponse
        {
            Tasks = tasks.ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}