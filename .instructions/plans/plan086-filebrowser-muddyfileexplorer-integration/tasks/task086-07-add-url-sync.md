---
type: task
description: "Task 086-07 — Add URL sync: navigate to file from URL, update URL on navigation"
---
# Task 086-07: Add URL Sync

**Status:** `done`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-07

## Objective

Add URL synchronization so that navigating to a file/folder updates the browser URL, and loading the page with a path in the URL navigates directly to that item.

## Scope

**Included:**
- Update URL on `CurrentItemChanged` (replace URL with `/files/{spaceId}?path={relativePath}`)
- On page load, if `path` query param is present, call `NavigateToFileAsync(folderId, itemId)`
- Handle browser back/forward

**Excluded:**
- No changes to MuddyFileExplorer component
- No changes to provider

## Steps

1. In `OnCurrentItemChangedAsync`:
   - After updating right panel, call `_navigationManager.NavigateTo($"/files/{_activeSpace.Id}?path={item.Id}", forceLoad: false, replace: true)`
2. In `OnInitializedAsync` or `OnParametersSetAsync`:
   - Parse `path` query parameter from URL
   - If present, split into folder path and item name
   - Call `_fileExplorerRef?.NavigateToFileAsync(folderId, itemId)`
3. Handle `OnAfterRenderAsync` first-render to avoid double-navigation
4. Build and verify URL sync

## Verification

- Clicking a file updates the URL
- Loading the page with `?path=some/file.txt` navigates to that file
- Browser back/forward works
