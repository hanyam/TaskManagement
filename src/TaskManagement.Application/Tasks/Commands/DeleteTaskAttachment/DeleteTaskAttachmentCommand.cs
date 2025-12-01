using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Application.Tasks.Commands.DeleteTaskAttachment;

/// <summary>
///     Command for deleting a task attachment.
/// </summary>
public record DeleteTaskAttachmentCommand : ICommand
{
    public Guid TaskId { get; init; }
    public Guid AttachmentId { get; init; }
    public Guid RequestedById { get; init; }
}