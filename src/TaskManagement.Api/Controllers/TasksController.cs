using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Commands.CreateTask;
using TaskManagement.Application.Tasks.Queries.GetTaskById;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Api.Controllers;

/// <summary>
///     Controller for handling task operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : BaseController
{
    public TasksController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    ///     Gets a task by its ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <returns>Task information.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var query = new GetTaskByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return HandleResult(result);
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

        var result = await _mediator.Send(query);
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
        var command = new CreateTaskCommand
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            AssignedUserId = request.AssignedUserId,
            CreatedBy = User.Identity?.Name ?? "system"
        };

        var result = await _mediator.Send(command);
        return HandleResult(result, 201);
    }
}