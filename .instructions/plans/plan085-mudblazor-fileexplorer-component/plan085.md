---
type: plan
description: "Plan 085 - Create a standalone MudBlazor FileExplorer component and sample app"
status: active
---

# Plan 085: MudBlazor FileExplorer Component

**Status:** active
**Created:** 2026-05-22T04:06:14+02:00
**Updated:** 2026-05-22T04:06:14+02:00
**Classification:** implementation plan

## Goal

Create a standalone MudBlazor-based FileExplorer component under `submodules/`, plus one sample application that uses it.

The result should be a fast, simple, compact Google Drive-like file browser with upload/download, file operation menus, drag-and-drop upload, queued upload progress, and a status bar with selected-file information.

## Context / Why

CatHerder.Web already uses MudBlazor and benefits from compact operational UI. A first-party MudBlazor FileExplorer package can provide a maintainable reusable file browsing surface without depending on a larger third-party file manager widget.

The package should be easy to consume from CatHerder.Web later, but it should remain standalone: its core should live in `submodules/`, expose a clean component/service contract, and include one sample app that proves the component end to end.

## Research Findings

See [data/research-notes.md](data/research-notes.md) for sources and detailed notes. Key findings:

- MudBlazor latest release checked through GitHub API was `v9.4.0`, published 2026-04-22.
- `MudDataGrid<T>` is the likely primary list component because it supports templated/property columns, selection, sorting, server data patterns, and dense list use.
- `MudFileUpload<T>` is the right upload primitive; current MudBlazor release notes mention first-class drag callbacks and drag-state reset work.
- `MudMenu` / `MudMenuItem` support action menus, and MudBlazor exposes right-click menu activation via `MouseEvent.RightClick`.
- `MudProgressLinear`, `MudDialog`, `MudBreadcrumbs`, `MudToolBar`, `MudIconButton`, `MudTextField`, and `MudTreeView<T>` cover the remaining expected UI with minimal custom styling.

## Solution Design

### Package Shape

Create a standalone solution under `submodules/MuddyFileExplorer`:

- `MuddyFileExplorer` Razor class library
- `MuddyFileExplorer.Sample` sample app
- package/readme documentation
- sample sandbox storage folder or generated demo data

Execution should decide whether this starts as a regular folder or a true git submodule based on repository/remote availability.

### Component Surface

Build a reusable `MuddyFileExplorer` component with parameters/events such as:

- `Provider` or equivalent file operation service
- `CurrentFolderId` / initial folder
- `Dense`, default `true`
- `MultiSelection`
- `AllowUpload`, `AllowDownload`, `AllowRename`, `AllowMove`, `AllowDelete`, `AllowCreateFolder`
- `MaxUploadFileSize`
- `AcceptedFileTypes`
- operation callbacks/events such as selected item changed, item opened, operation completed, operation failed

Use a UI-facing model that represents stable item IDs, display names, sizes, content type/file kind, icon hints, modified timestamp, folder/file flag, and permissions/capabilities. Do not expose physical paths.

### UI Layout

Use MudBlazor defaults and standard controls:

- Top toolbar with upload, new folder, refresh, search/filter, view density if needed, and overflow actions.
- Breadcrumbs for navigation.
- Compact `MudDataGrid<T>` list with icon, file name, size, type, modified date, and row/actions menu.
- Context menu for rename, move, delete, download, copy name/path/link where meaningful.
- Inline upload drop target integrated with `MudFileUpload<T>`.
- Upload queue panel or collapsible compact section with per-file status and progress.
- Bottom status bar with selected item metadata, selected count, folder item count, total size, and current operation.

### Backend / Provider Contract

Define a small provider contract for:

- list folder contents
- create folder
- rename item
- move item
- delete item
- open/download file
- upload one or more files with progress/cancellation
- refresh/reload state

The sample provider should use a sandbox directory and must normalize paths, prevent traversal, limit upload sizes, and return clear operation errors.

## Tasks

- [ ] Task 085-01: Verify current MudBlazor APIs and package baseline before implementation
- [ ] Task 085-02: Scaffold standalone submodule solution and sample app
- [ ] Task 085-03: Define FileExplorer DTOs, provider contract, and sample sandbox provider
- [ ] Task 085-04: Build compact MudBlazor FileExplorer UI
- [ ] Task 085-05: Implement file operations, dialogs, context menu, and status bar
- [ ] Task 085-06: Implement drag-and-drop upload queue, progress, cancellation, and error display
- [ ] Task 085-07: Add sample app page, demo data, and package documentation
- [ ] Task 085-08: Build, smoke test, and document verification results

## Acceptance Criteria

- [ ] A standalone FileExplorer solution exists under `submodules/`.
- [ ] The reusable component is built from MudBlazor components and can be consumed by the sample app.
- [ ] The main view is compact/dense and uses columns for icon, file name, size, type, and modified date.
- [ ] Users can navigate folders with breadcrumbs and refresh the current folder.
- [ ] Users can upload files by button and drag-and-drop.
- [ ] Uploads are queued, show per-file and/or aggregate progress, and expose completed/failed states.
- [ ] Users can download files.
- [ ] Context menu actions cover rename, move, delete, and download.
- [ ] Rename, move, and delete use appropriate dialogs/confirmation.
- [ ] Status bar shows useful information for selected files and current folder state.
- [ ] Sample app uses a sandboxed storage provider and prevents path traversal.
- [ ] Documentation explains component usage, provider contract, sample setup, and known limitations.
- [ ] The solution builds successfully and the sample can be run locally.

## Notes

- This plan is intentionally not executed when created. Implementation starts only when a future execution request selects a task.
- Prefer `MudDataGrid<T>` for the first implementation unless verification shows `MudTable` is materially simpler or faster for this use case.
- Keep drag-and-drop upload in v1. Drag-and-drop move between folders is out of scope for the first version unless it falls out naturally without complexity.
- Search/filter is useful but secondary to core browsing and file operations.
- The current project phase is Prototype, so favor a working, understandable component and sample over production-grade breadth.
