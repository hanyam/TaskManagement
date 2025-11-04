using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;

/// <summary>
///     Handler for updating task progress (employee).
/// </summary>
public class UpdateTaskProgressCommandHandler : ICommandHandler<UpdateTaskProgressCommand, TaskProgressDto>
{
    private readonly TaskManagementDbContext _context;
    private readonly TaskEfCommandRepository _taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository;

    public UpdateTaskProgressCommandHandler(
        TaskEfCommandRepository taskCommandRepository,
        UserDapperRepository userQueryRepository,
        TaskManagementDbContext context)
    {
        _taskCommandRepository = taskCommandRepository;
        _userQueryRepository = userQueryRepository;
        _context = context;
    }

    public async Task<Result<TaskProgressDto>> Handle(UpdateTaskProgressCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result<TaskProgressDto>.Failure(errors);
        }

        // Validate progress percentage
        if (request.ProgressPercentage < 0 || request.ProgressPercentage > 100)
        {
            errors.Add(Error.Validation("Progress percentage must be between 0 and 100", "ProgressPercentage"));
            return Result<TaskProgressDto>.Failure(errors);
        }

        // Check if task type supports progress
        if (task.Type == TaskType.Simple && request.ProgressPercentage > 0)
        {
            errors.Add(Error.Validation("Simple tasks cannot have progress tracking", "Type"));
            return Result<TaskProgressDto>.Failure(errors);
        }

        // Determine if progress requires acceptance
        var requiresAcceptance = task.Type == TaskType.WithAcceptedProgress;

        // Update task progress
        try
        {
            task.UpdateProgress(request.ProgressPercentage, requiresAcceptance);
            task.SetUpdatedBy(request.UpdatedById.ToString());
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Progress"));
            return Result<TaskProgressDto>.Failure(errors);
        }

        // Create progress history entry
        var progressHistory = new TaskProgressHistory(
            request.TaskId,
            request.UpdatedById,
            request.ProgressPercentage,
            request.Notes);

        progressHistory.SetCreatedBy(request.UpdatedById.ToString());
        await _context.Set<TaskProgressHistory>().AddAsync(progressHistory, cancellationToken);

        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Get user information
        var updatedByUser = await _userQueryRepository.GetByIdAsync(request.UpdatedById, cancellationToken);

        return new TaskProgressDto
        {
            Id = progressHistory.Id,
            TaskId = progressHistory.TaskId,
            UpdatedById = progressHistory.UpdatedById,
            UpdatedByEmail = updatedByUser?.Email,
            ProgressPercentage = progressHistory.ProgressPercentage,
            Notes = progressHistory.Notes,
            Status = progressHistory.Status,
            UpdatedAt = progressHistory.UpdatedAt ?? progressHistory.CreatedAt
        };
    }
}

