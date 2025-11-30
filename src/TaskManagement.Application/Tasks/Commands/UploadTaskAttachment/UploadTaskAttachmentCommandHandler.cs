using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Services;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Options;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;

namespace TaskManagement.Application.Tasks.Commands.UploadTaskAttachment;

/// <summary>
///     Handler for uploading a file attachment to a task.
/// </summary>
public class UploadTaskAttachmentCommandHandler(
    TaskEfCommandRepository taskRepository,
    UserDapperRepository userRepository,
    IFileStorageService fileStorageService,
    TaskManagementDbContext context,
    IOptions<FileStorageOptions> fileStorageOptions,
    ILogger<UploadTaskAttachmentCommandHandler> logger,
    IAuditLogService auditLogService) : ICommandHandler<UploadTaskAttachmentCommand, TaskAttachmentDto>
{
    private readonly TaskManagementDbContext _context = context;
    private readonly FileStorageOptions _fileStorageOptions = fileStorageOptions.Value;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly TaskEfCommandRepository _taskRepository = taskRepository;
    private readonly UserDapperRepository _userRepository = userRepository;
    private readonly ILogger<UploadTaskAttachmentCommandHandler> _logger = logger;
    private readonly IAuditLogService _auditLogService = auditLogService;

    public async Task<Result<TaskAttachmentDto>> Handle(UploadTaskAttachmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting file upload for task {TaskId} by user {UserId}. File: {FileName}, Size: {FileSize} bytes",
            request.TaskId,
            request.UploadedById,
            request.FileName,
            request.FileSize);

        var errors = new List<Error>();

        // Validate task exists
        var task = await _context.Set<Domain.Entities.Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found for file upload", request.TaskId);
            errors.Add(TaskErrors.NotFound);
        }

        // Validate user exists
        var user = await _userRepository.GetByIdAsync(request.UploadedById, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for file upload", request.UploadedById);
            errors.Add(TaskErrors.AssignedUserNotFound);
        }
        else if (task != null)
        {
            // Additional access control for employees:
            // Employees can upload attachments ONLY after they accept the task and while the task is still in progress.
            // Allowed statuses: Accepted (employee accepted, no ManagerRating), UnderReview.
            // NOT allowed: Created, Assigned (must accept first), PendingManagerReview (marked complete), Completed, Cancelled, RejectedByManager, Accepted by Manager (has ManagerRating).
            if (user.Role == UserRole.Employee)
            {
                // Explicitly exclude Assigned and Created statuses - employee must accept the task first
                if (task.Status == Domain.Entities.TaskStatus.Assigned || 
                    task.Status == Domain.Entities.TaskStatus.Created)
                {
                    _logger.LogWarning(
                        "Employee {UserId} attempted to upload attachment for task {TaskId} in status {Status} - task must be accepted first",
                        request.UploadedById,
                        request.TaskId,
                        task.Status);
                    errors.Add(TaskErrors.UnauthorizedFileAccess);
                }
                else
                {
                    // Check if task is in "Accepted by Manager" state (Accepted status with ManagerRating set)
                    var isAcceptedByManager = task.Status == Domain.Entities.TaskStatus.Accepted && task.ManagerRating.HasValue;
                    
                    // Allow only if: Accepted (employee accepted, no ManagerRating) or UnderReview
                    var canUpload = (task.Status == Domain.Entities.TaskStatus.Accepted && !isAcceptedByManager) ||
                                    task.Status == Domain.Entities.TaskStatus.UnderReview;
                    
                    if (!canUpload)
                    {
                        _logger.LogWarning(
                            "Employee {UserId} attempted to upload attachment for task {TaskId} in invalid status {Status} (AcceptedByManager: {AcceptedByManager})",
                            request.UploadedById,
                            request.TaskId,
                            task.Status,
                            isAcceptedByManager);
                        errors.Add(TaskErrors.UnauthorizedFileAccess);
                    }
                }
            }
        }

        // Validate file size
        if (request.FileSize > _fileStorageOptions.MaxFileSizeBytes)
        {
            _logger.LogWarning(
                "File size {FileSize} bytes exceeds maximum {MaxFileSize} bytes for task {TaskId}",
                request.FileSize,
                _fileStorageOptions.MaxFileSizeBytes,
                request.TaskId);
            errors.Add(TaskErrors.FileSizeExceeded);
        }

        // Validate file name
        if (string.IsNullOrWhiteSpace(request.FileName) || request.FileName.Length > 500)
        {
            _logger.LogWarning("Invalid file name for task {TaskId}: {FileName}", request.TaskId, request.FileName);
            errors.Add(TaskErrors.InvalidFileName);
        }

        if (errors.Any())
        {
            return Result<TaskAttachmentDto>.Failure(errors);
        }

        // Generate unique file name to avoid conflicts
        var fileExtension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

        try
        {
            // Upload file to storage
            var storagePath = await _fileStorageService.UploadFileAsync(
                request.FileStream,
                uniqueFileName,
                request.ContentType,
                cancellationToken);

            // Create attachment entity
            var attachment = new TaskAttachment(
                request.TaskId,
                uniqueFileName,
                request.FileName, // Original file name
                request.ContentType,
                request.FileSize,
                storagePath,
                request.Type,
                request.UploadedById);

            attachment.SetCreatedBy(request.UploadedBy);

            // Add to database
            await _context.Set<TaskAttachment>().AddAsync(attachment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Return DTO
            var attachmentDto = new TaskAttachmentDto
            {
                Id = attachment.Id,
                TaskId = attachment.TaskId,
                FileName = attachment.FileName,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                Type = attachment.Type,
                UploadedById = attachment.UploadedById,
                UploadedByEmail = user?.Email,
                UploadedByDisplayName = user?.DisplayName,
                CreatedAt = attachment.CreatedAt
            };

            _logger.LogInformation(
                "Successfully uploaded file {FileName} (AttachmentId: {AttachmentId}) for task {TaskId}",
                request.FileName,
                attachment.Id,
                request.TaskId);

            // Audit log
            _auditLogService.LogFileUploaded(
                request.TaskId,
                attachment.Id,
                request.FileName,
                request.UploadedById.ToString(),
                user?.Email ?? request.UploadedBy);

            return Result<TaskAttachmentDto>.Success(attachmentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to upload file {FileName} for task {TaskId} by user {UserId}",
                request.FileName,
                request.TaskId,
                request.UploadedById);
            return Result<TaskAttachmentDto>.Failure(TaskErrors.FileUploadFailed);
        }
    }
}

