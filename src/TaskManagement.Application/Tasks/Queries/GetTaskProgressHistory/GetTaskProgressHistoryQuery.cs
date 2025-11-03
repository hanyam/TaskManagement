using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Queries.GetTaskProgressHistory;

/// <summary>
///     Query for getting task progress history.
/// </summary>
public record GetTaskProgressHistoryQuery : IQuery<List<TaskProgressDto>>
{
    public Guid TaskId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

