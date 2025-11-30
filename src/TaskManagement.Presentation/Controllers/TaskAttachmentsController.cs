using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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
[Route("tasks/{taskId}/attachments")]
[Authorize]
public class TaskAttachmentsController(
    ICommandMediator commandMediator,
    IRequestMediator requestMediator,
    ICurrentUserService currentUserService,
    Application.Common.Interfaces.ILocalizationService localizationService)
    : BaseController(commandMediator, requestMediator, currentUserService, localizationService)
{
    /// <summary>
    ///     Uploads a file attachment to a task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="file">The file to upload.</param>
    /// <param name="type">The attachment type (ManagerUploaded or EmployeeUploaded).</param>
    /// <returns>The uploaded attachment information.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Upload Task Attachment",
        Description = "Uploads a file attachment to a task. Managers and Admins can upload ManagerUploaded attachments, while Employees can upload EmployeeUploaded attachments. The attachment type is automatically determined based on user role if not specified. File size and type validations apply. Returns the uploaded attachment information with file metadata."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskAttachmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
        Summary = "Get Task Attachments",
        Description = "Retrieves all attachments for a task with role-based access control. Managers and Admins can view all attachments regardless of task status. Employees can only view attachments when the task is in 'Accepted' or later status. Returns a filtered list of attachments the user has permission to access."
    )]
    [ProducesResponseType(typeof(ApiResponse<List<TaskAttachmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
        Summary = "Download Task Attachment",
        Description = "Downloads a task attachment file. Managers and Admins can download any attachment regardless of task status. Employees can only download attachments when the task is in 'Accepted' or later status. Returns the file stream with appropriate content type and filename headers."
    )]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
        Summary = "Delete Task Attachment",
        Description = "Deletes a task attachment. Users can only delete attachments they uploaded themselves, unless they are Managers or Admins who can delete any attachment. The file is permanently removed from storage. Returns a success response upon successful deletion."
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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

