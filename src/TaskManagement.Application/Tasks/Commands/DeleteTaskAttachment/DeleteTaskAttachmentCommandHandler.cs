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

        var errors = new List<Error>();

        // Find attachment
        var attachment = await _context.Set<TaskAttachment>().FindAsync(new object[] { request.AttachmentId }, cancellationToken);
        if (attachment == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found for deletion", request.AttachmentId);
            errors.Add(TaskErrors.AttachmentNotFound);
            return Result.Failure(errors);
        }

        // Verify attachment belongs to the task
        if (attachment.TaskId != request.TaskId)
        {
            _logger.LogWarning(
                "Attachment {AttachmentId} does not belong to task {TaskId}",
                request.AttachmentId,
                request.TaskId);
            errors.Add(TaskErrors.AttachmentNotFound);
            return Result.Failure(errors);
        }

        // Verify task exists
        var task = await _context.Set<Domain.Entities.Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for attachment deletion", request.TaskId);
            errors.Add(TaskErrors.NotFound);
            return Result.Failure(errors);
        }

        // Determine user role for additional access control
        var user = await _context.Set<User>().FindAsync(new object[] { request.RequestedById }, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for attachment deletion", request.RequestedById);
            errors.Add(TaskErrors.UnauthorizedFileAccess);
            return Result.Failure(errors);
        }

        // Additional rule: employees can only delete their own EmployeeUploaded attachments
        // Employees cannot delete ManagerUploaded attachments
        if (user.Role == UserRole.Employee)
        {
            // Employees can only delete EmployeeUploaded attachments (their own)
            if (attachment.Type != AttachmentType.EmployeeUploaded)
            {
                _logger.LogWarning(
                    "Employee {UserId} attempted to delete ManagerUploaded attachment {AttachmentId} for task {TaskId}",
                    request.RequestedById,
                    request.AttachmentId,
                    request.TaskId);
                errors.Add(TaskErrors.UnauthorizedFileAccess);
            }
            
            // Employee must be the uploader
            if (attachment.UploadedById != request.RequestedById)
            {
                _logger.LogWarning(
                    "Employee {UserId} attempted to delete attachment {AttachmentId} that they did not upload",
                    request.RequestedById,
                    request.AttachmentId);
                errors.Add(TaskErrors.UnauthorizedFileAccess);
            }
            
            // Employees can delete their own attachments when task is Assigned, Accepted (employee accepted, no ManagerRating), or UnderReview.
            // Employees cannot delete attachments once the task is pending manager review, completed, or accepted by manager
            // Check if task is in "Accepted by Manager" state (Accepted status with ManagerRating set)
            var isAcceptedByManager = task.Status == Domain.Entities.TaskStatus.Accepted && task.ManagerRating.HasValue;
            
            // Allow deletion in: Assigned, Accepted (employee accepted, no ManagerRating), UnderReview
            var canDelete = task.Status == Domain.Entities.TaskStatus.Assigned ||
                            (task.Status == Domain.Entities.TaskStatus.Accepted && !isAcceptedByManager) ||
                            task.Status == Domain.Entities.TaskStatus.UnderReview;
            
            if (!canDelete)
            {
                _logger.LogWarning(
                    "Employee {UserId} attempted to delete attachment {AttachmentId} for task {TaskId} in status {Status} (AcceptedByManager: {AcceptedByManager})",
                    request.RequestedById,
                    request.AttachmentId,
                    request.TaskId,
                    task.Status,
                    isAcceptedByManager);
                errors.Add(TaskErrors.UnauthorizedFileAccess);
            }
        }

        // Access control: Only uploader or task creator can delete (admins handled at controller level)
        // Note: For employees, this is already checked above, but we keep it for other roles
        if (attachment.UploadedById != request.RequestedById && task.CreatedById != request.RequestedById)
        {
            _logger.LogWarning(
                "User {UserId} attempted to delete attachment {AttachmentId} without permission",
                request.RequestedById,
                request.AttachmentId);
            errors.Add(TaskErrors.UnauthorizedFileAccess);
        }

        // Check all errors once before database operations
        if (errors.Any())
        {
            return Result.Failure(errors);
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

