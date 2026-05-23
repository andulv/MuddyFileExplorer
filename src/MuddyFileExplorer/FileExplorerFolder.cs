namespace MuddyFileExplorer;

public sealed record FileExplorerFolder
{
    public required string Id { get; init; }

    public string? ParentId { get; init; }

    public required string Name { get; init; }

    public string Path { get; init; } = "/";
}
