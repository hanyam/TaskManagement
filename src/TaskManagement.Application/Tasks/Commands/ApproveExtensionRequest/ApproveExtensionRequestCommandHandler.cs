using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Data.Repositories;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Application.Tasks.Commands.ApproveExtensionRequest;

/// <summary>
///     Handler for approving a deadline extension request (manager).
/// </summary>
public class ApproveExtensionRequestCommandHandler(
    TaskEfCommandRepository taskCommandRepository,
    TaskManagementDbContext context) : ICommandHandler<ApproveExtensionRequestCommand>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;

    public async Task<Result> Handle(ApproveExtensionRequestCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result.Failure(errors);
        }

        // Get extension request
        var extensionRequest = await _context.Set<DeadlineExtensionRequest>()
            .FirstOrDefaultAsync(er => er.Id == request.ExtensionRequestId && er.TaskId == request.TaskId,
                cancellationToken);

        if (extensionRequest == null)
        {
            errors.Add(Error.NotFound("Extension request", "ExtensionRequestId", "Errors.Tasks.ExtensionRequestNotFound"));
            return Result.Failure(errors);
        }

        if (extensionRequest.Status != ExtensionRequestStatus.Pending)
        {
            errors.Add(Error.Validation("Extension request has already been processed", "Status", "Errors.Tasks.ExtensionRequestAlreadyProcessed"));
        }

        // Approve extension request (may throw exceptions)
        try
        {
            extensionRequest.Approve(request.ApprovedById, request.ReviewNotes);
            extensionRequest.SetUpdatedBy(request.ApprovedById.ToString());

            // Apply extension to task
            task.ExtendDeadline(extensionRequest.RequestedDueDate, extensionRequest.Reason);
            task.SetUpdatedBy(request.ApprovedById.ToString());
        }
        catch (Exception ex)
        {
            errors.Add(Error.Validation(ex.Message, "Extension"));
        }

        // Check all errors once before database operations
        if (errors.Any())
        {
            return Result.Failure(errors);
        }

        _context.Set<DeadlineExtensionRequest>().Update(extensionRequest);
        await _taskCommandRepository.UpdateAsync(task, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}