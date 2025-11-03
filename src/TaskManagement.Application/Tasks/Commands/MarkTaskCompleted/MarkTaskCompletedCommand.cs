using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.MarkTaskCompleted;

/// <summary>
///     Command for marking a task as completed (manager).
/// </summary>
public record MarkTaskCompletedCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; init; }
    public Guid CompletedById { get; init; }
}

