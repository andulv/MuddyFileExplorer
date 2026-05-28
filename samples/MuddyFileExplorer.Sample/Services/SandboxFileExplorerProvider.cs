using System.Buffers.Text;
using System.Net;
using System.Text;
using MuddyFileExplorer;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace MuddyFileExplorer.Sample.Services;

public sealed class SandboxFileExplorerProvider(
    SandboxSeedDataService seedData,
    ILogger<SandboxFileExplorerProvider> logger) : IFileExplorerProvider
{
    private readonly SandboxSeedDataService _seedData = seedData;
    private readonly string _rootPath = seedData.RootPath;

    public async Task<FileExplorerFolderListing> ListFolderAsync(string? folderId, CancellationToken cancellationToken = default)
    {
        _seedData.EnsureSeedData();

        var folderPath = ResolvePath(folderId);
        if (!Directory.Exists(folderPath))
        {
            logger.LogWarning("Folder not found: {Path}", folderPath);
            throw new DirectoryNotFoundException("Folder not found.");
        }

        var relativeFolder = ToRelative(folderPath);
        var items = new List<FileExplorerItem>();

        foreach (var directory in Directory.EnumerateDirectories(folderPath).OrderBy(Path.GetFileName))
        {
            var info = new DirectoryInfo(directory);
            items.Add(new FileExplorerItem
            {
                Id = EncodeId(ToRelative(directory)),
                ParentId = EncodeNullableId(relativeFolder),
                Name = info.Name,
                IsDirectory = true,
                Type = "Folder",
                ModifiedAt = info.LastWriteTimeUtc
            });
        }

        foreach (var file in Directory.EnumerateFiles(folderPath).OrderBy(Path.GetFileName))
        {
            var info = new FileInfo(file);
            var relative = ToRelative(file);
            items.Add(new FileExplorerItem
            {
                Id = EncodeId(relative),
                ParentId = EncodeNullableId(relativeFolder),
                Name = info.Name,
                IsDirectory = false,
                Size = info.Length,
                Type = GetFileType(info.Extension),
                ContentType = GetContentType(info.Extension),
                ModifiedAt = info.LastWriteTimeUtc,
                DownloadUrl = $"/download/{WebUtility.UrlEncode(EncodeId(relative))}"
            });
        }

        await Task.Yield();

        return new FileExplorerFolderListing
        {
            FolderId = EncodeNullableId(relativeFolder),
            FolderName = string.IsNullOrEmpty(relativeFolder) ? "Files" : Path.GetFileName(folderPath),
            Breadcrumbs = BuildBreadcrumbs(relativeFolder),
            Items = items
        };
    }

    public Task<IReadOnlyList<FileExplorerFolder>> ListMoveTargetsAsync(CancellationToken cancellationToken = default)
    {
        _seedData.EnsureSeedData();

        var folders = new List<FileExplorerFolder>
        {
            new()
            {
                Id = EncodeId(string.Empty),
                Name = "Files",
                Path = "/"
            }
        };

        foreach (var directory in Directory.EnumerateDirectories(_rootPath, "*", SearchOption.AllDirectories).OrderBy(path => path))
        {
            var relative = ToRelative(directory);
            folders.Add(new FileExplorerFolder
            {
                Id = EncodeId(relative),
                ParentId = EncodeNullableId(Path.GetDirectoryName(relative)?.Replace('\\', '/')),
                Name = Path.GetFileName(directory),
                Path = "/" + relative.Replace('\\', '/')
            });
        }

        return Task.FromResult<IReadOnlyList<FileExplorerFolder>>(folders);
    }

    public Task<FileExplorerOperationResult> CreateFolderAsync(string? parentFolderId, string name, CancellationToken cancellationToken = default)
    {
        var parent = ResolvePath(parentFolderId);
        var target = ResolveChild(parent, CleanName(name));

        if (Directory.Exists(target) || File.Exists(target))
        {
            logger.LogWarning("Create folder failed – name already exists: {Path}", target);
            return Task.FromResult(FileExplorerOperationResult.Fail("An item with that name already exists."));
        }

        Directory.CreateDirectory(target);
        logger.LogInformation("Folder created: {Path}", target);
        return Task.FromResult(FileExplorerOperationResult.Ok("Folder created."));
    }

    public Task<FileExplorerOperationResult> RenameAsync(string itemId, string newName, CancellationToken cancellationToken = default)
    {
        var source = ResolvePath(itemId);
        var target = ResolveChild(Path.GetDirectoryName(source) ?? _rootPath, CleanName(newName));

        if (Directory.Exists(target) || File.Exists(target))
        {
            return Task.FromResult(FileExplorerOperationResult.Fail("An item with that name already exists."));
        }

        if (Directory.Exists(source))
        {
            Directory.Move(source, target);
            logger.LogInformation("Folder renamed: {Old} → {New}", source, target);
            return Task.FromResult(FileExplorerOperationResult.Ok("Folder renamed."));
        }

        if (File.Exists(source))
        {
            File.Move(source, target);
            logger.LogInformation("File renamed: {Old} → {New}", source, target);
            return Task.FromResult(FileExplorerOperationResult.Ok("File renamed."));
        }

        logger.LogWarning("Rename failed – item not found: {Path}", source);
        return Task.FromResult(FileExplorerOperationResult.Fail("Item not found."));
    }

    public Task<FileExplorerOperationResult> MoveAsync(string itemId, string destinationFolderId, CancellationToken cancellationToken = default)
    {
        var source = ResolvePath(itemId);
        var destinationFolder = ResolvePath(destinationFolderId);
        var target = ResolveChild(destinationFolder, Path.GetFileName(source));

        if (IsSameOrChild(source, destinationFolder))
        {
            logger.LogWarning("Move rejected – circular: {Source} → {Dest}", source, destinationFolder);
            return Task.FromResult(FileExplorerOperationResult.Fail("A folder cannot be moved into itself."));
        }

        if (Directory.Exists(target) || File.Exists(target))
        {
            logger.LogWarning("Move failed – name conflict at destination: {Path}", target);
            return Task.FromResult(FileExplorerOperationResult.Fail("Destination already contains an item with that name."));
        }

        if (Directory.Exists(source))
        {
            Directory.Move(source, target);
            logger.LogInformation("Folder moved: {Old} → {New}", source, target);
            return Task.FromResult(FileExplorerOperationResult.Ok("Folder moved."));
        }

        if (File.Exists(source))
        {
            File.Move(source, target);
            logger.LogInformation("File moved: {Old} → {New}", source, target);
            return Task.FromResult(FileExplorerOperationResult.Ok("File moved."));
        }

        logger.LogWarning("Move failed – item not found: {Path}", source);
        return Task.FromResult(FileExplorerOperationResult.Fail("Item not found."));
    }

    public Task<FileExplorerOperationResult> DeleteAsync(string itemId, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(itemId);

        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
            logger.LogInformation("Folder deleted: {Path}", path);
            return Task.FromResult(FileExplorerOperationResult.Ok("Folder deleted."));
        }

        if (File.Exists(path))
        {
            File.Delete(path);
            logger.LogInformation("File deleted: {Path}", path);
            return Task.FromResult(FileExplorerOperationResult.Ok("File deleted."));
        }

        logger.LogWarning("Delete failed – item not found: {Path}", path);
        return Task.FromResult(FileExplorerOperationResult.Fail("Item not found."));
    }

    public async Task<FileExplorerOperationResult> UploadAsync(
        string? folderId,
        IBrowserFile file,
        long maxAllowedSize,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var folder = ResolvePath(folderId);
        var target = ResolveChild(folder, CleanName(file.Name));

        if (File.Exists(target))
        {
            logger.LogWarning("Upload rejected – file already exists: {Path}", target);
            return FileExplorerOperationResult.Fail("A file with that name already exists. Delete it first or rename the upload.");
        }

        await using var source = file.OpenReadStream(maxAllowedSize, cancellationToken);
        await using var destination = File.Create(target);
        var buffer = new byte[64 * 1024];
        long total = 0;

        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            total += read;
            progress?.Report(total);
        }

        logger.LogInformation("File uploaded: {Path} ({Size} bytes)", target, total);
        return FileExplorerOperationResult.Ok("Upload complete.");
    }

    public string GetDownloadPath(string id)
    {
        var path = ResolvePath(id);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found.");
        }

        return path;
    }

    public static string GetContentTypeForDownload(string fileName) => GetContentType(Path.GetExtension(fileName));

    private IReadOnlyList<FileExplorerFolder> BuildBreadcrumbs(string relativeFolder)
    {
        var folders = new List<FileExplorerFolder>
        {
            new()
            {
                Id = EncodeId(string.Empty),
                Name = "Files",
                Path = "/"
            }
        };

        if (string.IsNullOrEmpty(relativeFolder))
        {
            return folders;
        }

        var current = string.Empty;
        foreach (var part in relativeFolder.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            current = string.IsNullOrEmpty(current) ? part : $"{current}/{part}";
            folders.Add(new FileExplorerFolder
            {
                Id = EncodeId(current),
                ParentId = EncodeNullableId(Path.GetDirectoryName(current)?.Replace('\\', '/')),
                Name = part,
                Path = "/" + current
            });
        }

        return folders;
    }

    private string ResolvePath(string? id)
    {
        _seedData.EnsureSeedData();

        var relative = DecodeId(id);
        var combined = Path.GetFullPath(Path.Combine(_rootPath, relative));

        if (!combined.StartsWith(_rootPath, StringComparison.Ordinal))
        {
            logger.LogError("Path traversal attempt detected: {Id} resolved to {Path}", id, combined);
            throw new InvalidOperationException("Invalid file path.");
        }

        return combined;
    }

    private string ResolveChild(string parent, string childName)
    {
        var combined = Path.GetFullPath(Path.Combine(parent, childName));
        if (!combined.StartsWith(_rootPath, StringComparison.Ordinal))
        {
            logger.LogError("Path traversal attempt in child resolution: {Parent}/{Child} resolved to {Path}", parent, childName, combined);
            throw new InvalidOperationException("Invalid file path.");
        }

        return combined;
    }

    private string ToRelative(string path)
    {
        var relative = Path.GetRelativePath(_rootPath, path);
        return relative == "." ? string.Empty : relative.Replace('\\', '/');
    }

    private static string? EncodeNullableId(string? relative) => relative is null ? null : EncodeId(relative);

    private static string EncodeId(string relative)
    {
        var bytes = Encoding.UTF8.GetBytes(relative);
        return Base64Url.EncodeToString(bytes);
    }

    private static string DecodeId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        var bytes = Base64Url.DecodeFromChars(id);
        return Encoding.UTF8.GetString(bytes).Replace('\\', '/');
    }

    private static string CleanName(string name)
    {
        var cleaned = Path.GetFileName(name.Trim());
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            throw new InvalidOperationException("Name is required.");
        }

        return cleaned;
    }

    private static bool IsSameOrChild(string source, string destination)
    {
        var normalizedSource = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var normalizedDestination = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return normalizedDestination.StartsWith(normalizedSource, StringComparison.Ordinal);
    }

    private static string GetFileType(string extension) => extension.ToLowerInvariant() switch
    {
        ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" => "Image",
        ".pdf" => "PDF",
        ".md" or ".markdown" => "Markdown",
        ".txt" or ".log" => "Text",
        ".zip" or ".gz" or ".tar" => "Archive",
        ".csv" or ".xlsx" => "Spreadsheet",
        ".cs" or ".razor" or ".json" or ".xml" or ".js" or ".ts" => "Code",
        _ => "File"
    };

    private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        ".md" or ".markdown" or ".txt" or ".log" => "text/plain",
        ".csv" => "text/csv",
        ".json" => "application/json",
        ".zip" => "application/zip",
        _ => "application/octet-stream"
    };

}
