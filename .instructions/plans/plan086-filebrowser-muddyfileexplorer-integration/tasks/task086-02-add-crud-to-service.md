---
type: task
description: "Task 086-02 — Add CRUD methods to FileBrowser2Service with path validation"
---
# Task 086-02: Add CRUD Methods to FileBrowser2Service

**Status:** `done`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-02

## Objective

Add create-folder, rename, move, delete, and upload methods to `FileBrowser2Service` so the provider adapter doesn't need to bypass the service's path validation and security checks.

## Scope

**Included:**
- `CreateFolderAsync(FileSpaceInfo, string? parentRelativePath, string name)`
- `RenameItemAsync(FileSpaceInfo, string relativePath, string newName)`
- `MoveItemAsync(FileSpaceInfo, string relativePath, string destinationFolderPath)`
- `DeleteItemAsync(FileSpaceInfo, string relativePath)`
- `UploadFileAsync(FileSpaceInfo, string? folderRelativePath, IBrowserFile file, long maxAllowedSize, IProgress<long>? progress, CancellationToken)`

**Excluded:**
- No changes to existing methods
- No UI changes

## Steps

1. Add `using Microsoft.AspNetCore.Components.Forms;` to `FileBrowser2Service.cs`
2. Add `CreateFolderAsync` — resolve+validate parent path, `Directory.CreateDirectory`, check for existing
3. Add `RenameItemAsync` — resolve+validate path, `Directory.Move` or `File.Move`
4. Add `MoveItemAsync` — resolve+validate source and destination, move
5. Add `DeleteItemAsync` — resolve+validate path, `Directory.Delete(recursive)` or `File.Delete`
6. Add `UploadFileAsync` — resolve+validate folder path, stream `IBrowserFile.OpenReadStream` to disk with progress reporting
7. All methods reuse existing `ResolveAndValidatePath` / `ResolveAndValidateFilePath` for traversal protection
8. Build CatHerder.Web to verify

## Verification

- `dotnet build` on CatHerder.Web succeeds
- All new methods validate paths before performing operations
- Path traversal attempts are rejected
