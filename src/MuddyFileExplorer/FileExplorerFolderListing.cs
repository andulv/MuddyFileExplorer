namespace MuddyFileExplorer;

public sealed record FileExplorerFolderListing
{
    public string? FolderId { get; init; }

    public string FolderName { get; init; } = "Files";

    public IReadOnlyList<FileExplorerFolder> Breadcrumbs { get; init; } = [];

    public IReadOnlyList<FileExplorerItem> Items { get; init; } = [];
}
