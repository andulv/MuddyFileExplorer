---
type: task
description: "Task 086-08 — End-to-end smoke test of the integrated FileBrowser2 page"
---
# Task 086-08: End-to-End Smoke Test

**Status:** `not-started`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-08

## Objective

Run the CatHerder.Web app and verify all file browser functionality works end-to-end with the new `MuddyFileExplorer` integration.

## Scope

**Included:**
- Manual smoke test of all operations
- Verify no regressions vs. old tree-based browser

**Excluded:**
- Automated tests (future work)

## Steps

1. Start CatHerder.Web
2. Navigate to `/files`
3. Verify:
   - [ ] Root folder contents load
   - [ ] Breadcrumbs show and navigate correctly
   - [ ] Double-click folder navigates into it
   - [ ] Single-click file selects it and shows right panel
   - [ ] Right panel shows preview for text/markdown/images
   - [ ] Right panel shows edit for writable text files
   - [ ] Create folder works
   - [ ] Rename works
   - [ ] Move works (with move target selection)
   - [ ] Delete works (with confirmation)
   - [ ] Upload works
   - [ ] Download works
   - [ ] Space selector switches spaces
   - [ ] URL sync works (navigate to file via URL)
   - [ ] Browser back/forward works
4. Fix any issues found

## Verification

- All checklist items pass
- No console errors
- No server-side exceptions
