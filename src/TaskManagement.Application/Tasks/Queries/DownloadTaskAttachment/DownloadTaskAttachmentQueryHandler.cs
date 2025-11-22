using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Application.Tasks.Queries.DownloadTaskAttachment;

/// <summary>
///     Handler for downloading a task attachment with access control.
/// </summary>
public class DownloadTaskAttachmentQueryHandler(
    IFileStorageService fileStorageService,
    TaskManagementDbContext context,
    ILogger<DownloadTaskAttachmentQueryHandler> logger,
    IAuditLogService auditLogService) : IRequestHandler<DownloadTaskAttachmentQuery, DownloadAttachmentResponse>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ILogger<DownloadTaskAttachmentQueryHandler> _logger = logger;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task<Result<DownloadAttachmentResponse>> Handle(DownloadTaskAttachmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting file download for attachment {AttachmentId} in task {TaskId} by user {UserId}",
            request.AttachmentId,
            request.TaskId,
            request.RequestedById);

        // Find attachment
        var attachment = await _context.Set<TaskAttachment>().FindAsync(new object[] { request.AttachmentId }, cancellationToken);
        if (attachment == null)
        {
            _logger.LogWarning("Attachment {AttachmentId} not found for download", request.AttachmentId);
            return Result<DownloadAttachmentResponse>.Failure(TaskErrors.AttachmentNotFound);
        }

        // Verify attachment belongs to the task
        if (attachment.TaskId != request.TaskId)
        {
            _logger.LogWarning(
                "Attachment {AttachmentId} does not belong to task {TaskId}",
                request.AttachmentId,
                request.TaskId);
            return Result<DownloadAttachmentResponse>.Failure(TaskErrors.AttachmentNotFound);
        }

        // Verify task exists
        var task = await _context.Set<Domain.Entities.Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for attachment download", request.TaskId);
            return Result<DownloadAttachmentResponse>.Failure(TaskErrors.NotFound);
        }

        // Access control: Same rules as GetTaskAttachments
        bool canAccess = false;

        // Manager-uploaded files: visible if task is Accepted, UnderReview, PendingManagerReview, or Completed
        if (attachment.Type == AttachmentType.ManagerUploaded)
        {
            canAccess = task.Status == Domain.Entities.TaskStatus.Accepted ||
                       task.Status == Domain.Entities.TaskStatus.UnderReview ||
                       task.Status == Domain.Entities.TaskStatus.PendingManagerReview ||
                       task.Status == Domain.Entities.TaskStatus.Completed;
        }
        // Employee-uploaded files: visible if task is UnderReview, PendingManagerReview, or Completed
        else if (attachment.Type == AttachmentType.EmployeeUploaded)
        {
            canAccess = task.Status == Domain.Entities.TaskStatus.UnderReview ||
                       task.Status == Domain.Entities.TaskStatus.PendingManagerReview ||
                       task.Status == Domain.Entities.TaskStatus.Completed;
        }

        if (!canAccess)
        {
            _logger.LogWarning(
                "User {UserId} attempted to download attachment {AttachmentId} without permission (TaskStatus: {TaskStatus})",
                request.RequestedById,
                request.AttachmentId,
                task.Status);
            return Result<DownloadAttachmentResponse>.Failure(TaskErrors.UnauthorizedFileAccess);
        }

        try
        {
            // Download file from storage
            var fileStream = await _fileStorageService.DownloadFileAsync(attachment.StoragePath, cancellationToken);

            var response = new DownloadAttachmentResponse
            {
                FileStream = fileStream,
                FileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize
            };

            _logger.LogInformation(
                "Successfully downloaded file {FileName} (AttachmentId: {AttachmentId}, Size: {FileSize} bytes) for task {TaskId}",
                attachment.OriginalFileName,
                request.AttachmentId,
                attachment.FileSize,
                request.TaskId);

            // Audit log
            _auditLogService.LogFileDownloaded(
                request.TaskId,
                request.AttachmentId,
                request.RequestedById.ToString(),
                "Unknown"); // Email not available in query

            return Result<DownloadAttachmentResponse>.Success(response);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(
                ex,
                "File not found in storage for attachment {AttachmentId} (StoragePath: {StoragePath})",
                request.AttachmentId,
                attachment.StoragePath);
            return Result<DownloadAttachmentResponse>.Failure(TaskErrors.FileNotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to download attachment {AttachmentId} for task {TaskId}",
                request.AttachmentId,
                request.TaskId);
            return Result<DownloadAttachmentResponse>.Failure(TaskErrors.FileUploadFailed);
        }
    }
}

