using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Tasks.Commands.CreateTask;

/// <summary>
///     Command for creating a new task.
/// </summary>
public record CreateTaskCommand : ICommand<TaskDto>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;
    public DateTime? DueDate { get; init; }
    public Guid? AssignedUserId { get; init; }
    public TaskType Type { get; init; } = TaskType.Simple;
    public Guid CreatedById { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}