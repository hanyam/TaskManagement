using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Queries.DownloadTaskAttachment;

/// <summary>
///     Query for downloading a task attachment.
/// </summary>
public record DownloadTaskAttachmentQuery : IQuery<DownloadAttachmentResponse>
{
    public Guid TaskId { get; init; }
    public Guid AttachmentId { get; init; }
    public Guid RequestedById { get; init; }
}