using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Tasks.Commands.DeleteTaskAttachment;

/// <summary>
///     Handler for deleting a task attachment.
/// </summary>
public class DeleteTaskAttachmentCommandHandler(
    IFileStorageService fileStorageService,
    TaskManagementDbContext context,
    ILogger<DeleteTaskAttachmentCommandHandler> logger) : ICommandHandler<DeleteTaskAttachmentCommand>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<DeleteTaskAttachmentCommandHandler> _logger = logger;

    public async Task<Result> Handle(DeleteTaskAttachmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting deletion of attachment {AttachmentId} for task {TaskId} by user {UserId}",
            request.AttachmentId,
            request.TaskId,
            request.RequestedById);

        // Find attachment
        var attachment = await _context.Set<TaskAttachment>().FindAsync(new object[] { request.AttachmentId }, cancellationToken);
        if (attachment == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found for deletion", request.AttachmentId);
            return Result.Failure(TaskErrors.AttachmentNotFound);
        }

        // Verify attachment belongs to the task
        if (attachment.TaskId != request.TaskId)
        {
            _logger.LogWarning(
                "Attachment {AttachmentId} does not belong to task {TaskId}",
                request.AttachmentId,
                request.TaskId);
            return Result.Failure(TaskErrors.AttachmentNotFound);
        }

        // Verify task exists
        var task = await _context.Set<Domain.Entities.Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for attachment deletion", request.TaskId);
            return Result.Failure(TaskErrors.NotFound);
        }

        // Access control: Only uploader or task creator can delete
        if (attachment.UploadedById != request.RequestedById && task.CreatedById != request.RequestedById)
        {
            _logger.LogWarning(
                "User {UserId} attempted to delete attachment {AttachmentId} without permission",
                request.RequestedById,
                request.AttachmentId);
            return Result.Failure(TaskErrors.UnauthorizedFileAccess);
        }

        try
        {
            // Delete file from storage
            await _fileStorageService.DeleteFileAsync(attachment.StoragePath, cancellationToken);

            // Remove from database
            _context.Set<TaskAttachment>().Remove(attachment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully deleted attachment {AttachmentId} (FileName: {FileName}) for task {TaskId}",
                request.AttachmentId,
                attachment.OriginalFileName,
                request.TaskId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete attachment {AttachmentId} for task {TaskId}",
                request.AttachmentId,
                request.TaskId);
            return Result.Failure(TaskErrors.FileUploadFailed);
        }
    }
}

