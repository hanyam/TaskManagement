using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.ReassignTask;

/// <summary>
///     Command for reassigning a task to different user(s) (manager).
/// </summary>
public record ReassignTaskCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; init; }
    public List<Guid> NewUserIds { get; init; } = new();
    public Guid ReassignedById { get; init; }
}

