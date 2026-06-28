---
type: task
description: "Task 086-03 — Rewrite CatHerderFileExplorerProvider with proper async, validation, and full metadata"
---
# Task 086-03: Rewrite CatHerderFileExplorerProvider

**Status:** `done`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-03

## Objective

Rewrite the `CatHerderFileExplorerProvider` to fix critical bugs (sync-over-async deadlock, missing path validation, lost metadata, backslash breadcrumbs) and delegate CRUD to the new `FileBrowser2Service` methods from Task 086-02.

## Scope

**Included:**
- Full rewrite of `CatHerderFileExplorerProvider.cs`
- `ListFolderAsync` — populate `FileExplorerItem` with real size, modified date, type classification, content type, download URL
- `ListMoveTargetsAsync` — async recursive folder enumeration (no `.Result`)
- CRUD methods — delegate to `FileBrowser2Service` new methods
- `BuildBreadcrumbs` — use forward slashes consistently
- `TreeItemToFileExplorerItem` — map all available metadata from `FileSystemInfo`

**Excluded:**
- No changes to `IFileExplorerProvider` interface
- No changes to `FileExplorerItem` model

## Steps

1. Rewrite `ListFolderAsync`:
   - Call `GetChildItemsAsync` for the tree items
   - For each item, also get `FileSystemInfo` to populate `Size`, `ModifiedAt`, `ContentType`
   - Use `ClassifyPreviewKind` logic to set `Type` field (image, pdf, text, markdown, code, etc.)
   - Build download URL using `BuildRawPreviewUrl` pattern for previewable files
2. Rewrite `ListMoveTargetsAsync`:
   - Async recursive enumeration of all directories under root
   - Return `FileExplorerFolder` list with proper paths
3. Rewrite CRUD methods to delegate to `FileBrowser2Service`:
   - `CreateFolderAsync` → `_service.CreateFolderAsync(_space, parentFolderId, name)`
   - `RenameAsync` → `_service.RenameItemAsync(_space, itemId, newName)`
   - `MoveAsync` → `_service.MoveItemAsync(_space, itemId, destinationFolderId)`
   - `DeleteAsync` → `_service.DeleteItemAsync(_space, itemId)`
   - `UploadAsync` → `_service.UploadFileAsync(_space, folderId, file, maxAllowedSize, progress, cancellationToken)`
4. Fix `BuildBreadcrumbs` — use string concatenation with `/` instead of `Path.Combine`
5. Build CatHerder.Web to verify

## Verification

- No `.Result` calls anywhere in the provider
- All CRUD operations go through `FileBrowser2Service` (path validation)
- `FileExplorerItem` records have real `Size`, `ModifiedAt`, `Type`, `ContentType`, `DownloadUrl`
- Breadcrumbs use forward slashes
- `dotnet build` succeeds
