namespace RaceDay.Api.Storage;

/// <summary>Abstraction over file/image storage (Azure Blob Storage or local fallback).</summary>
public interface IFileStorageService
{
    /// <summary>Uploads a file and returns its public URL.</summary>
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);

    /// <summary>Deletes a previously uploaded file by URL (best effort).</summary>
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}
