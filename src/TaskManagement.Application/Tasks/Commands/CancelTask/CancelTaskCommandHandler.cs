using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;
using DomainTask = TaskManagement.Domain.Entities.Task;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Commands.CancelTask;

/// <summary>
///     Handler responsible for cancelling a task (soft cancel) or deleting it entirely.
/// </summary>
public class CancelTaskCommandHandler(
    TaskManagementDbContext context,
    TaskEfCommandRepository taskCommandRepository,
    IFileStorageService fileStorageService,
    ILogger<CancelTaskCommandHandler> logger,
    Domain.Interfaces.ITaskHistoryService taskHistoryService) : ICommandHandler<CancelTaskCommand>
{
    private static readonly TaskStatus[] PreAcceptanceStatuses =
    {
        TaskStatus.Created,
        TaskStatus.Assigned,
        TaskStatus.Rejected
    };

    private static readonly TaskStatus[] SoftCancelableStatuses =
    {
        TaskStatus.Accepted,
        TaskStatus.UnderReview,
        TaskStatus.PendingManagerReview
    };

    private readonly TaskManagementDbContext _context = context;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<CancelTaskCommandHandler> _logger = logger;
    private readonly TaskEfCommandRepository _taskCommandRepository = taskCommandRepository;
    private readonly Domain.Interfaces.ITaskHistoryService _taskHistoryService = taskHistoryService;

    public async Task<Result> Handle(CancelTaskCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing cancel request for task {TaskId} by user {UserId} with role {Role}",
            request.TaskId,
            request.RequestedById,
            request.RequestedByRole);

        var errors = new List<Error>();

        var task = await _context.Set<DomainTask>()
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for cancellation", request.TaskId);
            errors.Add(TaskErrors.NotFoundById(request.TaskId));
            return Result.Failure(errors);
        }

        if (!IsAuthorized(request, task))
        {
            _logger.LogWarning(
                "User {UserId} is not authorized to cancel task {TaskId}",
                request.RequestedById,
                request.TaskId);
            errors.Add(TaskErrors.CannotDeleteOtherUserTask);
        }

        if (IsTaskReviewed(task))
        {
            _logger.LogWarning(
                "Task {TaskId} has already been reviewed or completed and cannot be cancelled",
                request.TaskId);
            errors.Add(TaskErrors.CannotCancelReviewedTask);
        }

        if (task.Status == TaskStatus.Cancelled)
        {
            _logger.LogInformation("Task {TaskId} is already cancelled", request.TaskId);
            errors.Add(TaskErrors.TaskAlreadyCancelled);
        }

        // Check all errors once before database operations
        if (errors.Any())
        {
            return Result.Failure(errors);
        }

        if (PreAcceptanceStatuses.Contains(task.Status))
        {
            await DeleteTaskWithAttachments(task, cancellationToken);
            _logger.LogInformation("Task {TaskId} deleted as part of cancellation", task.Id);
            return Result.Success();
        }

        if (SoftCancelableStatuses.Contains(task.Status))
        {
            var previousStatus = task.Status;
            task.Cancel();
            task.SetUpdatedBy(request.RequestedById.ToString());

            // Record history: Task cancelled
            await _taskHistoryService.RecordStatusChangeAsync(
                task.Id,
                previousStatus,
                task.Status,
                "Cancelled",
                request.RequestedById,
                $"Task cancelled by {request.RequestedByRole}",
                cancellationToken);

            await _taskCommandRepository.UpdateAsync(task, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} was soft-cancelled (status set to Cancelled)", task.Id);
            return Result.Success();
        }

        _logger.LogWarning(
            "Task {TaskId} is in status {Status} which cannot be cancelled",
            task.Id,
            task.Status);
        return Result.Failure(TaskErrors.InvalidStatus);
    }

    private static bool IsAuthorized(CancelTaskCommand request, DomainTask task)
    {
        var isCreator = task.CreatedById == request.RequestedById;
        var role = request.RequestedByRole?.Trim() ?? string.Empty;
        var isManagerOrAdmin = role.Equals(RoleNames.Manager, StringComparison.OrdinalIgnoreCase) ||
                               role.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase);

        return isCreator || isManagerOrAdmin;
    }

    private static bool IsTaskReviewed(DomainTask task)
    {
        return task.Status is TaskStatus.Completed or TaskStatus.RejectedByManager
               || task.ManagerRating.HasValue;
    }

    private async System.Threading.Tasks.Task DeleteTaskWithAttachments(DomainTask task, CancellationToken cancellationToken)
    {
        var attachments = await _context.Set<TaskAttachment>()
            .Where(a => a.TaskId == task.Id)
            .ToListAsync(cancellationToken);

        foreach (var attachment in attachments)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(attachment.StoragePath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete file at {StoragePath} for attachment {AttachmentId} during task cancellation",
                    attachment.StoragePath,
                    attachment.Id);
            }
        }

        _context.Set<TaskAttachment>().RemoveRange(attachments);
        _context.Set<DomainTask>().Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }
}


