using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Storage;

/// <summary>Azure Blob Storage implementation of <see cref="IFileStorageService"/>.</summary>
public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlob:ConnectionString"]
            ?? throw new InvalidOperationException("AzureBlob:ConnectionString is not configured.");
        var containerName = configuration["AzureBlob:Container"] ?? "raceday-uploads";

        var service = new BlobServiceClient(connectionString);
        _container = service.GetBlobContainerClient(containerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var blobName = $"{Guid.NewGuid():N}/{Sanitize(fileName)}";
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);

        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var blobName = new Uri(fileUrl).GetComponents(UriComponents.Path, UriFormat.Unescaped);
        await _container.DeleteBlobIfExistsAsync(blobName, cancellationToken: ct);
    }

    private static string Sanitize(string fileName) =>
        string.Concat(fileName.Split(Path.GetInvalidFileNameChars())).Replace(" ", "-");
}
