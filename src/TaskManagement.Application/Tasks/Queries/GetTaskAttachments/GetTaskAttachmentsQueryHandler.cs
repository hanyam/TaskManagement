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
        var isAdmin = userRole == Admin;
        
        // Determine if user is the task creator (manager) based on task relationship, not role
        var userId = _currentUserService.GetUserId();
        var isManager = userId.HasValue && task.CreatedById == userId.Value;

        // Filter attachments based on access rules
        var accessibleAttachments = new List<TaskAttachmentDto>();

        // Check if task is in "Accepted by Manager" state (Accepted status with ManagerRating set)
        var isAcceptedByManager = task.Status == Domain.Entities.TaskStatus.Accepted && task.ManagerRating.HasValue;

        foreach (var attachment in attachments)
        {
            if (attachment.Type == AttachmentType.ManagerUploaded)
            {
                // Manager-uploaded files:
                // - Admins and task creators can always view
                // - Assigned users (employees) can view when task is Assigned, Accepted (employee accepted) or later, but NOT in Created status
                if (isAdmin || isManager ||
                    (task.Status != Domain.Entities.TaskStatus.Created &&
                     (task.Status == Domain.Entities.TaskStatus.Assigned ||
                      task.Status == Domain.Entities.TaskStatus.Accepted ||
                      task.Status == Domain.Entities.TaskStatus.UnderReview ||
                      task.Status == Domain.Entities.TaskStatus.PendingManagerReview ||
                      task.Status == Domain.Entities.TaskStatus.Completed)))
                {
                    accessibleAttachments.Add(attachment);
                }
            }
            else if (attachment.Type == AttachmentType.EmployeeUploaded)
            {
                // Employee-uploaded files:
                // - Admins can always view
                // - Task creators can view during review phase (PendingManagerReview) and after manager accepts (Accepted with ManagerRating)
                // - Assigned users (employees) can view when task is Assigned, Accepted (employee accepted, no ManagerRating) or later
                if (isAdmin ||
                    (isManager && (task.Status == Domain.Entities.TaskStatus.PendingManagerReview ||
                                   isAcceptedByManager)) ||
                    (!isManager && !isAdmin &&
                     (task.Status == Domain.Entities.TaskStatus.Assigned ||
                      (task.Status == Domain.Entities.TaskStatus.Accepted && !isAcceptedByManager) ||
                      task.Status == Domain.Entities.TaskStatus.UnderReview ||
                      task.Status == Domain.Entities.TaskStatus.PendingManagerReview ||
                      task.Status == Domain.Entities.TaskStatus.Completed ||
                      isAcceptedByManager)))
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

