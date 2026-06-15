---
type: plan
description: "Plan 086 - Replace FileBrowser2 MudTreeView with MuddyFileExplorer component"
status: active
---

# Plan 086: Replace FileBrowser2 MudTreeView with MuddyFileExplorer

**Status:** active
**Created:** 2026-05-22T14:30:00+02:00
**Updated:** 2026-05-22T14:30:00+02:00
**Classification:** implementation plan
**Depends on:** plan085 (MuddyFileExplorer RCL component — mostly complete)

## Goal

Replace the existing `FileBrowser2.razor` page's `MudTreeView`-based file browser with the `MuddyFileExplorer` RCL component built in plan 085. The consumer page retains its right-side preview/edit/extraction panel; the left panel switches from tree navigation to flat folder navigation with breadcrumbs.

## Context / Why

Plan 085 built a standalone `MuddyFileExplorer` RCL component with CRUD, upload, breadcrumbs, multi-selection, and `CurrentItem` tracking. The component is functionally complete and tested in its sample app. Now we need to integrate it into CatHerder.Web's `FileBrowser2` page.

The existing `FileBrowser2` uses `MudTreeView` with lazy-loaded hierarchical tree data, single-path selection, file preview/edit, document extraction, folder indexing, multi-space support, and URL sync. The migration must preserve all these capabilities while switching the navigation paradigm from tree to flat folder view.

### Design Decisions (from fit/gap analysis)

| # | Decision | Rationale |
|---|----------|-----------|
| 1 | Preview/edit stays in consumer page right panel | By design — MuddyFileExplorer is a file list, not a viewer |
| 2 | Extraction/indexing stays in consumer page right panel | Option A — no changes to MuddyFileExplorer |
| 3 | Accept flat folder navigation (no tree sidebar) | Option A — standard file explorer paradigm |
| 4 | Space awareness lives in provider, not component | Provider is scoped to one `FileSpaceInfo` |
| 5 | URL sync via `NavigateToFileAsync` + consumer wiring | Consumer listens to `CurrentItemChanged`, syncs URL |
| 6 | Consumer enriches `FileExplorerItem` → `FileBrowserSelectionMetadata` | Consumer calls `GetSelectionMetadataAsync` on `CurrentItemChanged` |

## Pre-existing Issues to Fix First

Before starting integration tasks, two issues from prior ad-hoc work must be fixed:

1. **MuddyFileExplorer.razor has `NavigateToFileAsync` in markup section** — C# code was inserted between `</MudFileUpload>` and `<MudSpacer />` instead of inside `@code { }`. This breaks the build.
2. **CatHerderFileExplorerProvider.cs has critical bugs** — `.Result` sync-over-async deadlock, missing path validation, lost file metadata, `Path.Combine` backslashes in breadcrumbs. This file needs a full rewrite.

## Solution Design

### Architecture

```
FileBrowser2.razor (consumer page)
├── Space selector (tabs or dropdown)
├── MudSplitPanel
│   ├── FirstPanel: MuddyFileExplorer
│   │   └── Provider = CatHerderFileExplorerProvider(space)
│   │       └── wraps FileBrowser2Service
│   └── SecondPanel: Preview/Edit/Extraction
│       ├── FileViewerComponent
│       ├── WrappedToast (markdown editor)
│       └── Extraction/Indexing buttons + dialog
└── URL sync (?path= query parameter)
```

### CatHerderFileExplorerProvider

Adapter implementing `IFileExplorerProvider`, scoped to one `FileSpaceInfo`:

- `ListFolderAsync` → calls `FileBrowser2Service.GetChildItemsAsync`, maps `TreeItemData<string>` → `FileExplorerItem` with **full metadata** (size, modified date, type classification, content type, download URL)
- `ListMoveTargetsAsync` → recursively enumerates folders (async, no `.Result`)
- CRUD operations → delegate to `FileBrowser2Service` (new methods) with **path validation**
- `UploadAsync` → stream-based write with progress reporting

### FileBrowser2Service Extensions

Add CRUD methods to the existing service so the provider doesn't bypass validation:

- `CreateFolderAsync(space, parentPath, name)`
- `RenameItemAsync(space, relativePath, newName)`
- `MoveItemAsync(space, relativePath, destinationFolderPath)`
- `DeleteItemAsync(space, relativePath)`
- `UploadFileAsync(space, folderPath, IBrowserFile, maxFileSize, progress, cancellationToken)`

These reuse the existing `ResolveAndValidatePath` / `ResolveAndValidateFilePath` security checks.

### Consumer Page Refactor

- Replace `MudTreeView` + `LoadServerData` with `MuddyFileExplorer` + `CatHerderFileExplorerProvider`
- On `CurrentItemChanged`: call `GetSelectionMetadataAsync` → populate right panel
- On space change: recreate provider with new `FileSpaceInfo`
- On page load with `?path=`: parse path, navigate to parent folder, set current item
- On navigation/selection: sync URL query parameter

### MuddyFileExplorer Enhancement

- Fix `NavigateToFileAsync` placement (move to `@code` block)
- Enhance `NavigateToFileAsync` to accept optional `itemId` parameter — after loading folder, set `_currentItem` to the matching item and invoke `CurrentItemChanged`

## Tasks

- [ ] Task 086-01: Fix MuddyFileExplorer.razor — move NavigateToFileAsync to @code block, enhance with itemId selection
- [ ] Task 086-02: Add CRUD methods to FileBrowser2Service (with path validation)
- [ ] Task 086-03: Rewrite CatHerderFileExplorerProvider with proper async, validation, and full metadata
- [ ] Task 086-04: Add MuddyFileExplorer RCL reference to CatHerder.Web.csproj
- [ ] Task 086-05: Refactor FileBrowser2.razor — replace MudTreeView with MuddyFileExplorer, wire events, keep right panel
- [ ] Task 086-06: Add space selector and provider recreation logic
- [ ] Task 086-07: Implement URL sync (NavigateToFileAsync on load, CurrentItemChanged → URL update)
- [ ] Task 086-08: Build, smoke test, and verify all flows

## Acceptance Criteria

- [ ] MuddyFileExplorer.razor builds cleanly (NavigateToFileAsync in @code block)
- [ ] CatHerderFileExplorerProvider uses async throughout (no .Result), validates paths, populates full metadata
- [ ] FileBrowser2Service has CRUD methods with path traversal protection
- [ ] CatHerder.Web references MuddyFileExplorer RCL
- [ ] FileBrowser2 page uses MuddyFileExplorer instead of MudTreeView
- [ ] Right panel still shows preview, edit, extraction, indexing
- [ ] Space switching works (system/user/global)
- [ ] URL sync works (?path= deep links to correct folder+item)
- [ ] All existing flows still work: navigation, selection, preview, edit, extraction, indexing, CRUD, upload
- [ ] Solution builds and runs without errors

## Notes

- Plan 085 (MuddyFileExplorer RCL) is a prerequisite. Its component is functionally complete; only the NavigateToFileAsync fix is needed here.
- The `CatHerderFileExplorerProvider.cs` file created during ad-hoc execution has critical bugs and will be fully rewritten in Task 086-03.
- The navigation paradigm shift (tree → flat) is intentional. Breadcrumbs replace tree navigation.
- Extraction and indexing remain in the consumer page. No changes to MuddyFileExplorer for these features.
