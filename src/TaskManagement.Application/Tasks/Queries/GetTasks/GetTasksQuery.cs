using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Queries.GetTasks;

/// <summary>
///     Query for getting a list of tasks with filtering and pagination.
/// </summary>
public record GetTasksQuery : IQuery<GetTasksResponse>
{
    public TaskStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public Guid? AssignedUserId { get; init; }
    public DateTime? DueDateFrom { get; init; }
    public DateTime? DueDateTo { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

/// <summary>
///     Response for the GetTasksQuery.
/// </summary>
public record GetTasksResponse
{
    public List<TaskDto> Tasks { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}