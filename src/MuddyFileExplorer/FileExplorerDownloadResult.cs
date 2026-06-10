using System.IO;

namespace MuddyFileExplorer;

public sealed record FileExplorerDownloadResult(
    string FileName,
    string ContentType,
    Stream Content);
