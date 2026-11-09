namespace RaceDay.Api.Storage;

/// <summary>
/// Local-disk fallback used when no Azure Blob connection string is configured
/// (e.g. local development and docker-compose without Azurite).
/// Files are served from wwwroot/uploads.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private const string Folder = "uploads";

    public LocalFileStorageService(IWebHostEnvironment env) => _env = env;

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var root = Path.Combine(_env.WebRootPath ?? "wwwroot", Folder);
        Directory.CreateDirectory(root);

        var safe = $"{Guid.NewGuid():N}-{Path.GetFileName(fileName).Replace(' ', '-')}";
        await using var fs = File.Create(Path.Combine(root, safe));
        await content.CopyToAsync(fs, ct);

        return $"/{Folder}/{safe}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var name = Path.GetFileName(new Uri(fileUrl, UriKind.RelativeOrAbsolute).IsAbsoluteUri
            ? new Uri(fileUrl).AbsolutePath
            : fileUrl);
        var path = Path.Combine(_env.WebRootPath ?? "wwwroot", Folder, name);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}
