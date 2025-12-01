namespace TaskManagement.Domain.Options;

/// <summary>
///     Configuration options for file storage settings.
/// </summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>
    ///     Storage provider: "Local" or "AzureBlob".
    /// </summary>
    public string Provider { get; set; } = "Local";

    /// <summary>
    ///     Maximum file size in bytes (default: 50MB).
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 52_428_800; // 50MB

    /// <summary>
    ///     Local file storage path (for Local provider).
    /// </summary>
    public string LocalPath { get; set; } = string.Empty;

    /// <summary>
    ///     Azure Blob Storage configuration.
    /// </summary>
    public AzureBlobStorageOptions AzureBlob { get; set; } = new();
}

/// <summary>
///     Configuration options for Azure Blob Storage.
/// </summary>
public class AzureBlobStorageOptions
{
    /// <summary>
    ///     Azure Blob Storage connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    ///     Container name for storing task attachments.
    /// </summary>
    public string ContainerName { get; set; } = "task-attachments";
}