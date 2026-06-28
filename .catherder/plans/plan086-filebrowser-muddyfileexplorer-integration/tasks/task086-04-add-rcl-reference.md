---
type: task
description: "Task 086-04 — Add MuddyFileExplorer RCL project reference to CatHerder.Web"
---
# Task 086-04: Add RCL Reference to CatHerder.Web

**Status:** `done`

**Created:** 2026-05-22T14:30:00+02:00

**Updated:** 2026-05-22T14:30:00+02:00

## Task ID

086-04

## Objective

Add a `ProjectReference` to the `MuddyFileExplorer` RCL in `CatHerder.Web.csproj` and add the static CSS link in the app's layout or `App.razor`.

## Scope

**Included:**
- Add `<ProjectReference>` to `CatHerder.Web.csproj`
- Add `<link>` for `MuddyFileExplorer.css` static asset in the appropriate place
- Verify the RCL's `wwwroot` assets are accessible

**Excluded:**
- No code changes to the RCL
- No consumer page changes yet

## Steps

1. Add to `CatHerder.Web.csproj` `<ItemGroup>`:
   ```xml
   <ProjectReference Include="..\..\submodules\MuddyFileExplorer\src\MuddyFileExplorer\MuddyFileExplorer.csproj" />
   ```
2. Add CSS link in `App.razor` or `Components/App.razor` (wherever MudBlazor CSS is referenced):
   ```html
   <link rel="stylesheet" href="_content/MuddyFileExplorer/MuddyFileExplorer.css" />
   ```
3. Build CatHerder.Web to verify the reference resolves

## Verification

- `dotnet build` on CatHerder.Web succeeds
- RCL static assets are available at `_content/MuddyFileExplorer/`
