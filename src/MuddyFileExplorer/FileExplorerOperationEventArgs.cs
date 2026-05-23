namespace MuddyFileExplorer;

public sealed record FileExplorerOperationEventArgs(
    string Operation,
    FileExplorerItem? Item,
    FileExplorerOperationResult Result);
