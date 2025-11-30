using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Commands.RejectTaskProgress;

/// <summary>
///     Handler for rejecting a task progress update (manager).
/// </summary>
public class RejectTaskProgressCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    TaskManagementDbContext context) : ICommandHandler<RejectTaskProgressCommand>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;

    public async Task<Result> Handle(RejectTaskProgressCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result.Failure(errors);
        }

        // Get progress history entry
        var progressHistory = await _context.Set<TaskProgressHistory>()
            .FirstOrDefaultAsync(ph => ph.Id == request.ProgressHistoryId && ph.TaskId == request.TaskId,
                cancellationToken);

        if (progressHistory == null)
        {
            errors.Add(Error.NotFound("Progress history entry", "ProgressHistoryId", "Errors.Tasks.ProgressHistoryNotFound"));
            return Result.Failure(errors);
        }

        // Validate user is the task creator
        if (task.CreatedById != request.RejectedById)
        {
            errors.Add(Error.Forbidden("Only the task creator can reject progress updates", "Errors.Tasks.OnlyCreatorCanRejectProgress"));
        }

        // Validate task is in UnderReview status
        if (task.Status != TaskStatus.UnderReview)
        {
            errors.Add(Error.Validation("Task must be under review to reject progress", "Status", "Errors.Tasks.TaskMustBeUnderReviewToReject"));
        }

        // Validate task type supports progress approval
        if (task.Type != TaskType.WithAcceptedProgress)
        {
            errors.Add(Error.Validation("This task type does not require progress acceptance", "Type", "Errors.Tasks.TaskTypeNoProgressAcceptance"));
        }

        // Validate progress history is pending
        if (progressHistory.Status != ProgressStatus.Pending)
        {
            errors.Add(Error.Validation("Progress history entry is not pending", "ProgressHistoryId", "Errors.Tasks.ProgressHistoryNotPending"));
        }

        // Reject progress (may throw exceptions)
        try
        {
            // Find the last accepted progress history entry to revert to
            var lastAcceptedProgress = await _context.Set<TaskProgressHistory>()
                .Where(ph => ph.TaskId == request.TaskId && ph.Status == ProgressStatus.Accepted)
                .OrderByDescending(ph => ph.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            // Reject the progress history entry
            progressHistory.Reject(request.RejectedById);
            progressHistory.SetUpdatedBy(request.RejectedById.ToString());
            
            // Revert progress percentage to the last accepted value (or 0 if none exists)
            // This must be done BEFORE changing status, as AcceptProgress() validates status is UnderReview
            var revertToPercentage = lastAcceptedProgress?.ProgressPercentage ?? 0;
            task.SetProgressPercentage(revertToPercentage);
            
            // Change status back to Accepted (task remains accepted, only progress was rejected)
            // AcceptProgress() changes status from UnderReview to Accepted
            task.AcceptProgress();
            task.SetUpdatedBy(request.RejectedById.ToString());
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Progress"));
        }

        // Check all errors once before database operations
        if (errors.Any())
        {
            return Result.Failure(errors);
        }


        _context.Set<TaskProgressHistory>().Update(progressHistory);
        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

