---
type: project
description: "MuddyFileExplorer project-specific instructions"
alwaysApply: true
---
# Project Instructions - MuddyFileExplorer

MuddyFileExplorer is a standalone MudBlazor Razor Class Library for browsing
and operating on files through a host-provided provider contract.

Read first:
- [catherder.instructions.md](catherder.instructions.md)
- [../README.md](../README.md)

## Project Shape
- `src/MuddyFileExplorer` - reusable Razor class library.
- `samples/MuddyFileExplorer.Sample` - Blazor Web App sample host.
- `plans/` - CatHerder plan artifacts when needed.
- `doc/` - screenshots and usage/styling reference material.

## API Boundaries
- Keep file-system operations behind `IFileExplorerProvider`.
- UI-facing models should use opaque item IDs. Do not expose physical paths to
  the browser.
- Keep host-specific storage, routing, authentication, and authorization outside
  the reusable component unless exposed through provider methods or parameters.
- Preserve the component as a MudBlazor RCL; do not couple it to the main
  CatHerder application.

## Implementation Guidelines
- Read existing component, provider, and sample code before changing behavior.
- Follow the current MudBlazor patterns used by the component.
- Keep CSS overrides documented in `doc/styling.md`.
- Preserve provider-driven download/upload behavior.
- Treat the sample sandbox as demo code, not as the package security boundary.

## Commands
From this repository root:

- Build package: `dotnet build src/MuddyFileExplorer/MuddyFileExplorer.csproj`
- Build sample: `dotnet build samples/MuddyFileExplorer.Sample/MuddyFileExplorer.Sample.csproj`
- Run sample: `dotnet run --project samples/MuddyFileExplorer.Sample/MuddyFileExplorer.Sample.csproj`
