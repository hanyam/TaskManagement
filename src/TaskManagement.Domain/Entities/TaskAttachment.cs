using TaskManagement.Domain.Common;

namespace TaskManagement.Domain.Entities;

/// <summary>
///     Represents a file attachment for a task.
/// </summary>
public class TaskAttachment : BaseEntity
{
    private TaskAttachment()
    {
    }

    public TaskAttachment(
        Guid taskId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string storagePath,
        AttachmentType type,
        Guid uploadedById)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name cannot be null or empty", nameof(originalFileName));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be null or empty", nameof(contentType));

        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));

        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("Storage path cannot be null or empty", nameof(storagePath));

        if (taskId == Guid.Empty)
            throw new ArgumentException("Task ID cannot be empty", nameof(taskId));

        if (uploadedById == Guid.Empty)
            throw new ArgumentException("Uploaded by user ID cannot be empty", nameof(uploadedById));

        TaskId = taskId;
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        FileSize = fileSize;
        StoragePath = storagePath;
        Type = type;
        UploadedById = uploadedById;
    }

    public Guid TaskId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public AttachmentType Type { get; private set; }
    public Guid UploadedById { get; private set; }

    // Navigation properties
    public Task? Task { get; private set; }
    public User? UploadedBy { get; private set; }
}

/// <summary>
///     Represents the type of attachment (who uploaded it).
/// </summary>
public enum AttachmentType
{
    ManagerUploaded = 0,
    EmployeeUploaded = 1
}

