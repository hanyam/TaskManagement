using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Commands.RequestDeadlineExtension;

/// <summary>
///     Command for requesting a deadline extension (employee).
/// </summary>
public record RequestDeadlineExtensionCommand : ICommand<ExtensionRequestDto>
{
    public Guid TaskId { get; init; }
    public DateTime RequestedDueDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid RequestedById { get; init; }
}

