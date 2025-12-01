using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Tasks.Queries.GetTaskHistory;

/// <summary>
///     Query to get task history.
/// </summary>
public record GetTaskHistoryQuery(Guid TaskId, Guid UserId) : IRequest<List<TaskHistoryDto>>;