# MuddyFileExplorer

A standalone MudBlazor-based file explorer component with a compact list UI. Upload, copy, move, delete, etc.

## Projects

- `src/MuddyFileExplorer` - reusable Razor class library
- `samples/MuddyFileExplorer.Sample` - Blazor Web App sample using a sandboxed local file provider

## Features

- Dense MudBlazor file list using `MudDataGrid`
- Columns for icon, file name, size, type, and modified date
- Breadcrumb folder navigation
- Search/filter box
- Upload button and drag-and-drop upload area
- Queued uploads with per-file progress
- Download links supplied by the host provider
- Context actions for open, rename, move, delete, and download
- Status bar for selected item/folder summary

## Usage

Register MudBlazor in the host app:

```csharp
builder.Services.AddMudServices();
```

Include MudBlazor providers in the host layout:

```razor
<MudThemeProvider />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

Use the component with an `IFileExplorerProvider` implementation:

```razor
@using MuddyFileExplorer
@using MuddyFileExplorer.Components

<MuddyFileExplorer Provider="@FileProvider"
                 Dense="true"
                 MultiSelection="true"
                 MaxUploadFileSize="104857600" />
```

## Provider Contract

Hosts implement `IFileExplorerProvider` for file operations:

- list folder contents
- list move targets
- create folder
- rename item
- move item
- delete item
- upload file with progress

The UI-facing models use opaque item IDs. Providers should not expose physical paths to the browser.

## Sample

Run the sample from this folder:

```bash
dotnet run --project samples/MuddyFileExplorer.Sample/MuddyFileExplorer.Sample.csproj
```

The sample stores demo files under `samples/MuddyFileExplorer.Sample/App_Data/FileExplorerSandbox`. It normalizes file paths, rejects traversal outside the sandbox, and exposes downloads through `/download/{id}`.

