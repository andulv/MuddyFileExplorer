---
type: task
description: "Task 086-06 — Add space selector and provider recreation logic"
---
# Task 086-06: Add Space Selector

**Status:** `done`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-06

## Objective

Add a space selector (tabs or dropdown) above the `MuddyFileExplorer` so users can switch between system/user/global file spaces, and recreate the provider when the space changes.

## Scope

**Included:**
- Space selector UI (MudTabs or MudSelect with system/user/global options)
- Provider recreation on space change
- Route handling for `/files/{spaceId}`

**Excluded:**
- URL sync (Task 086-07)
- No changes to MuddyFileExplorer component

## Steps

1. Add space selector above `MudSplitPanel` in `FileBrowser2.razor`:
   - Use `MudTabs` with one tab per `FileSpaceInfo` from `_currentUser.FileSpaces`
   - Or use `MudSelect` dropdown if spaces are many
2. On tab/select change:
   - Update `_activeSpace`
   - Call `CreateProvider()` to recreate provider with new space
   - Clear right panel selection
3. Handle route parameter `SpaceId` to set initial active space
4. Build and verify space switching

## Verification

- Space selector shows available spaces
- Switching spaces reloads the file list for that space
- Route `/files/{spaceId}` selects the correct space
- Right panel clears on space switch


