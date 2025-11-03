using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.RejectTask;

/// <summary>
///     Command for rejecting an assigned task (employee).
/// </summary>
public record RejectTaskCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; init; }
    public string? Reason { get; init; }
    public Guid RejectedById { get; init; }
}

