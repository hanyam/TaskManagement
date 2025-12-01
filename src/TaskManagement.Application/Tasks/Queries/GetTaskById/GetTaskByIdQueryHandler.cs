using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Application.Tasks.Queries.GetTaskById;

/// <summary>
///     Handler for getting a task by its ID using Dapper for optimized querying.
///     Includes authorization check to ensure user has access to the task.
///     Follows CQRS pattern: Queries use Dapper only (no EF Core).
/// </summary>
public class GetTaskByIdQueryHandler(TaskDapperRepository taskRepository) : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly TaskDapperRepository _taskRepository = taskRepository;

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate input
        if (request.Id == Guid.Empty) errors.Add(TaskErrors.InvalidTaskId);

        if (request.UserId == Guid.Empty) errors.Add(TaskErrors.InvalidUserId);

        // If there are any validation errors, return them all
        if (errors.Any()) return Result<TaskDto>.Failure(errors);

        // Get task using Dapper (optimized query)
        var taskDto = await _taskRepository.GetTaskWithUserAsync(request.Id, cancellationToken);

        if (taskDto == null)
        {
            errors.Add(TaskErrors.NotFound);
            return Result<TaskDto>.Failure(errors);
        }

        // Authorization check: Verify user has access to this task using Dapper
        // User can access task if:
        // 1. User created the task (CreatedById)
        // 2. User is assigned to the task (AssignedUserId)
        // 3. User is in the assignment chain (TaskAssignments)
        var hasAccess = await _taskRepository.HasUserAccessToTaskAsync(request.Id, request.UserId, cancellationToken);

        if (!hasAccess)
        {
            errors.Add(TaskErrors.AccessDenied);
            return Result<TaskDto>.Failure(errors);
        }

        // Set IsManager property: true if current user is the creator of the task
        taskDto.IsManager = taskDto.CreatedById == request.UserId;

        // Set CurrentUserId property: current user's ID from backend (supports impersonation)
        taskDto.CurrentUserId = request.UserId;

        // Populate RecentProgressHistory - get the most recent 10 entries (includes pending entries needed for progress approval)
        var progressHistory = await _taskRepository.GetTaskProgressHistoryAsync(
            request.Id,
            1,
            10,
            cancellationToken);
        taskDto.RecentProgressHistory = progressHistory.ToList();

        return Result<TaskDto>.Success(taskDto);
    }
}