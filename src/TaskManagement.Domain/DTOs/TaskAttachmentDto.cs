using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.DTOs;

/// <summary>
///     Data Transfer Object for TaskAttachment entity.
/// </summary>
public class TaskAttachmentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public AttachmentType Type { get; set; }
    public Guid UploadedById { get; set; }
    public string? UploadedByEmail { get; set; }
    public string? UploadedByDisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
///     Response DTO for downloading a file attachment.
/// </summary>
public class DownloadAttachmentResponse
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
}