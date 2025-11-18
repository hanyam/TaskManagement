using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Tasks.Commands.ApproveExtensionRequest;

/// <summary>
///     Command for approving a deadline extension request (manager).
/// </summary>
public record ApproveExtensionRequestCommand : ICommand
{
    public Guid TaskId { get; init; }
    public Guid ExtensionRequestId { get; init; }
    public Guid ApprovedById { get; init; }
    public string? ReviewNotes { get; init; }
}