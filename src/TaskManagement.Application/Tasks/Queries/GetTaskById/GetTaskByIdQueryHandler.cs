using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Queries.GetTaskById;

/// <summary>
///     Handler for getting a task by its ID using Dapper for optimized querying.
///     Includes authorization check to ensure user has access to the task.
/// </summary>
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly TaskDapperRepository _taskRepository;
    private readonly TaskManagementDbContext _context;

    public GetTaskByIdQueryHandler(TaskDapperRepository taskRepository, TaskManagementDbContext context)
    {
        _taskRepository = taskRepository;
        _context = context;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate input
        if (request.Id == Guid.Empty)
        {
            errors.Add(TaskErrors.InvalidTaskId);
        }

        if (request.UserId == Guid.Empty)
        {
            errors.Add(TaskErrors.InvalidUserId);
        }

        // If there are any validation errors, return them all
        if (errors.Any())
        {
            return Result<TaskDto>.Failure(errors);
        }

        // Get task using Dapper (optimized query)
        var taskDto = await _taskRepository.GetTaskWithUserAsync(request.Id, cancellationToken);

        if (taskDto == null)
        {
            errors.Add(TaskErrors.NotFound);
            return Result<TaskDto>.Failure(errors);
        }

        // Authorization check: Verify user has access to this task
        // User can access task if:
        // 1. User created the task (CreatedById)
        // 2. User is assigned to the task (AssignedUserId)
        // 3. User is in the assignment chain (TaskAssignments)
        var hasAccess = await HasUserAccessToTask(request.Id, request.UserId, taskDto, cancellationToken);

        if (!hasAccess)
        {
            errors.Add(TaskErrors.AccessDenied);
            return Result<TaskDto>.Failure(errors);
        }

        return taskDto;
    }

    /// <summary>
    ///     Checks if the user has access to the task by verifying:
    ///     1. User created the task
    ///     2. User is assigned to the task
    ///     3. User is in the assignment chain
    /// </summary>
    private async Task<bool> HasUserAccessToTask(Guid taskId, Guid userId, TaskDto taskDto, CancellationToken cancellationToken)
    {
        // Check if user created the task
        if (taskDto.CreatedById == userId)
        {
            return true;
        }

        // Check if user is assigned to the task
        if (taskDto.AssignedUserId.HasValue && taskDto.AssignedUserId.Value == userId)
        {
            return true;
        }

        // Check if user is in the assignment chain (TaskAssignments)
        var isInAssignmentChain = await _context.Set<TaskManagement.Domain.Entities.TaskAssignment>()
            .AnyAsync(ta => ta.TaskId == taskId && ta.UserId == userId, cancellationToken);

        return isInAssignmentChain;
    }
}