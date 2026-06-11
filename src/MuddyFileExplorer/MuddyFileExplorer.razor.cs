using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using MuddyFileExplorer;

namespace MuddyFileExplorer.Components;

public partial class MuddyFileExplorer : IAsyncDisposable
{
    private FileExplorerFolderListing? _listing;
    private readonly HashSet<FileExplorerItem> _selectedItems = [];
    private readonly List<FileExplorerUploadItem> _uploads = [];
    private string? _currentFolderId;
    private string? _searchText;
    private string? _error;
    private string _operationText = "Ready";
    private bool _busy;
    private FileExplorerItem? _currentItem;
    private IJSObjectReference? _jsModule;

    [Parameter, EditorRequired]
    public IFileExplorerProvider Provider { get; set; } = default!;

    [Parameter]
    public string? InitialFolderId { get; set; }

    [Parameter]
    public bool Dense { get; set; } = true;

    [Parameter]
    public bool MultiSelection { get; set; } = true;

    [Parameter]
    public bool AllowUpload { get; set; } = true;

    [Parameter]
    public bool AllowDownload { get; set; } = true;

    [Parameter]
    public bool AllowRename { get; set; } = true;

    [Parameter]
    public bool AllowMove { get; set; } = true;

    [Parameter]
    public bool AllowDelete { get; set; } = true;

    [Parameter]
    public bool AllowCreateFolder { get; set; } = true;

    [Parameter]
    public long MaxUploadFileSize { get; set; } = 100 * 1024 * 1024;

    [Parameter]
    public int MaximumUploadCount { get; set; } = 25;

    [Parameter]
    public string? AcceptedFileTypes { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyCollection<FileExplorerItem>> SelectedItemsChanged { get; set; }

    [Parameter]
    public EventCallback<FileExplorerItem> CurrentItemChanged { get; set; }

    [Parameter]
    public EventCallback<FileExplorerItemEventArgs> ItemOpened { get; set; }

    [Parameter]
    public EventCallback<FileExplorerOperationEventArgs> OperationCompleted { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _currentFolderId = InitialFolderId;
        await LoadFolderAsync(_currentFolderId);
    }

    private IEnumerable<FileExplorerItem> FilteredItems
    {
        get
        {
            var items = _listing?.Items ?? [];
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return items;
            }

            return items.Where(item =>
                item.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                item.Type.Contains(_searchText, StringComparison.OrdinalIgnoreCase));
        }
    }

    private string StatusText
    {
        get
        {
            var items = _listing?.Items ?? [];
            var folders = items.Count(item => item.IsDirectory);
            var files = items.Count - folders;
            var totalSize = items.Where(item => !item.IsDirectory).Sum(item => item.Size ?? 0);

            var text = $"{folders} folders | {files} files | {FormatBytes(totalSize)}";

            if (_selectedItems.Count > 0)
            {
                text += $"  ·  {_selectedItems.Count} selected";
            }

            return text;
        }
    }

    private RenderFragment RowMenu(FileExplorerItem item) => builder =>
    {
        var sequence = 0;

        if (item.IsDirectory)
        {
            builder.OpenComponent<MudMenuItem>(sequence++);
            builder.AddAttribute(sequence++, "Icon", Icons.Material.Filled.FolderOpen);
            builder.AddAttribute(sequence++, "Label", "Open");
            builder.AddAttribute(sequence++, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OpenItemAsync(item)));
            builder.CloseComponent();
        }

