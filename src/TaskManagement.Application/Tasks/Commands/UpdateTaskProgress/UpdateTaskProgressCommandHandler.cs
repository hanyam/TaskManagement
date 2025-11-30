using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
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
public class UpdateTaskProgressCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    UserDapperRepository userQueryRepository,
    TaskManagementDbContext context) : ICommandHandler<UpdateTaskProgressCommand, TaskProgressDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly UserDapperRepository _userQueryRepository = userQueryRepository;

    public async Task<Result<TaskProgressDto>> Handle(UpdateTaskProgressCommand request,
        CancellationToken cancellationToken)
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
            errors.Add(Error.Validation("Progress percentage must be between 0 and 100", "ProgressPercentage", "Errors.Tasks.ProgressPercentageInvalid"));
        }

        // Check if task type supports progress
        if (task.Type == TaskType.Simple && request.ProgressPercentage > 0)
        {
            errors.Add(Error.Validation("Simple tasks cannot have progress tracking", "Type", "Errors.Tasks.SimpleTaskNoProgress"));
        }

        // Validate that new progress is not less than the last approved progress
        // Get the most recent accepted progress entry using repository
        var lastAcceptedProgress = await _taskCommandRepository.GetLastAcceptedProgressAsync(request.TaskId, cancellationToken);

        // Determine the minimum allowed progress
        // If there's a last accepted progress, use that; otherwise use current task progress or 0
        var minAllowedProgress = lastAcceptedProgress?.ProgressPercentage ?? task.ProgressPercentage ?? 0;

        // Validate that new progress is not less than the minimum allowed
        if (request.ProgressPercentage < minAllowedProgress)
        {
            errors.Add(Error.Validation(
                $"Progress must be at least {minAllowedProgress}% (last approved progress). You can only increase the progress.",
                "ProgressPercentage", "Errors.Tasks.ProgressMinNotMet"));
        }

        // Determine if progress requires acceptance
        var requiresAcceptance = task.Type == TaskType.WithAcceptedProgress;

        // Update task progress (this may throw exceptions)
        try
        {
            task.UpdateProgress(request.ProgressPercentage, requiresAcceptance);
            task.SetUpdatedBy(request.UpdatedById.ToString());
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Progress"));
        }

        // Check all errors once before any database operations
        if (errors.Any())
        {
            return Result<TaskProgressDto>.Failure(errors);
        }

        // All validations passed - proceed with database operations
        // Create progress history entry
        var progressHistory = new TaskProgressHistory(
            request.TaskId,
            request.UpdatedById,
            request.ProgressPercentage,
            request.Notes);

        progressHistory.SetCreatedBy(request.UpdatedById.ToString());
        
        // If progress doesn't require approval, automatically accept it
        if (!requiresAcceptance)
        {
            progressHistory.Accept(request.UpdatedById);
        }
        
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