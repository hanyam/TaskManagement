using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.AcceptTask;
using TaskManagement.Application.Tasks.Commands.AcceptTaskProgress;
using TaskManagement.Application.Tasks.Commands.ApproveExtensionRequest;
using TaskManagement.Application.Tasks.Commands.AssignTask;
using TaskManagement.Application.Tasks.Commands.CancelTask;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Application.Tasks.Commands.MarkTaskCompleted;
using TaskManagement.Application.Tasks.Commands.ReassignTask;
using TaskManagement.Application.Tasks.Commands.RejectTask;
using TaskManagement.Application.Tasks.Commands.RejectTaskProgress;
using TaskManagement.Application.Tasks.Commands.RequestDeadlineExtension;
using TaskManagement.Application.Tasks.Commands.RequestMoreInfo;
using TaskManagement.Application.Tasks.Commands.ReviewCompletedTask;
using TaskManagement.Application.Tasks.Commands.UpdateTask;
using TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Application.Tasks.Queries.GetTaskHistory;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Application.Tasks.Services;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Presentation.Attributes;
using static TaskManagement.Domain.Constants.RoleNames;
using Task = TaskManagement.Domain.Entities.Task;

namespace TaskManagement.Presentation.Controllers;

/// <summary>
///     Controller for handling task operations.
/// </summary>
[ApiController]
[Route("tasks")]
[Authorize]
public class TasksController(
    ICommandMediator commandMediator,
    IRequestMediator requestMediator,
    ITaskActionService taskActionService,
    TaskManagementDbContext context,
    ICurrentUserService currentUserService,
    ILocalizationService localizationService)
    : BaseController(commandMediator, requestMediator, currentUserService, localizationService)
{
    private readonly TaskManagementDbContext _context = context;
    private readonly ITaskActionService _taskActionService = taskActionService;

    /// <summary>
    ///     Gets a task by its ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>Task information.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get Task by ID",
        Description =
            "Retrieves detailed information about a specific task by its unique identifier. Returns task details including status, priority, assignments, progress history, and HATEOAS links for available actions based on user role and task state."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [EnsureUserId]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        // Get user ID (guaranteed to exist by EnsureUserIdAttribute)
        var userId = GetRequiredUserId();

        var query = new GetTaskByIdQuery
        {
            Id = id,
            UserId = userId
        };
        var result = await _requestMediator.Send(query);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Gets a list of tasks with optional filtering and pagination.
    /// </summary>
    /// <param name="request">Filter parameters for the task list.</param>
    /// <returns>List of tasks with pagination information.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get Tasks List",
        Description =
            "Retrieves a paginated list of tasks with optional filtering by status, priority, assigned user, due date range, and reminder level. Supports filtering by 'created' (tasks created by user) or 'assigned' (tasks assigned to user). Returns pagination metadata including total count, page number, page size, and total pages."
    )]
    [ProducesResponseType(typeof(ApiResponse<GetTasksResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [EnsureUserId]
    public async Task<IActionResult> GetTasks([FromQuery] GetTasksRequest request)
    {
        var userId = GetRequiredUserId();
        var filter = (request.Filter ?? "created").ToLowerInvariant();

        var assignedUserId = request.AssignedUserId;
        Guid? createdById = null;

        switch (filter)
        {
            case "assigned":
                assignedUserId ??= userId;
                break;
            case "created":
            default:
                filter = "created";
                createdById = userId;
                break;
        }

        var query = new GetTasksQuery
        {
            Status = request.Status,
            Priority = request.Priority,
            AssignedUserId = assignedUserId,
            CreatedById = createdById,
            DueDateFrom = request.DueDateFrom,
            DueDateTo = request.DueDateTo,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _requestMediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    ///     Creates a new task.
    /// </summary>
    /// <param name="request">The task creation request.</param>
    /// <returns>Created task information.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create New Task",
        Description =
            "Creates a new task with the specified details. Managers and Admins can create tasks. Tasks can be created as drafts (without assigned user) or assigned immediately. Returns the created task with HATEOAS links for available actions. Requires Manager or Admin role."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = ManagerOrAdmin)]
    [EnsureUserId]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        // Get user ID (guaranteed to exist by EnsureUserIdAttribute)
        var userId = GetRequiredUserId();
        var userEmail = GetCurrentUserEmail() ?? "system";

        var command = new CreateTaskCommand
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedUserId = request.AssignedUserId,
            Type = request.Type,
            CreatedById = userId,
            CreatedBy = userEmail
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result, 201);

        // Generate HATEOAS links for the newly created task
        var links = await GenerateTaskLinks(result.Value!.Id, userId);

        return HandleResultWithLinks(result, links, 201);
    }

    /// <summary>
    ///     Updates an existing task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The task update request.</param>
    /// <returns>Updated task information.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update Task",
        Description =
            "Updates an existing task's details including title, description, priority, due date, and assigned user. Only Managers and Admins can update tasks. Returns the updated task with refreshed HATEOAS links. Note: Task type cannot be changed after creation."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [EnsureUserId]
    [Authorize(Roles = ManagerOrAdmin)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var userId = GetRequiredUserId();
        var userEmail = GetCurrentUserEmail() ?? "system";

        var command = new UpdateTaskCommand
        {
            TaskId = id,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedUserId = request.AssignedUserId,
            UpdatedById = userId,
            UpdatedBy = userEmail
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links for the updated task
        var links = await GenerateTaskLinks(result.Value!.Id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Assigns a task to one or multiple users (manager only).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The assignment request.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/assign")]
    [SwaggerOperation(
        Summary = "Assign Task to Users",
        Description =
            "Assigns a task to one or multiple users. Only Managers can assign tasks. The task status changes from 'Created' to 'Assigned'. Managers can only assign tasks to employees they manage. Returns the updated task with new HATEOAS links."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = Manager)]
    [EnsureUserId]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
    {
        var userId = GetRequiredUserId();

        var command = new AssignTaskCommand
        {
            TaskId = id,
            UserIds = request.UserIds,
            AssignedById = userId
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Cancels a task. Depending on task state, it will either be deleted entirely or marked as Cancelled.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = $"{EmployeeOrManager},{Admin}")]
    [EnsureUserId]
    public async Task<IActionResult> CancelTask(Guid id)
    {
        var userId = GetRequiredUserId();
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? Default;

        var command = new CancelTaskCommand
        {
            TaskId = id,
            RequestedById = userId,
            RequestedByRole = userRole
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    ///     Updates task progress (employee).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The progress update request.</param>
    /// <returns>Progress update information.</returns>
    [HttpPost("{id}/progress")]
    [SwaggerOperation(
        Summary = "Update Task Progress",
        Description =
            "Updates the progress percentage of a task. Available for Employees and Managers. Progress must be between 0 and 100. Creates a progress history entry. For tasks with 'WithAcceptedProgress' type, progress updates require manager acceptance. Returns the progress update information."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = EmployeeOrManager)]
    [EnsureUserId]
    public async Task<IActionResult> UpdateTaskProgress(Guid id, [FromBody] UpdateTaskProgressRequest request)
    {
        var userId = GetRequiredUserId();

        var command = new UpdateTaskProgressCommand
        {
            TaskId = id,
            ProgressPercentage = request.ProgressPercentage,
            Notes = request.Notes,
            UpdatedById = userId
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Accepts a task progress update (manager).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The progress acceptance request.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id}/progress/accept")]
    [SwaggerOperation(
        Summary = "Accept Task Progress Update",
        Description =
            "Accepts a pending task progress update. Only Managers can accept progress updates. Used for tasks with 'WithAcceptedProgress' type where progress updates require manager approval. Updates the progress history entry status to 'Accepted' and applies the progress to the task."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = Manager)]
    [EnsureUserId]
    public async Task<IActionResult> AcceptTaskProgress(Guid id, [FromBody] AcceptTaskProgressRequest request)
    {
        var userId = GetRequiredUserId();

        var command = new AcceptTaskProgressCommand
        {
            TaskId = id,
            ProgressHistoryId = request.ProgressHistoryId,
            AcceptedById = userId
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    ///     Rejects a task progress update (manager).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The progress rejection request.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id}/progress/reject")]
    [SwaggerOperation(
        Summary = "Reject Task Progress Update",
        Description =
            "Rejects a pending task progress update. Only Managers can reject progress updates. Used for tasks with 'WithAcceptedProgress' type where progress updates require manager approval. Updates the progress history entry status to 'Rejected' and returns the task to 'Accepted' status so the employee can update progress again."
    )]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = Manager)]
    [EnsureUserId]
    public async Task<IActionResult> RejectTaskProgress(Guid id, [FromBody] RejectTaskProgressRequest request)
    {
        var userId = GetRequiredUserId();

        var command = new RejectTaskProgressCommand
        {
            TaskId = id,
            ProgressHistoryId = request.ProgressHistoryId,
            RejectedById = userId
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    ///     Accepts an assigned task (employee).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/accept")]
    [SwaggerOperation(
        Summary = "Accept Assigned Task",
        Description =
            "Accepts an assigned task. Available for Employees and Managers. Changes task status from 'Assigned' to 'Accepted'. The assigned user confirms they will work on the task. Returns the updated task with new HATEOAS links reflecting the new status."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = EmployeeOrManager)]
    [EnsureUserId]
    public async Task<IActionResult> AcceptTask(Guid id)
    {
        var userId = GetRequiredUserId();

        var command = new AcceptTaskCommand
        {
            TaskId = id,
            AcceptedById = userId
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Rejects an assigned task (employee).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The rejection request.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/reject")]
    [SwaggerOperation(
        Summary = "Reject Assigned Task",
        Description =
            "Rejects an assigned task. Available for Employees and Managers. Changes task status from 'Assigned' to 'Rejected'. An optional rejection reason can be provided. Returns the updated task with new HATEOAS links."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = EmployeeOrManager)]
    [EnsureUserId]
    public async Task<IActionResult> RejectTask(Guid id, [FromBody] RejectTaskRequest? request = null)
    {
        var userId = GetRequiredUserId();

        var command = new RejectTaskCommand
        {
            TaskId = id,
            Reason = request?.Reason,
            RejectedById = userId
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Requests more information on a task (employee).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The information request.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/request-info")]
    [SwaggerOperation(
        Summary = "Request More Information",
        Description =
            "Requests additional information or clarification about a task. Available for Employees and Managers. Creates an information request that can be viewed by task creators and managers. Returns the updated task with new HATEOAS links."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = EmployeeOrManager)]
    [EnsureUserId]
    public async Task<IActionResult> RequestMoreInfo(Guid id, [FromBody] RequestMoreInfoRequest request)
    {
        var userId = GetRequiredUserId();

        var command = new RequestMoreInfoCommand
        {
            TaskId = id,
            RequestMessage = request.RequestMessage,
            RequestedById = userId
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Reassigns a task to different user(s) (manager).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The reassignment request.</param>
    /// <returns>Updated task information.</returns>
    [HttpPut("{id}/reassign")]
    [SwaggerOperation(
        Summary = "Reassign Task",
        Description =
            "Reassigns a task to different user(s). Only Managers can reassign tasks. Replaces the current assignment with new user(s). Managers can only reassign to employees they manage. Returns the updated task with new HATEOAS links."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = Manager)]
    [EnsureUserId]
    public async Task<IActionResult> ReassignTask(Guid id, [FromBody] ReassignTaskRequest request)
    {
        var userId = GetRequiredUserId();

        var command = new ReassignTaskCommand
        {
            TaskId = id,
            NewUserIds = request.NewUserIds,
            ReassignedById = userId
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Requests a deadline extension (employee).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The extension request.</param>
    /// <returns>Extension request information.</returns>
    [HttpPost("{id}/extension-request")]
    [SwaggerOperation(
        Summary = "Request Deadline Extension",
        Description =
            "Requests an extension to the task's due date. Available for Employees and Managers. Creates a deadline extension request that requires manager approval. The requested due date must be in the future and later than the current due date. Returns the extension request information."
    )]
    [ProducesResponseType(typeof(ApiResponse<ExtensionRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = EmployeeOrManager)]
    [EnsureUserId]
    public async Task<IActionResult> RequestDeadlineExtension(Guid id,
        [FromBody] RequestDeadlineExtensionRequest request)
    {
        var userId = GetRequiredUserId();

        var command = new RequestDeadlineExtensionCommand
        {
            TaskId = id,
            RequestedDueDate = request.RequestedDueDate,
            Reason = request.Reason,
            RequestedById = userId
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Approves a deadline extension request (manager).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="requestId">The extension request ID.</param>
    /// <param name="request">The approval request.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id}/extension-request/{requestId}/approve")]
    [SwaggerOperation(
        Summary = "Approve Deadline Extension Request",
        Description =
            "Approves a pending deadline extension request. Only Managers can approve extension requests. Updates the task's extended due date and marks the extension request as approved. Optional review notes can be provided. Returns the updated extension request."
    )]
    [ProducesResponseType(typeof(ApiResponse<ExtensionRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = Manager)]
    [EnsureUserId]
    public async Task<IActionResult> ApproveExtensionRequest(Guid id, Guid requestId,
        [FromBody] ApproveExtensionRequestRequest? request = null)
    {
        var userId = GetRequiredUserId();

        var command = new ApproveExtensionRequestCommand
        {
            TaskId = id,
            ExtensionRequestId = requestId,
            ApprovedById = userId,
            ReviewNotes = request?.ReviewNotes
        };

        var result = await _commandMediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    ///     Marks a task as completed by employee (transitions to PendingManagerReview).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The mark completed request with optional comment.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/complete")]
    [SwaggerOperation(
        Summary = "Mark Task as Completed",
        Description =
            "Marks a task as completed by an employee. Changes task status to 'PendingManagerReview' and records a history entry with optional comment. This action is available to employees when the task is in 'Assigned' or 'Accepted' status. Returns the updated task with new HATEOAS links."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize]
    [EnsureUserId]
    public async Task<IActionResult> MarkTaskCompleted(Guid id, [FromBody] MarkTaskCompletedRequest? request = null)
    {
        var userId = GetRequiredUserId();

        var command = new MarkTaskCompletedCommand
        {
            TaskId = id,
            CompletedById = userId,
            Comment = request?.Comment
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);

        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Reviews a completed task with rating and feedback (manager).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The review request.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/review-completed")]
    [SwaggerOperation(
        Summary = "Review Completed Task",
        Description =
            "Reviews a completed task with a rating (1-5 stars) and optional feedback. Only Managers and Admins can review tasks. Can accept the completion (status becomes 'Completed'), reject it (status becomes 'RejectedByManager'), or send back for rework (status becomes 'Assigned'). Rating is required if accepting. Returns the updated task with manager rating and feedback."
    )]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [Authorize(Roles = ManagerOrAdmin)]
    [EnsureUserId]
    public async Task<IActionResult> ReviewCompletedTask(Guid id, [FromBody] ReviewCompletedTaskRequest request)
    {
        var command = new ReviewCompletedTaskCommand
        {
            TaskId = id,
            Accepted = request.Accepted,
            Rating = request.Rating,
            Feedback = request.Feedback,
            SendBackForRework = request.SendBackForRework
        };

        var result = await _commandMediator.Send(command);

        if (!result.IsSuccess) return HandleResult(result);

        // Get user ID (guaranteed to exist by EnsureUserIdAttribute)
        var userId = GetRequiredUserId();

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);
        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Gets the history of a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>List of task history entries.</returns>
    [HttpGet("{id}/history")]
    [SwaggerOperation(
        Summary = "Get Task History",
        Description =
            "Gets the complete history of status changes and actions for a task. Only accessible by task creator, assignee, manager, or admin."
    )]
    [ProducesResponseType(typeof(ApiResponse<List<TaskHistoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [EnsureUserId]
    public async Task<IActionResult> GetTaskHistory(Guid id)
    {
        var userId = GetRequiredUserId();
        var query = new GetTaskHistoryQuery(id, userId);
        var result = await _requestMediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    ///     Generates HATEOAS links for a task based on current user and task state.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The current user ID.</param>
    /// <returns>List of available action links.</returns>
    private async Task<List<ApiActionLink>?> GenerateTaskLinks(Guid taskId, Guid userId)
    {
        // Fetch the task entity from the database
        var task = await _context.Set<Task>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) return null;

        // Get user role from ICurrentUserService (supports override) or fallback to claims
        var userRole = Default;
        if (_currentUserService != null)
            userRole = _currentUserService.GetClaimValue(ClaimTypes.Role) ?? Default;
        else
            userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? Default;

        // Generate HATEOAS links using the service
        var links = _taskActionService.GetAvailableActions(task, userId, userRole);

        return links;
    }
}

/// <summary>
///     Request DTO for reviewing a completed task.
/// </summary>
public record MarkTaskCompletedRequest
{
    public string? Comment { get; init; }
}

public record ReviewCompletedTaskRequest
{
    public bool Accepted { get; init; }
    public int Rating { get; init; }
    public string? Feedback { get; init; }
    public bool SendBackForRework { get; init; }
}