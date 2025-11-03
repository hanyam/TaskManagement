using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.AssignTask;

/// <summary>
///     Command for assigning a task to one or multiple users (manager only).
/// </summary>
public record AssignTaskCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; init; }
    public List<Guid> UserIds { get; init; } = new();
    public Guid AssignedById { get; init; }
}

