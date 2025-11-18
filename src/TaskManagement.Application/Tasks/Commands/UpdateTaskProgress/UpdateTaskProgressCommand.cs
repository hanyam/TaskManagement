using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.UpdateTaskProgress;

/// <summary>
///     Command for updating task progress (employee).
/// </summary>
public record UpdateTaskProgressCommand : ICommand<TaskProgressDto>
{
    public Guid TaskId { get; init; }
    public int ProgressPercentage { get; init; }
    public string? Notes { get; init; }
    public Guid UpdatedById { get; init; }
}