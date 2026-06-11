using Microsoft.AspNetCore.Components.Forms;

namespace MuddyFileExplorer;

public interface IFileExplorerProvider
{
    Task<FileExplorerFolderListing> ListFolderAsync(string? folderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileExplorerFolder>> ListMoveTargetsAsync(CancellationToken cancellationToken = default);

    Task<FileExplorerOperationResult> CreateFolderAsync(string? parentFolderId, string name, CancellationToken cancellationToken = default);

    Task<FileExplorerOperationResult> CreateFileAsync(string? parentFolderId, string name, CancellationToken cancellationToken = default);

    Task<FileExplorerOperationResult> RenameAsync(string itemId, string newName, CancellationToken cancellationToken = default);

    Task<FileExplorerOperationResult> MoveAsync(string itemId, string destinationFolderId, CancellationToken cancellationToken = default);

    Task<FileExplorerOperationResult> DeleteAsync(string itemId, CancellationToken cancellationToken = default);

    Task<FileExplorerDownloadResult> DownloadAsync(string itemId, CancellationToken cancellationToken = default);

    Task<FileExplorerOperationResult> UploadAsync(
        string? folderId,
        IBrowserFile file,
        long maxAllowedSize,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default);
}
