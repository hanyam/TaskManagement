using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Errors.Tasks;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Data.Repositories;
using static TaskManagement.Domain.Constants.RoleNames;

namespace TaskManagement.Application.Tasks.Queries.GetTaskAttachments;

/// <summary>
///     Handler for getting task attachments with access control.
/// </summary>
public class GetTaskAttachmentsQueryHandler(
    TaskAttachmentDapperRepository attachmentRepository,
    TaskManagementDbContext context,
    ICurrentUserService currentUserService,
    ILogger<GetTaskAttachmentsQueryHandler> logger) : IRequestHandler<GetTaskAttachmentsQuery, List<TaskAttachmentDto>>
{
    private readonly TaskAttachmentDapperRepository _attachmentRepository = attachmentRepository;
    private readonly TaskManagementDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ILogger<GetTaskAttachmentsQueryHandler> _logger = logger;

    public async Task<Result<List<TaskAttachmentDto>>> Handle(GetTaskAttachmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving attachments for task {TaskId} requested by user {UserId}", request.TaskId, request.RequestedById);

        // Verify task exists
        var task = await _context.Set<Domain.Entities.Task>().FindAsync(new object[] { request.TaskId }, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task {TaskId} not found when retrieving attachments", request.TaskId);
            return Result<List<TaskAttachmentDto>>.Failure(TaskErrors.NotFound);
        }

        // Get all attachments
        var attachments = (await _attachmentRepository.GetTaskAttachmentsAsync(request.TaskId, cancellationToken)).ToList();

        // Get user role from claims
        var userRole = _currentUserService.GetUserPrincipal()?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        // Managers and Admins can always view all attachments regardless of task status
        if (userRole == Manager || userRole == Admin)
        {
            _logger.LogInformation(
                "User {UserId} with role {Role} can view all {Count} attachments for task {TaskId}",
                request.RequestedById,
                userRole,
                attachments.Count,
                request.TaskId);

            return Result<List<TaskAttachmentDto>>.Success(attachments);
        }

        // Filter attachments based on access rules for non-manager/admin users
        var accessibleAttachments = new List<TaskAttachmentDto>();

        foreach (var attachment in attachments)
        {
            // Manager-uploaded files: visible if task is Accepted, UnderReview, PendingManagerReview, or Completed
            if (attachment.Type == AttachmentType.ManagerUploaded)
            {
                if (task.Status == Domain.Entities.TaskStatus.Accepted ||
                    task.Status == Domain.Entities.TaskStatus.UnderReview ||
                    task.Status == Domain.Entities.TaskStatus.PendingManagerReview ||
                    task.Status == Domain.Entities.TaskStatus.Completed)
                {
                    accessibleAttachments.Add(attachment);
                }
            }
            // Employee-uploaded files: visible if task is UnderReview, PendingManagerReview, or Completed
            else if (attachment.Type == AttachmentType.EmployeeUploaded)
            {
                if (task.Status == Domain.Entities.TaskStatus.UnderReview ||
                    task.Status == Domain.Entities.TaskStatus.PendingManagerReview ||
                    task.Status == Domain.Entities.TaskStatus.Completed)
                {
                    accessibleAttachments.Add(attachment);
                }
            }
        }

        _logger.LogInformation(
            "Retrieved {Count} accessible attachments out of {TotalCount} total attachments for task {TaskId} (user role: {Role})",
            accessibleAttachments.Count,
            attachments.Count,
            request.TaskId,
            userRole);

        return Result<List<TaskAttachmentDto>>.Success(accessibleAttachments);
    }
}

