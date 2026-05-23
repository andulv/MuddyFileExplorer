---
type: task
description: "Task 086-05 — Refactor FileBrowser2.razor to use MuddyFileExplorer, wire events, keep right panel"
---
# Task 086-05: Refactor FileBrowser2 Page

**Status:** `done`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-05

## Objective

Replace the `MudTreeView` in `FileBrowser2.razor` with `MuddyFileExplorer`, wire up `CurrentItemChanged` and `SelectedItemsChanged` events, and keep the right-side preview/edit/extraction panel intact.

## Scope

**Included:**
- Replace `MudTreeView` + `LoadServerData` with `MuddyFileExplorer` component
- Create `CatHerderFileExplorerProvider` instance scoped to active `FileSpaceInfo`
- Wire `CurrentItemChanged` → load `FileBrowserSelectionMetadata` → populate right panel
- Wire `SelectedItemsChanged` → update selection state if needed
- Keep right panel: `FileViewerComponent`, `WrappedToast`, extraction buttons, indexing dialog
- Remove tree-specific code: `_treeItems`, `LoadServerData`, `OnSelectedPathChanged`
- Add `_fileExplorerRef` for calling `NavigateToFileAsync`

**Excluded:**
- Space selector (Task 086-06)
- URL sync (Task 086-07)

## Steps

1. In `FileBrowser2.razor.cs`:
   - Remove `_treeItems`, `LoadServerData`, `OnSelectedPathChanged` tree-specific code
   - Add `_provider` field (`CatHerderFileExplorerProvider`)
   - Add `_fileExplorerRef` field (`MuddyFileExplorer?` with `@ref`)
   - Add `OnCurrentItemChangedAsync(FileExplorerItem item)` — calls `GetSelectionMetadataAsync`, populates `_selectedSelection`, loads content
   - Add `CreateProvider()` — creates `CatHerderFileExplorerProvider(_fileBrowser2Service, _activeSpace)`
   - Update `RefreshTreeCoreAsync` to use `_fileExplorerRef?.RefreshAsync()` or recreate provider
2. In `FileBrowser2.razor`:
   - Replace `MudTreeView` block with:
     ```razor
     <MuddyFileExplorer @ref="_fileExplorerRef"
                      Provider="_provider"
                      Dense="true"
                      MultiSelection="false"
                      AllowUpload="true"
                      AllowDownload="true"
                      AllowRename="true"
                      AllowMove="true"
                      AllowDelete="true"
                      AllowCreateFolder="true"
                      CurrentItemChanged="OnCurrentItemChangedAsync" />
     ```
   - Keep right panel markup unchanged
3. Build and verify basic navigation and selection

## Verification

- Page loads with MuddyFileExplorer showing root folder contents
- Clicking a row sets CurrentItem and updates right panel
- Double-clicking a folder navigates into it
- Right panel shows preview/edit/extraction as before
- `dotnet build` succeeds
