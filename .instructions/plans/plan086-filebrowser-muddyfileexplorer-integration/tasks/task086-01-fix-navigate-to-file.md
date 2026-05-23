---
type: task
description: "Task 086-01 — Fix NavigateToFileAsync placement in MuddyFileExplorer and enhance with itemId selection"
---
# Task 086-01: Fix NavigateToFileAsync in MuddyFileExplorer

**Status:** `done`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-01

## Objective

Fix the broken `NavigateToFileAsync` method that was inserted into the HTML markup section of `MuddyFileExplorer.razor` instead of the `@code` block, and enhance it to support selecting a specific item after navigating to a folder.

## Scope

**Included:**
- Remove the misplaced C# code from the markup section (lines between `</MudFileUpload>` and `<MudSpacer />`)
- Add `NavigateToFileAsync(string? folderId, string? itemId = null)` to the `@code` block
- After loading folder, if `itemId` is provided, find matching item in listing and set as `_currentItem`

**Excluded:**
- No other MuddyFileExplorer changes
- No consumer page changes

## Steps

1. Remove the misplaced `NavigateToFileAsync` code block from the markup section (between `</MudFileUpload>` and `<MudSpacer />`)
2. Add the method to the `@code` block after `OnInitializedAsync`:
   ```csharp
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
   ```
3. Build the RCL project to verify

## Verification

- `dotnet build` on `MuddyFileExplorer` succeeds
- No C# code in the markup section
- `NavigateToFileAsync` is accessible as a public method on the component
