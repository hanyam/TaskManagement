using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Tasks.Commands.CancelTask;

/// <summary>
///     Command for cancelling (or deleting) a task.
/// </summary>
public record CancelTaskCommand : ICommand
{
    public Guid TaskId { get; init; }
    public Guid RequestedById { get; init; }
    public string RequestedByRole { get; init; } = string.Empty;
}


