namespace TaskManagement.Domain.Interfaces;

/// <summary>
///     Interface for file storage operations (local filesystem or cloud storage).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    ///     Uploads a file to storage and returns the storage path.
    /// </summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The name to use for the stored file.</param>
    /// <param name="contentType">The content type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The storage path where the file was saved.</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Downloads a file from storage.
    /// </summary>
    /// <param name="storagePath">The storage path of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file stream.</returns>
    Task<Stream> DownloadFileAsync(string storagePath, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a file from storage.
    /// </summary>
    /// <param name="storagePath">The storage path of the file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks if a file exists in storage.
    /// </summary>
    /// <param name="storagePath">The storage path of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    Task<bool> FileExistsAsync(string storagePath, CancellationToken cancellationToken);
}