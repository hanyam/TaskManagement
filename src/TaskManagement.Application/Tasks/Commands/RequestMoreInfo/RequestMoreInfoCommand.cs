using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.RequestMoreInfo;

/// <summary>
///     Command for requesting more information on a task (employee).
/// </summary>
public record RequestMoreInfoCommand : ICommand<TaskDto>
{
    public Guid TaskId { get; init; }
    public string RequestMessage { get; init; } = string.Empty;
    public Guid RequestedById { get; init; }
}

