using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Application.Tasks.Queries.GetAssignedTasks;

/// <summary>
///     Handler for getting tasks assigned to a user.
///     Follows CQRS pattern: Queries use Dapper only (no EF Core).
/// </summary>
public class GetAssignedTasksQueryHandler(TaskDapperRepository taskRepository)
    : IRequestHandler<GetAssignedTasksQuery, GetTasksResponse>
{
    private readonly TaskDapperRepository _taskRepository = taskRepository;

    public async Task<Result<GetTasksResponse>> Handle(GetAssignedTasksQuery request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination
        if (request.Page < 1) errors.Add(TaskErrors.InvalidPageNumber);

        if (request.PageSize < 1 || request.PageSize > 100) errors.Add(TaskErrors.InvalidPageSize);

        if (errors.Any()) return Result<GetTasksResponse>.Failure(errors);

        // Get tasks using Dapper (optimized query)
        var (tasks, totalCount) = await _taskRepository.GetAssignedTasksAsync(
            request.UserId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result<GetTasksResponse>.Success(new GetTasksResponse
        {
            Tasks = tasks.ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}