using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.DTOs;

namespace TaskManagement.Application.Tasks.Queries.GetTaskAttachments;

/// <summary>
///     Query for getting all attachments for a task.
/// </summary>
public record GetTaskAttachmentsQuery : IQuery<List<TaskAttachmentDto>>
{
    public Guid TaskId { get; init; }
    public Guid RequestedById { get; init; }
}