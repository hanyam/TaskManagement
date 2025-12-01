using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Tasks.Commands.UploadTaskAttachment;

/// <summary>
///     Command for uploading a file attachment to a task.
/// </summary>
public record UploadTaskAttachmentCommand : ICommand<TaskAttachmentDto>
{
    public Guid TaskId { get; init; }
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public AttachmentType Type { get; init; }
    public Guid UploadedById { get; init; }
    public string UploadedBy { get; init; } = string.Empty;
}