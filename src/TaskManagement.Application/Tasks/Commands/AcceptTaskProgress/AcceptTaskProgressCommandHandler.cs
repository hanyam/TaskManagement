using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

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
        }

        // Get progress history entry
        var progressHistory = await _context.Set<TaskProgressHistory>()
            .FirstOrDefaultAsync(ph => ph.Id == request.ProgressHistoryId && ph.TaskId == request.TaskId,
                cancellationToken);

        if (progressHistory == null)
        {
            errors.Add(Error.NotFound("Progress history entry", "ProgressHistoryId"));
        }

        // Accept progress (only if both task and progress history exist)
        if (task != null && progressHistory != null)
        {
            try
            {
                progressHistory.Accept(request.AcceptedById);
                progressHistory.SetUpdatedBy(request.AcceptedById.ToString());
                task.AcceptProgress();
                task.SetUpdatedBy(request.AcceptedById.ToString());
            }
            catch (Exception ex)
            {
                errors.Add(Error.Validation(ex.Message, "Progress"));
            }
        }

        if (errors.Any())
        {
            return Result.Failure(errors);
        }

        // At this point, we know both task and progressHistory exist
        if (task == null || progressHistory == null)
        {
            return Result.Failure(errors.Any() ? errors : new List<Error> { TaskErrors.NotFound });
        }

        _context.Set<TaskProgressHistory>().Update(progressHistory);
        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}