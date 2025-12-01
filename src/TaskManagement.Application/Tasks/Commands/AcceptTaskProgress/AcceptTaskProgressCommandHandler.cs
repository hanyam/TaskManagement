using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;
using Task = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Commands.AcceptTaskProgress;

/// <summary>
///     Handler for accepting a task progress update (manager).
/// </summary>
public class AcceptTaskProgressCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    TaskManagementDbContext context) : ICommandHandler<AcceptTaskProgressCommand>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;

    public async Task<Result> Handle(AcceptTaskProgressCommand request, CancellationToken cancellationToken)
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
            errors.Add(Error.NotFound("Progress history entry", "ProgressHistoryId",
                "Errors.Tasks.ProgressHistoryNotFound"));
            return Result.Failure(errors);
        }

        // Validate user is the task creator
        if (task.CreatedById != request.AcceptedById)
            errors.Add(Error.Forbidden("Only the task creator can accept progress updates",
                "Errors.Tasks.OnlyCreatorCanAcceptProgress"));

        // Validate task is in UnderReview status
        if (task.Status != TaskStatus.UnderReview)
            errors.Add(Error.Validation("Task must be under review to accept progress", "Status",
                "Errors.Tasks.TaskMustBeUnderReview"));

        // Validate task type supports progress approval
        if (task.Type != TaskType.WithAcceptedProgress)
            errors.Add(Error.Validation("This task type does not require progress acceptance", "Type",
                "Errors.Tasks.TaskTypeNoProgressAcceptance"));

        // Validate progress history is pending
        if (progressHistory.Status != ProgressStatus.Pending)
            errors.Add(Error.Validation("Progress history entry is not pending", "ProgressHistoryId",
                "Errors.Tasks.ProgressHistoryNotPending"));

        // Accept the progress history entry (may throw exceptions)
        try
        {
            // Accept the progress history entry
            progressHistory.Accept(request.AcceptedById);
            progressHistory.SetUpdatedBy(request.AcceptedById.ToString());

            // Accept progress - this changes status from UnderReview to Accepted
            // ProgressPercentage is already set to the new value, so we keep it
            task.AcceptProgress();
            task.SetUpdatedBy(request.AcceptedById.ToString());
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Progress"));
        }

        // Check all errors once before database operations
        if (errors.Any()) return Result.Failure(errors);

        _context.Set<TaskProgressHistory>().Update(progressHistory);
        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}