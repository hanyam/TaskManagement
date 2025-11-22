using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Options;

namespace TaskManagement.Infrastructure.FileStorage;

/// <summary>
///     Azure Blob Storage implementation of file storage service for production mode.
/// </summary>
public class AzureBlobStorageService(
    IOptions<FileStorageOptions> options,
    ILogger<AzureBlobStorageService> logger) : IFileStorageService
{
    private readonly FileStorageOptions _options = options.Value;
    private readonly ILogger<AzureBlobStorageService> _logger = logger;
    private BlobContainerClient? _containerClient;

    private BlobContainerClient GetContainerClient()
    {
        if (_containerClient != null)
            return _containerClient;

        if (string.IsNullOrWhiteSpace(_options.AzureBlob.ConnectionString))
            throw new InvalidOperationException("Azure Blob Storage connection string is not configured");

        if (string.IsNullOrWhiteSpace(_options.AzureBlob.ContainerName))
            throw new InvalidOperationException("Azure Blob Storage container name is not configured");

        var blobServiceClient = new BlobServiceClient(_options.AzureBlob.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(_options.AzureBlob.ContainerName);

        // Ensure container exists
        _containerClient.CreateIfNotExists(PublicAccessType.None);

        return _containerClient;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        var containerClient = GetContainerClient();

        // Create blob path: tasks/{taskId}/{attachmentId}/{fileName}
        // For now, we'll use: attachments/{guid}/{fileName}
        var attachmentId = Guid.NewGuid();
        var blobName = $"attachments/{attachmentId}/{fileName}";

        try
        {
            var blobClient = containerClient.GetBlobClient(blobName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            };

            await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

            _logger.LogInformation("File uploaded successfully to Azure Blob Storage: {BlobName}", blobName);
            return blobName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to Azure Blob Storage: {BlobName}", blobName);
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(storagePath);

        if (!await blobClient.ExistsAsync(cancellationToken: cancellationToken))
            throw new FileNotFoundException($"File not found: {storagePath}");

        try
        {
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from Azure Blob Storage: {StoragePath}", storagePath);
            throw;
        }
    }

    public async Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(storagePath);

        if (!await blobClient.ExistsAsync(cancellationToken: cancellationToken))
        {
            _logger.LogWarning("File not found for deletion: {StoragePath}", storagePath);
            return;
        }

        try
        {
            await blobClient.DeleteAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("File deleted successfully from Azure Blob Storage: {StoragePath}", storagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from Azure Blob Storage: {StoragePath}", storagePath);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string storagePath, CancellationToken cancellationToken)
    {
        var containerClient = GetContainerClient();
        var blobClient = containerClient.GetBlobClient(storagePath);
        return await blobClient.ExistsAsync(cancellationToken: cancellationToken);
    }
}

