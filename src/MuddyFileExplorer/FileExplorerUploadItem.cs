namespace MuddyFileExplorer;

public sealed class FileExplorerUploadItem
{
    public Guid Id { get; } = Guid.NewGuid();

    public required string Name { get; init; }

    public long Size { get; init; }

    public FileExplorerUploadStatus Status { get; set; } = FileExplorerUploadStatus.Queued;

    public long BytesUploaded { get; set; }

    public string? Message { get; set; }

    public int PercentComplete => Size <= 0 ? 0 : (int)Math.Min(100, Math.Round(BytesUploaded * 100d / Size));
}

public enum FileExplorerUploadStatus
{
    Queued,
    Uploading,
    Complete,
    Failed,
    Canceled
}
