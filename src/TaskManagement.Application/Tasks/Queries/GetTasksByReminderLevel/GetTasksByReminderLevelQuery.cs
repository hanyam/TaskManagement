using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Queries.GetTasks;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Tasks.Queries.GetTasksByReminderLevel;

/// <summary>
///     Query for getting tasks filtered by reminder level.
/// </summary>
public record GetTasksByReminderLevelQuery : IQuery<GetTasksResponse>
{
    public ReminderLevel ReminderLevel { get; init; }
    public Guid? UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

