---
type: plan
description: "Plan 085 draft - Create a standalone MudBlazor FileExplorer component and sample app"
status: draft
---

# Plan 085 Draft: MudBlazor FileExplorer Component

**Status:** draft
**Created:** 2026-05-22T04:06:14+02:00
**Updated:** 2026-05-22T04:06:14+02:00
**Readiness Rank:** 86/100
**Classification:** implementation plan
**Promoted Plan:** [plan085.md](plan085.md)

## Goal

Create a new standalone MudBlazor-based FileExplorer component under `submodules/`, plus one sample application that demonstrates the component.

The component should feel like a compact Google Drive-style file browser: fast list/grid browsing, dense rows, recognizable file icons, upload/download support, row/context actions, drag-and-drop upload, queued uploads with progress, and a status bar that summarizes selection and current state.

## Context / Why

CatHerder already uses `submodules/` for reusable package-style UI work. A MudBlazor-native file explorer gives the project a simpler, more idiomatic alternative to heavier third-party file manager wrappers when the desired experience is compact, operational, and aligned with the existing CatHerder.Web MudBlazor stack.

The first version should prioritize a clean reusable component contract and a realistic sample backend over feature sprawl. The component should use MudBlazor defaults and standard controls wherever possible so it is easy to understand, theme, and maintain.

## What We Want To Achieve (Outcomes)

- A new standalone submodule package, tentatively named `MuddyFileExplorer`, under `submodules/`.
- A reusable Razor class library containing a MudBlazor FileExplorer component.
- One sample app that references the component and demonstrates local/sandboxed file browsing.
- A Google Drive-like compact list interface with columns for icon, file name, size, type, and modified date.
- Upload/download support through a clear provider/service contract.
- Drag-and-drop uploads with an upload queue and visible progress.
- Context actions for rename, move, delete, download, and related operations.
- Status bar showing selected file details, selection count, folder count/size, and operation status.
- Documentation for the component API, sample storage contract, and security assumptions.

## Summary Of Work Needed

Research the current MudBlazor component APIs before implementation, especially `MudDataGrid`, `MudFileUpload`, `MudMenu`, `MudDialog`, and progress components. Scaffold a standalone .NET solution under `submodules/MuddyFileExplorer` with a Razor class library and sample MudBlazor application.

Design a small file explorer domain model that does not leak physical paths to the UI. Implement a provider/service contract for listing folders, creating folders, renaming, moving, deleting, uploading, downloading, and optionally searching. Build the component with MudBlazor defaults: a compact toolbar, breadcrumbs, `MudDataGrid` file list, context menu actions, upload drop zone/queue, dialogs for destructive or naming operations, and a bottom status bar.

The sample app should use a sandbox folder, enforce path normalization/traversal protection, and include enough sample data to verify browsing, uploading, downloading, rename, move, delete, queue progress, and status behavior.

## Key Principles / Constraints

- Create plan artifacts only now; do not scaffold or implement the component until this plan is executed later.
- Keep the component standalone and reusable outside CatHerder.Web.
- Use MudBlazor components, defaults, and standard parameters before custom CSS.
- Keep custom CSS minimal, targeted, and owned by the reusable component only when needed.
- Prefer compact/dense UI: dense grid, small icon buttons, restrained spacing, and status text optimized for repeated use.
- Avoid over-engineering. Do not introduce interfaces/classes beyond what the reusable component and sample genuinely need.
- Do not expose physical server paths through UI DTOs, browser requests, or status messages.
- Treat upload stream limits, cancellation, errors, and path traversal protection as core behavior.
- Use file operation callbacks/results that host applications can observe, log, or override.
- Keep drag-and-drop upload in scope for v1; drag-and-drop move/reorder can be deferred.

## Open Questions

- Confirm final package/project name: `MuddyFileExplorer`, `MudBlazor.FileExplorer`, or another preferred name.
- Decide whether to initialize an actual git submodule immediately during execution or first create a normal folder until a remote exists.
- Decide whether v1 should use `MudDataGrid<T>` by default, with a fallback to `MudTable` if DataGrid overhead or APIs are awkward.
- Decide whether the sample app should be Blazor Server, Blazor Web App interactive server, or WebAssembly hosted. Blazor Server/Interactive Server is the likely first sample because file IO and progress are simpler.
- Decide whether search/filter is in v1 or left as a follow-up after core file operations are stable.
