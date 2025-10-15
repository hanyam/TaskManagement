using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Errors.Tasks;

namespace TaskManagement.Application.Tasks.Queries.GetTaskById;

/// <summary>
///     Handler for getting a task by its ID using Dapper for optimized querying.
/// </summary>
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly TaskDapperRepository _taskRepository;

    public GetTaskByIdQueryHandler(TaskDapperRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate input
        if (request.Id == Guid.Empty)
        {
            errors.Add(TaskErrors.InvalidTaskId);
        }

        // If there are any validation errors, return them all
        if (errors.Any())
        {
            return Result<TaskDto>.Failure(errors);
        }

        var taskDto = await _taskRepository.GetTaskWithUserAsync(request.Id, cancellationToken);

        if (taskDto == null)
        {
            errors.Add(TaskErrors.NotFound);
            return Result<TaskDto>.Failure(errors);
        }

        return taskDto;
    }
}