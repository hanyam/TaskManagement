using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;

namespace TaskManagement.Application.Tasks.Queries.GetTaskProgressHistory;

/// <summary>
///     Handler for getting task progress history.
///     Follows CQRS pattern: Queries use Dapper only (no EF Core).
/// </summary>
public class GetTaskProgressHistoryQueryHandler(TaskDapperRepository taskRepository) : IRequestHandler<GetTaskProgressHistoryQuery, List<TaskProgressDto>>
{
    private readonly TaskDapperRepository _taskRepository = taskRepository;

    public async Task<Result<List<TaskProgressDto>>> Handle(GetTaskProgressHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate pagination
        if (request.Page < 1) errors.Add(TaskErrors.InvalidPageNumber);

        if (request.PageSize < 1 || request.PageSize > 100) errors.Add(TaskErrors.InvalidPageSize);

        // Validate task exists using Dapper
        var taskExists = await _taskRepository.TaskExistsAsync(request.TaskId, cancellationToken);
        if (!taskExists) errors.Add(TaskErrors.NotFoundById(request.TaskId));

        if (errors.Any()) return Result<List<TaskProgressDto>>.Failure(errors);

        // Get progress history using Dapper (optimized query)
        var progressHistory = await _taskRepository.GetTaskProgressHistoryAsync(
            request.TaskId,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result<List<TaskProgressDto>>.Success(progressHistory.ToList());
    }
}