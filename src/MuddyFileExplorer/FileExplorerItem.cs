namespace MuddyFileExplorer;

public sealed record FileExplorerItem
{
    public required string Id { get; init; }

    public string? ParentId { get; init; }

    public required string Name { get; init; }

    public bool IsDirectory { get; init; }

    public long? Size { get; init; }

    public string Type { get; init; } = "File";

    public string? ContentType { get; init; }

    public DateTimeOffset ModifiedAt { get; init; }

    public string? DownloadUrl { get; init; }

    public bool CanDownload { get; init; } = true;

    public bool CanRename { get; init; } = true;

    public bool CanMove { get; init; } = true;

    public bool CanDelete { get; init; } = true;
}
