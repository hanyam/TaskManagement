using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.AcceptTask;
using TaskManagement.Application.Tasks.Commands.AcceptTaskProgress;
using TaskManagement.Application.Tasks.Commands.ApproveExtensionRequest;
using TaskManagement.Application.Tasks.Commands.AssignTask;
using TaskManagement.Application.Tasks.Commands.MarkTaskCompleted;
using TaskManagement.Application.Tasks.Commands.ReassignTask;
using TaskManagement.Application.Tasks.Commands.RejectTask;
using TaskManagement.Application.Tasks.Commands.RequestDeadlineExtension;
using TaskManagement.Application.Tasks.Commands.RequestMoreInfo;
using TaskManagement.Application.Tasks.Commands.ReviewCompletedTask;
using TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Data;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Controller for handling task operations.
/// </summary>
[ApiController]
[Route("tasks")]
[Authorize]
public class TasksController(
    ICommandMediator commandMediator, 
    IRequestMediator requestMediator,
    TaskManagement.Application.Tasks.Services.ITaskActionService taskActionService,
    TaskManagementDbContext context)
    : BaseController(commandMediator, requestMediator)
{
    private readonly TaskManagement.Application.Tasks.Services.ITaskActionService _taskActionService = taskActionService;
    private readonly TaskManagementDbContext _context = context;
    /// <summary>
    ///     Gets a task by its ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>Task information.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        // Get user ID from claims (should be set during authentication)
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var query = new GetTaskByIdQuery 
        { 
            Id = id,
            UserId = userId
        };
        var result = await _requestMediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);
        
        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Gets a list of tasks with optional filtering and pagination.
    /// </summary>
    /// <param name="status">Filter by task status.</param>
    /// <param name="priority">Filter by task priority.</param>
    /// <param name="assignedUserId">Filter by assigned user ID.</param>
    /// <param name="dueDateFrom">Filter by due date from.</param>
    /// <param name="dueDateTo">Filter by due date to.</param>
    /// <param name="page">Page number for pagination.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>List of tasks with pagination information.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] TaskStatus? status = null,
        [FromQuery] TaskPriority? priority = null,
        [FromQuery] Guid? assignedUserId = null,
        [FromQuery] DateTime? dueDateFrom = null,
        [FromQuery] DateTime? dueDateTo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetTasksQuery
        {
            Status = status,
            Priority = priority,
            AssignedUserId = assignedUserId,
            DueDateFrom = dueDateFrom,
            DueDateTo = dueDateTo,
            Page = page,
            PageSize = pageSize
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
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        // Get user ID from claims (should be set during authentication)
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new CreateTaskCommand
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedUserId = request.AssignedUserId,
            Type = request.Type,
            CreatedById = userId,
            CreatedBy = User.Identity?.Name ?? "system"
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result, 201);
        }

        // Generate HATEOAS links for the newly created task
        var links = await GenerateTaskLinks(result.Value!.Id, userId);
        
        return HandleResultWithLinks(result, links, 201);
    }

    /// <summary>
    ///     Assigns a task to one or multiple users (manager only).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The assignment request.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/assign")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new AssignTaskCommand
        {
            TaskId = id,
            UserIds = request.UserIds,
            AssignedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        // Generate HATEOAS links
        var links = await GenerateTaskLinks(id, userId);
        
        return HandleResultWithLinks(result, links);
    }

    /// <summary>
    ///     Updates task progress (employee).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The progress update request.</param>
    /// <returns>Progress update information.</returns>
    [HttpPost("{id}/progress")]
    [Authorize(Roles = "Employee,Manager")]
    public async Task<IActionResult> UpdateTaskProgress(Guid id, [FromBody] UpdateTaskProgressRequest request)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new UpdateTaskProgressCommand
        {
            TaskId = id,
            ProgressPercentage = request.ProgressPercentage,
            Notes = request.Notes,
            UpdatedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

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
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> AcceptTaskProgress(Guid id, [FromBody] AcceptTaskProgressRequest request)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

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
    ///     Accepts an assigned task (employee).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/accept")]
    [Authorize(Roles = "Employee,Manager")]
    public async Task<IActionResult> AcceptTask(Guid id)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new AcceptTaskCommand
        {
            TaskId = id,
            AcceptedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

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
    [Authorize(Roles = "Employee,Manager")]
    public async Task<IActionResult> RejectTask(Guid id, [FromBody] RejectTaskRequest? request = null)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new RejectTaskCommand
        {
            TaskId = id,
            Reason = request?.Reason,
            RejectedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

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
    [Authorize(Roles = "Employee,Manager")]
    public async Task<IActionResult> RequestMoreInfo(Guid id, [FromBody] RequestMoreInfoRequest request)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new RequestMoreInfoCommand
        {
            TaskId = id,
            RequestMessage = request.RequestMessage,
            RequestedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

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
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ReassignTask(Guid id, [FromBody] ReassignTaskRequest request)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new ReassignTaskCommand
        {
            TaskId = id,
            NewUserIds = request.NewUserIds,
            ReassignedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

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
    [Authorize(Roles = "Employee,Manager")]
    public async Task<IActionResult> RequestDeadlineExtension(Guid id, [FromBody] RequestDeadlineExtensionRequest request)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new RequestDeadlineExtensionCommand
        {
            TaskId = id,
            RequestedDueDate = request.RequestedDueDate,
            Reason = request.Reason,
            RequestedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

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
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ApproveExtensionRequest(Guid id, Guid requestId, [FromBody] ApproveExtensionRequestRequest? request = null)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

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
    ///     Marks a task as completed (manager).
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>Updated task information.</returns>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> MarkTaskCompleted(Guid id)
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User ID not found in token", HttpContext.TraceIdentifier));
        }

        var command = new MarkTaskCompletedCommand
        {
            TaskId = id,
            CompletedById = userId
        };

        var result = await _commandMediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

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
    [Authorize(Roles = "Manager,Admin")]
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
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        // Get user ID from claims for HATEOAS generation
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
        {
            // Generate HATEOAS links
            var links = await GenerateTaskLinks(id, userId);
            return HandleResultWithLinks(result, links);
        }
        
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
        var task = await _context.Set<TaskManagement.Domain.Entities.Task>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return null;
        }

        // Get user role from claims
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";

        // Generate HATEOAS links using the service
        var links = _taskActionService.GetAvailableActions(task, userId, userRole);

        return links;
    }
}

/// <summary>
///     Request DTO for reviewing a completed task.
/// </summary>
public class ReviewCompletedTaskRequest
{
    public bool Accepted { get; set; }
    public int Rating { get; set; }
    public string? Feedback { get; set; }
    public bool SendBackForRework { get; set; }
}