using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.DeleteTaskAttachment;
using TaskManagement.Application.Tasks.Commands.UploadTaskAttachment;
using TaskManagement.Application.Tasks.Queries.DownloadTaskAttachment;
using TaskManagement.Application.Tasks.Queries.GetTaskAttachments;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Constants;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Presentation.Attributes;
using static TaskManagement.Domain.Constants.RoleNames;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for handling task attachment operations.
/// </summary>
[ApiController]
[Route("api/tasks/{taskId}/attachments")]
[Authorize]
public class TaskAttachmentsController(
    ICommandMediator commandMediator,
    IRequestMediator requestMediator,
    ICurrentUserService currentUserService)
    : BaseController(commandMediator, requestMediator, currentUserService)
{
    /// <summary>
    ///     Uploads a file attachment to a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="type">The attachment type (ManagerUploaded or EmployeeUploaded).</param>
    /// <returns>The uploaded attachment information.</returns>
    [HttpPost]
    [EnsureUserId]
    public async Task<IActionResult> UploadAttachment(
        Guid taskId,
        IFormFile file,
        [FromForm] AttachmentType type)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<TaskAttachmentDto>.ErrorResponse(
                new List<Error> { Error.Validation("File is required", "File") },
                HttpContext.TraceIdentifier));
        }

        var userId = GetRequiredUserId();
        var userEmail = GetCurrentUserEmail() ?? string.Empty;

        // Determine attachment type based on user role if not provided
        // Managers upload ManagerUploaded, Employees upload EmployeeUploaded
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        if (type == 0 && userRole != RoleNames.Manager && userRole != RoleNames.Admin)
        {
            type = AttachmentType.EmployeeUploaded;
        }
        else if (type == 0)
        {
            type = AttachmentType.ManagerUploaded;
        }

        // Check authorization based on type
        if (type == AttachmentType.ManagerUploaded && userRole != RoleNames.Manager && userRole != RoleNames.Admin)
        {
            return Forbid();
        }

        using var fileStream = file.OpenReadStream();
        var command = new UploadTaskAttachmentCommand
        {
            TaskId = taskId,
            FileStream = fileStream,
            FileName = file.FileName,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileSize = file.Length,
            Type = type,
            UploadedById = userId,
            UploadedBy = userEmail
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result, 201);
    }

    /// <summary>
    ///     Gets all attachments for a task (with access control).
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>List of accessible attachments.</returns>
    [HttpGet]
    [EnsureUserId]
    [Authorize(Roles = EmployeeOrManager)]
    public async Task<IActionResult> GetAttachments(Guid taskId)
    {
        var userId = GetRequiredUserId();

        var query = new GetTaskAttachmentsQuery
        {
            TaskId = taskId,
            RequestedById = userId
        };

        var result = await _requestMediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    ///     Downloads a task attachment.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="attachmentId">The attachment ID.</param>
    /// <returns>The file stream.</returns>
    [HttpGet("{attachmentId}/download")]
    [EnsureUserId]
    [Authorize(Roles = EmployeeOrManager)]
    public async Task<IActionResult> DownloadAttachment(Guid taskId, Guid attachmentId)
    {
        var userId = GetRequiredUserId();

        var query = new DownloadTaskAttachmentQuery
        {
            TaskId = taskId,
            AttachmentId = attachmentId,
            RequestedById = userId
        };

        var result = await _requestMediator.Send(query);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        var response = result.Value!;
        return File(response.FileStream, response.ContentType, response.FileName);
    }

    /// <summary>
    ///     Deletes a task attachment.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="attachmentId">The attachment ID.</param>
    /// <returns>Success response.</returns>
    [HttpDelete("{attachmentId}")]
    [EnsureUserId]
    [Authorize(Roles = EmployeeOrManager)]
    public async Task<IActionResult> DeleteAttachment(Guid taskId, Guid attachmentId)
    {
        var userId = GetRequiredUserId();

        var command = new DeleteTaskAttachmentCommand
        {
            TaskId = taskId,
            AttachmentId = attachmentId,
            RequestedById = userId
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result);
    }
}

