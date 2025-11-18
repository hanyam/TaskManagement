using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.AcceptTask;

/// <summary>
///     Command for accepting an assigned task (employee).
/// </summary>
public record AcceptTaskCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; init; }
    public Guid AcceptedById { get; init; }
}