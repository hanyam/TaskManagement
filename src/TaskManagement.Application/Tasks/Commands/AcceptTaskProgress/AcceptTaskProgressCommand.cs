using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Tasks.Commands.AcceptTaskProgress;

/// <summary>
///     Command for accepting a task progress update (manager).
/// </summary>
public record AcceptTaskProgressCommand : ICommand
{
    public Guid TaskId { get; init; }
    public Guid ProgressHistoryId { get; init; }
    public Guid AcceptedById { get; init; }
}