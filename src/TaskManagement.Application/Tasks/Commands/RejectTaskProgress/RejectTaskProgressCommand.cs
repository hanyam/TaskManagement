using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Tasks.Commands.RejectTaskProgress;

/// <summary>
///     Command for rejecting a task progress update (manager).
/// </summary>
public record RejectTaskProgressCommand : ICommand
{
    public Guid TaskId { get; init; }
    public Guid ProgressHistoryId { get; init; }
    public Guid RejectedById { get; init; }
}