        if (!item.IsDirectory && CanOpenInNewTab(item))
        {
            builder.OpenComponent<MudMenuItem>(sequence++);
            builder.AddAttribute(sequence++, "Icon", Icons.Material.Filled.OpenInNew);
            builder.AddAttribute(sequence++, "Label", "Open");
            builder.AddAttribute(sequence++, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, () => OpenInNewTabAsync(item)));
            builder.CloseComponent();
        }

        if (AllowDownload && item.CanDownload)
        {
            builder.OpenComponent<MudMenuItem>(sequence++);
            builder.AddAttribute(sequence++, "Icon", Icons.Material.Filled.Download);
            builder.AddAttribute(sequence++, "Label", "Download");
            builder.AddAttribute(sequence++, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, () => DownloadAsync(item)));
            builder.CloseComponent();
        }

        if (AllowRename && item.CanRename)
        {
            builder.OpenComponent<MudMenuItem>(sequence++);
            builder.AddAttribute(sequence++, "Icon", Icons.Material.Filled.DriveFileRenameOutline);
            builder.AddAttribute(sequence++, "Label", "Rename");
            builder.AddAttribute(sequence++, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, () => RenameAsync(item)));
            builder.CloseComponent();
        }

        if (AllowMove && item.CanMove)
        {
            builder.OpenComponent<MudMenuItem>(sequence++);
            builder.AddAttribute(sequence++, "Icon", Icons.Material.Filled.DriveFileMove);
            builder.AddAttribute(sequence++, "Label", "Move");
            builder.AddAttribute(sequence++, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, () => MoveAsync(item)));
            builder.CloseComponent();
        }

        if (AllowDelete && item.CanDelete)
        {
            builder.OpenComponent<MudMenuItem>(sequence++);
            builder.AddAttribute(sequence++, "Icon", Icons.Material.Filled.Delete);
            builder.AddAttribute(sequence++, "IconColor", Color.Error);
            builder.AddAttribute(sequence++, "Label", "Delete");
            builder.AddAttribute(sequence++, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(this, () => DeleteAsync(item)));
            builder.CloseComponent();
        }
    };

    private async Task LoadFolderAsync(string? folderId)
    {
        _busy = true;
        _error = null;
        _operationText = "Loading...";

        try
        {
            _listing = await Provider.ListFolderAsync(folderId);
            _currentFolderId = _listing.FolderId;
            _selectedItems.Clear();
            _currentItem = null;
            _operationText = "Ready";
            await SelectedItemsChanged.InvokeAsync(_selectedItems);
            await CurrentItemChanged.InvokeAsync(_currentItem);
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            _operationText = "Load failed";
        }
        finally
        {
            _busy = false;
        }
    }

    public Task RefreshAsync() => LoadFolderAsync(_currentFolderId);

    private Task NavigateToFolderAsync(string? folderId) => LoadFolderAsync(folderId);

    /// <summary>
    /// Programmatically navigates to the given folder and optionally selects an item within it.
    /// </summary>
    public async Task NavigateToFileAsync(string? folderId, string? itemId = null)
    {
        await LoadFolderAsync(folderId);
        if (itemId is not null)
        {
            var item = _listing?.Items.FirstOrDefault(i => i.Id == itemId);
            if (item is not null)
            {
                _currentItem = item;
                await CurrentItemChanged.InvokeAsync(_currentItem);
            }
        }
    }

    private async Task RowClickedAsync(DataGridRowClickEventArgs<FileExplorerItem> args)
    {
        _currentItem = args.Item;
        await CurrentItemChanged.InvokeAsync(_currentItem);

        if (args.MouseEventArgs.Detail >= 2)
        {
            await OpenItemAsync(args.Item);
        }
    }

    private string RowClassFunc(FileExplorerItem item, int rowIndex) =>
        _currentItem?.Id == item.Id ? "muddy-file-explorer-row-current" : "";

    private async Task SelectionChangedAsync(HashSet<FileExplorerItem> selected)
    {
        _selectedItems.Clear();
        foreach (var item in selected)
        {
            _selectedItems.Add(item);
        }

        await SelectedItemsChanged.InvokeAsync(_selectedItems);
    }

    private async Task OpenItemAsync(FileExplorerItem item)
    {
        if (item.IsDirectory)
        {
            await LoadFolderAsync(item.Id);
            return;
        }

        await ItemOpened.InvokeAsync(new FileExplorerItemEventArgs(item));
    }

    private async Task CreateFolderAsync()
    {
        var name = await PromptForNameAsync("New folder", "Folder name", "New folder");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        await RunOperationAsync("Create folder", null, () => Provider.CreateFolderAsync(_currentFolderId, name));
    }

    private async Task CreateFileAsync()
    {
        var name = await PromptForNameAsync("New file", "File name", "New file.txt");
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var success = await RunOperationAsync("Create file", null, () => Provider.CreateFileAsync(_currentFolderId, name));
        if (success)
        {
            await SelectItemByNameAsync(name, isDirectory: false);
        }
    }

    private async Task SelectItemByNameAsync(string name, bool isDirectory)
    {
        var item = (_listing?.Items ?? [])
            .FirstOrDefault(i => i.IsDirectory == isDirectory && string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        if (item is null)
        {
            return;
        }

        _currentItem = item;
        _selectedItems.Clear();
        _selectedItems.Add(item);
        await CurrentItemChanged.InvokeAsync(_currentItem);
        await SelectedItemsChanged.InvokeAsync(_selectedItems);
    }

    private async Task RenameAsync(FileExplorerItem item)
    {
        var name = await PromptForNameAsync("Rename", "Name", item.Name);
        if (string.IsNullOrWhiteSpace(name) || string.Equals(name, item.Name, StringComparison.Ordinal))
        {
            return;
        }

        await RunOperationAsync("Rename", item, () => Provider.RenameAsync(item.Id, name));
    }

    private async Task MoveAsync(FileExplorerItem item)
    {
        var folders = await Provider.ListMoveTargetsAsync();
        var parameters = new DialogParameters<FileExplorerMoveDialog>
        {
            { dialog => dialog.ItemName, item.Name },
            { dialog => dialog.Folders, folders },
            { dialog => dialog.CurrentParentId, item.ParentId }
        };

        var dialog = await DialogService.ShowAsync<FileExplorerMoveDialog>("Move", parameters, DialogOptions());
        var result = await dialog.Result;
        if (result is null || result.Canceled || result.Data is not string destinationId)
        {
            return;
        }

        await RunOperationAsync("Move", item, () => Provider.MoveAsync(item.Id, destinationId));
    }

    private async Task DeleteAsync(FileExplorerItem item)
    {
        var parameters = new DialogParameters<FileExplorerConfirmDialog>
        {
            { dialog => dialog.Message, $"Delete '{item.Name}'? This cannot be undone." },
            { dialog => dialog.ConfirmText, "Delete" },
            { dialog => dialog.ConfirmColor, Color.Error }
        };

        var dialog = await DialogService.ShowAsync<FileExplorerConfirmDialog>("Delete", parameters, DialogOptions());
        var confirmed = await dialog.Result;
        if (confirmed is null || confirmed.Canceled || confirmed.Data is not true)
        {
            return;
        }

        await RunOperationAsync("Delete", item, () => Provider.DeleteAsync(item.Id));
    }

    private async Task DownloadAsync(FileExplorerItem item)
    {
        _error = null;
        _operationText = $"Downloading {item.Name}...";

        try
        {
            var download = await Provider.DownloadAsync(item.Id);
            await using var content = download.Content;
            using var streamRef = new DotNetStreamReference(content);

            var module = await GetJsModuleAsync();
            await module.InvokeVoidAsync("downloadFileFromStream", download.FileName, download.ContentType, streamRef);

            _operationText = $"Downloaded {download.FileName}";
            await OperationCompleted.InvokeAsync(new FileExplorerOperationEventArgs("Download", item, FileExplorerOperationResult.Ok()));
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            _operationText = "Download failed";
            await OperationCompleted.InvokeAsync(new FileExplorerOperationEventArgs("Download", item, FileExplorerOperationResult.Fail(ex.Message)));
        }
    }

    private async Task OpenInNewTabAsync(FileExplorerItem item)
    {
        _error = null;
        _operationText = $"Opening {item.Name}...";

        try
        {
            var download = await Provider.DownloadAsync(item.Id);
            await using var content = download.Content;
            using var streamRef = new DotNetStreamReference(content);

            var module = await GetJsModuleAsync();
            await module.InvokeVoidAsync("openFileFromStream", download.ContentType, streamRef);

            _operationText = $"Opened {download.FileName}";
            await OperationCompleted.InvokeAsync(new FileExplorerOperationEventArgs("Open", item, FileExplorerOperationResult.Ok()));
        }
        catch (Exception ex)
        {
            _error = ex.Message;
            _operationText = "Open failed";
            await OperationCompleted.InvokeAsync(new FileExplorerOperationEventArgs("Open", item, FileExplorerOperationResult.Fail(ex.Message)));
        }
    }

    private static bool CanOpenInNewTab(FileExplorerItem item)
    {
        var contentType = item.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IJSObjectReference> GetJsModuleAsync()
    {
        _jsModule ??= await JsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/MuddyFileExplorer/muddyFileExplorer.js");
        return _jsModule;
    }

    private async Task QueueUploadsAsync(IReadOnlyList<IBrowserFile>? files)
    {
        if (files is null || files.Count == 0)
        {
            return;
        }

        var startIndex = _uploads.Count;
        foreach (var file in files)
        {
            _uploads.Add(new FileExplorerUploadItem
            {
                Name = file.Name,
                Size = file.Size
            });
        }

        await ProcessUploadsAsync(files, startIndex);
    }

    private async Task ProcessUploadsAsync(IReadOnlyList<IBrowserFile> files, int startIndex)
    {
        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            var upload = _uploads[startIndex + index];
            upload.Status = FileExplorerUploadStatus.Uploading;
            upload.Message = "Uploading";
            _operationText = $"Uploading {file.Name}";

            var progress = new Progress<long>(bytes =>
            {
                upload.BytesUploaded = bytes;
                InvokeAsync(StateHasChanged);
            });

            var result = await Provider.UploadAsync(_currentFolderId, file, MaxUploadFileSize, progress);
            upload.Message = result.Message;
            upload.Status = result.Success ? FileExplorerUploadStatus.Complete : FileExplorerUploadStatus.Failed;
            upload.BytesUploaded = result.Success ? upload.Size : upload.BytesUploaded;

            await OperationCompleted.InvokeAsync(new FileExplorerOperationEventArgs("Upload", null, result));
            StateHasChanged();
        }

        _operationText = "Upload queue complete";
        await RefreshAsync();
    }

    private async Task<bool> RunOperationAsync(string name, FileExplorerItem? item, Func<Task<FileExplorerOperationResult>> operation)
    {
        _busy = true;
        _error = null;
        _operationText = $"{name}...";

        FileExplorerOperationResult result;
        try
        {
            result = await operation();
            if (!result.Success)
            {
                _error = result.Message;
            }
        }
        catch (Exception ex)
        {
            result = FileExplorerOperationResult.Fail(ex.Message);
            _error = ex.Message;
        }
        finally
        {
            _busy = false;
        }

        _operationText = result.Success ? $"{name} complete" : $"{name} failed";
        await OperationCompleted.InvokeAsync(new FileExplorerOperationEventArgs(name, item, result));

        if (result.Success)
        {
            await RefreshAsync();
        }

        return result.Success;
    }

    private async Task<string?> PromptForNameAsync(string title, string label, string value)
    {
        var parameters = new DialogParameters<FileExplorerNameDialog>
        {
            { dialog => dialog.Label, label },
            { dialog => dialog.Value, value }
        };

        var dialog = await DialogService.ShowAsync<FileExplorerNameDialog>(title, parameters, DialogOptions());
        var result = await dialog.Result;
        return result is { Canceled: false, Data: string name } ? name.Trim() : null;
    }

    private static DialogOptions DialogOptions() => new()
    {
        CloseButton = true,
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.ExtraSmall,
        FullWidth = true
    };

    private static string GetIcon(FileExplorerItem item)
    {
        if (item.IsDirectory)
        {
            return Icons.Material.Filled.Folder;
        }

        return item.Type.ToLowerInvariant() switch
        {
            "image" => Icons.Material.Filled.Image,
            "pdf" => Icons.Material.Filled.PictureAsPdf,
            "text" or "markdown" => Icons.Material.Filled.Article,
            "archive" => Icons.Material.Filled.FolderZip,
            "spreadsheet" => Icons.Material.Filled.TableChart,
            "code" => Icons.Material.Filled.Code,
            _ => Icons.Material.Filled.InsertDriveFile
        };
    }

    private static Color GetIconColor(FileExplorerItem item)
    {
        if (item.IsDirectory)
        {
            return Color.Warning;
        }

        return item.Type.ToLowerInvariant() switch
        {
            "image" => Color.Success,
            "pdf" => Color.Error,
            "code" => Color.Info,
            _ => Color.Default
        };
    }

    private static string FormatSize(FileExplorerItem item) => item.IsDirectory ? "-" : FormatBytes(item.Size ?? 0);

    private static string FormatBytes(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} B" : $"{value:0.#} {units[unit]}";
    }

    private bool AllUploadsComplete => _uploads.Count > 0 && _uploads.All(u =>
        u.Status is FileExplorerUploadStatus.Complete or FileExplorerUploadStatus.Failed or FileExplorerUploadStatus.Canceled);

    private int ConsolidatedUploadPercent
    {
        get
        {
            if (_uploads.Count == 0) return 0;
            var totalSize = _uploads.Sum(u => u.Size);
            var totalUploaded = _uploads.Sum(u => u.BytesUploaded);
            return totalSize <= 0 ? 0 : (int)Math.Min(100, Math.Round(totalUploaded * 100d / totalSize));
        }
    }

    private Color GetConsolidatedUploadColor()
    {
        if (_uploads.Any(u => u.Status == FileExplorerUploadStatus.Failed)) return Color.Error;
        if (AllUploadsComplete) return Color.Success;
        return Color.Primary;
    }

    private string GetConsolidatedUploadText()
    {
        var completed = _uploads.Count(u => u.Status is FileExplorerUploadStatus.Complete or FileExplorerUploadStatus.Failed or FileExplorerUploadStatus.Canceled);
        var total = _uploads.Count;
        var current = _uploads.FirstOrDefault(u => u.Status == FileExplorerUploadStatus.Uploading);

        if (AllUploadsComplete)
        {
            var failed = _uploads.Count(u => u.Status == FileExplorerUploadStatus.Failed);
            var totalBytes = _uploads.Where(u => u.Status == FileExplorerUploadStatus.Complete).Sum(u => u.Size);
            return failed > 0
                ? $"{completed}/{total} done ({failed} failed)"
                : $"{total}/{total} done · {FormatBytes(totalBytes)}";
        }

        return current is not null
            ? $"{completed + 1}/{total} · {current.Name}"
            : $"{completed}/{total}";
    }

    private async Task DismissUploadsAsync()
    {
        _uploads.Clear();
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.DisposeAsync();
            _jsModule = null;
        }
    }
}
