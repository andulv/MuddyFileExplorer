namespace MuddyFileExplorer;

public sealed record FileExplorerOperationResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public static FileExplorerOperationResult Ok(string message = "") => new()
    {
        Success = true,
        Message = message
    };

    public static FileExplorerOperationResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}
