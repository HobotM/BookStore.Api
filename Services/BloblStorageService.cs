using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BookStore.Api.Models;

namespace BookStore.Api.Services;

public sealed class BlobStorageService
{
    private const string ContainerName = "uploads";

    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        IConfiguration configuration,
        ILogger<BlobStorageService> logger)
    {
        _logger = logger;

        var storageAccountName = configuration["Storage:AccountName"];

        if (string.IsNullOrWhiteSpace(storageAccountName))
        {
            throw new InvalidOperationException(
                "Storage account name is missing. Configure Storage:AccountName.");
        }

        var blobServiceUri = new Uri(
            $"https://{storageAccountName}.blob.core.windows.net");

        var blobServiceClient = new BlobServiceClient(
            blobServiceUri,
            new DefaultAzureCredential());

        _containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
    }

    public async Task<string> UploadAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("File is empty.");
        }

        var safeFileName = Path.GetFileName(file.FileName);

        var blobName = $"{Guid.NewGuid()}-{safeFileName}";

        var blobClient = _containerClient.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();

        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders
            {
                ContentType = file.ContentType
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "File uploaded to Blob Storage. BlobName: {BlobName}",
            blobName);

        return blobName;
    }

    public async Task<IReadOnlyList<BlobFileDto>> ListAsync(
        CancellationToken cancellationToken)
    {
        var files = new List<BlobFileDto>();

        await foreach (var blobItem in _containerClient.GetBlobsAsync(
            cancellationToken: cancellationToken))
        {
            files.Add(new BlobFileDto(
                Name: blobItem.Name,
                Size: blobItem.Properties.ContentLength,
                ContentType: blobItem.Properties.ContentType,
                LastModified: blobItem.Properties.LastModified));
        }

        return files;
    }

    public async Task<BlobDownloadResult> DownloadAsync(
        string blobName,
        CancellationToken cancellationToken)
    {
        var safeBlobName = Path.GetFileName(blobName);

        var blobClient = _containerClient.GetBlobClient(safeBlobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            _logger.LogWarning("File {blobName} not found", blobName);
            throw new FileNotFoundException(
                $"Blob '{safeBlobName}' was not found.");
        }

        var downloadResult = await blobClient.DownloadContentAsync(
            cancellationToken);

        return downloadResult.Value;
    }

    public async Task<bool> DeleteAsync(
        string blobName,
        CancellationToken cancellationToken)
    {
        var safeBlobName = Path.GetFileName(blobName);

        var blobClient = _containerClient.GetBlobClient(safeBlobName);
        try
        {
            var response = await blobClient.DeleteIfExistsAsync(
            cancellationToken: cancellationToken);
             if (response.Value)
        {
            _logger.LogInformation(
                "File deleted from Blob Storage. BlobName: {BlobName}",
                safeBlobName);
                
        }

         return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,"Deletion failed for blob {BlobName}",safeBlobName);
            return false;
        }
    }
}