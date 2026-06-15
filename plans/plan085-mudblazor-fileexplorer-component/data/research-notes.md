# Plan 085 Research Notes: MudBlazor FileExplorer Component

**Created:** 2026-05-22T04:06:14+02:00
**Updated:** 2026-05-22T04:06:14+02:00

## Sources Checked

- MudBlazor `MudFileUpload<T>` API page: https://mudblazor.com/api/MudFileUpload%601
- MudBlazor File Upload component page: https://mudblazor.com/components/fileupload
- MudBlazor `MudDataGrid<T>` docs/source examples: https://mudblazor.com/components/datagrid and https://github.com/MudBlazor/MudBlazor
- MudBlazor `MudMenuItem` API page: https://mudblazor.com/api/MudMenuItem
- MudBlazor latest GitHub release API: https://api.github.com/repos/MudBlazor/MudBlazor/releases/latest
- MudBlazor release v9.4.0: https://github.com/MudBlazor/MudBlazor/releases/tag/v9.4.0
- MudBlazor README setup notes: https://github.com/MudBlazor/MudBlazor
- MudBlazor docs source basic DataGrid example: https://raw.githubusercontent.com/MudBlazor/MudBlazor/dev/src/MudBlazor.Docs/Pages/Components/DataGrid/Examples/DataGridBasicExample.razor
- ASP.NET Core Blazor file upload guidance: https://learn.microsoft.com/en-us/aspnet/core/blazor/file-uploads

Note: the rendered MudBlazor documentation pages returned a client-side "Unhandled error" through the browser tool during research. Search/API snippets, official GitHub source, and release metadata were used as the documentation trail.

## Current MudBlazor Baseline

MudBlazor latest release checked through GitHub API was `v9.4.0`, published 2026-04-22. Release notes mention:

- `MudDataGrid` breaking change around grid-level icon customization.
- `MudDataGrid` accessibility/filter improvements.
- `MudFileUpload` first-class drag callbacks while preserving internal drag-state reset.
- `MudMenu` right-click/activator bug fix.

Execution should use the repository's package baseline if one already exists, but the implementation task should check the current installed/latest MudBlazor version before coding because component APIs changed in v9.

## MudBlazor Component Choices

- Main file list: `MudDataGrid<T>` for the Google Drive-like list, using `Dense="true"`, `Hover="true"`, compact column templates, single/multi selection, sortable columns, and optional `ServerData` for large directories. Use `MudTable` only if the DataGrid proves too heavy for the target package.
- Columns: icon/template column, file name, size, type, modified date, and actions/context-menu column.
- Toolbar: `MudToolBar`, `MudIconButton`, `MudButton`, `MudTextField`, `MudBreadcrumbs`, and `MudSpacer`.
- Context menu: `MudMenu` / `MudMenuItem`, including right-click activation where supported. Include item actions for rename, move, delete, download, copy path/link, and new folder where appropriate.
- Dialogs: `MudDialog` for rename, move destination picker, delete confirmation, and upload queue details if the inline queue grows too large.
- Upload: `MudFileUpload<T>` with drag-and-drop support and app-owned upload queue state. Track queued, uploading, complete, failed, canceled.
- Progress: `MudProgressLinear` for total and per-file upload progress, with compact inline progress rows.
- Status bar: `MudPaper` or `MudStack` styled via MudBlazor defaults/utility classes to show selected file count, selected item metadata, directory item count, total size, and current operation.
- Folder picker/move target: `MudTreeView<T>` or a compact dialog with breadcrumbs plus list, depending on how much hierarchy the first version needs.
- Notifications: `ISnackbar` in the sample for completed actions and failures; the component itself should expose callbacks/results rather than hard-depend on app-level snackbars.

## Design Implications

- Keep the reusable component MudBlazor-first and CSS-light.
- Prefer a compact table/list experience over a card/gallery view for v1.
- Keep domain/file operations behind a small provider contract so the component is standalone and the sample can use local disk storage safely.
- Avoid embedding physical paths in UI-facing DTOs or requests.
- Use Blazor upload guidance for stream limits, cancellation, and progress accounting.
- Treat drag-and-drop as upload input only for v1. File/folder move by drag/drop can be a future feature after rename/move/delete/download are stable.
