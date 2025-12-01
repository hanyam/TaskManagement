using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Tasks.Commands.UpdateTask;

/// <summary>
///     Command for updating an existing task.
/// </summary>
public record UpdateTaskCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }

    public Guid? AssignedUserId { get; init; }

    // Note: Type is read-only and cannot be changed after task creation
    public Guid UpdatedById { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
}