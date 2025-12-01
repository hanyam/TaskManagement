using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Options;

namespace TaskManagement.Infrastructure.FileStorage;

/// <summary>
///     Local filesystem implementation of file storage service for development/debug mode.
/// </summary>
public class LocalFileStorageService(
    IOptions<FileStorageOptions> options,
    ILogger<LocalFileStorageService> logger) : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger = logger;
    private readonly FileStorageOptions _options = options.Value;

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.LocalPath))
            throw new InvalidOperationException("Local file storage path is not configured");

        // Create directory structure: {LocalPath}/attachments/{guid}/{fileName}
        var attachmentId = Guid.NewGuid();
        var directoryPath = Path.IsPathRooted(_options.LocalPath)
            ? Path.Combine(_options.LocalPath, "attachments", attachmentId.ToString())
            : Path.Combine(Directory.GetCurrentDirectory(), _options.LocalPath, "attachments", attachmentId.ToString());

        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, fileName);
        var storagePath = Path.Combine("attachments", attachmentId.ToString(), fileName).Replace('\\', '/');

        try
        {
            await using var fileStreamWriter =
                new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
            await fileStreamWriter.FlushAsync(cancellationToken);

            _logger.LogInformation("File uploaded successfully to local storage: {StoragePath}", storagePath);
            return storagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to local storage: {FilePath}", filePath);
            throw;
        }
    }

    public Task<Stream> DownloadFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.LocalPath))
            throw new InvalidOperationException("Local file storage path is not configured");

        var filePath = Path.Combine(_options.LocalPath, storagePath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {storagePath}");

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream>(fileStream);
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.LocalPath))
            throw new InvalidOperationException("Local file storage path is not configured");

        var filePath = Path.Combine(_options.LocalPath, storagePath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found for deletion: {StoragePath}", storagePath);
            return Task.CompletedTask;
        }

        try
        {
            File.Delete(filePath);

            // Try to delete the parent directory if it's empty
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(directoryPath).Any()) Directory.Delete(directoryPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete empty directory: {DirectoryPath}", directoryPath);
                }

            _logger.LogInformation("File deleted successfully from local storage: {StoragePath}", storagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from local storage: {StoragePath}", storagePath);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.LocalPath))
            return Task.FromResult(false);

        var filePath = Path.Combine(_options.LocalPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(filePath));
    }
}