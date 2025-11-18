using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskStatus = TaskManagement.Domain.Entities.TaskStatus;

namespace TaskManagement.Application.Tasks.Queries.GetAssignedTasks;

/// <summary>
///     Query for getting tasks assigned to a user.
/// </summary>
public record GetAssignedTasksQuery : IQuery<GetTasksResponse>
{
    public Guid UserId { get; init; }
    public TaskStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}