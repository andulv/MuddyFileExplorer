---
type: plan
description: "Plan 086 — Replace FileBrowser2 MudTreeView with MuddyFileExplorer RCL"
status: active
created: 2026-05-22T14:30:00+02:00
updated: 2026-05-22T14:30:00+02:00
---

# Plan 086: FileBrowser2 → MuddyFileExplorer Integration

## Goal

Replace the `MudTreeView`-based file browser in `FileBrowser2.razor` with the `MuddyFileExplorer` RCL component, preserving all existing functionality (preview, edit, extraction, indexing) while gaining CRUD operations, upload, breadcrumbs, and multi-selection.

## Context

- **MuddyFileExplorer RCL** was built in Plan 085 and is functionally complete
- **FileBrowser2.razor** currently uses `MudTreeView` with `LoadServerData` for tree navigation
- **FileBrowser2Service** has path validation and security but lacks CRUD methods
- **CatHerderFileExplorerProvider** exists but has critical bugs (sync-over-async, missing metadata, backslash breadcrumbs)

## Decisions (from fit/gap analysis)

| # | Gap | Decision |
|---|-----|----------|
| 1 | CRUD methods missing from service | Add to `FileBrowser2Service` with path validation |
| 2 | Provider rewrite needed | Full rewrite with async, metadata, forward-slash breadcrumbs |
| 3 | NavigateToFileAsync broken | Fix placement + add itemId selection |
| 4 | Space selector | Add MudTabs above explorer |
| 5 | URL sync | Add query-param sync |
| 6 | Right panel preservation | Keep existing panel, wire via CurrentItemChanged |

## Tasks

| Task | Description | Depends On | Status |
|------|-------------|------------|--------|
| [086-01](tasks/task086-01-fix-navigate-to-file.md) | Fix NavigateToFileAsync in MuddyFileExplorer | — | done |
| [086-02](tasks/task086-02-add-crud-to-service.md) | Add CRUD methods to FileBrowser2Service | — | done |
| [086-03](tasks/task086-03-rewrite-provider.md) | Rewrite CatHerderFileExplorerProvider | 086-02 | done |
| [086-04](tasks/task086-04-add-rcl-reference.md) | Add RCL reference to CatHerder.Web | — | done |
| [086-05](tasks/task086-05-refactor-filebrowser-page.md) | Refactor FileBrowser2 page | 086-01, 086-03, 086-04 | done |
| [086-06](tasks/task086-06-add-space-selector.md) | Add space selector | 086-05 | done |
| [086-07](tasks/task086-07-add-url-sync.md) | Add URL sync | 086-05 | done |
| [086-08](tasks/task086-08-smoke-test.md) | End-to-end smoke test | 086-05, 086-06, 086-07 | not-started |

## Execution Order

Tasks 086-01, 086-02, and 086-04 have no dependencies and can be done in parallel.
Task 086-03 depends on 086-02.
Task 086-05 depends on 086-01, 086-03, and 086-04.
Tasks 086-06 and 086-07 depend on 086-05 and can be done in parallel.
Task 086-08 is the final validation step.

```
086-01 ──┐
086-02 ──┼── 086-03 ──┐
086-04 ──┘             ├── 086-05 ──┬── 086-06 ──┐
                        └───────────┤── 086-07 ──┤── 086-08
                                    └─────────────┘
```
