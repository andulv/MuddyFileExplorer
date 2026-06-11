# MuddyFileExplorer Styling

`MuddyFileExplorer` uses MudBlazor `MudDataGrid`. Most density and typography
customization should be done with CSS overrides in the host app, not component
parameters.

The component exposes stable CSS hooks:

- `muddy-file-explorer-grid`
- `muddy-file-explorer-folder-icon-link`
- `muddy-file-explorer-folder-link`

## CSS Isolation

If the override lives in a global stylesheet, omit `::deep`.

Use `::deep` from CSS-isolated files when the styles must cross component
boundaries:

```css
::deep .muddy-file-explorer-grid {
    font-size: 0.8125rem;
}
```

## Dense Grid Selector

When overriding table row density, `mud-table-dense` is applied to the grid root
element itself.

Use:

```css
::deep .muddy-file-explorer-grid.mud-table-dense .mud-table-row .mud-table-cell {
    padding: 2px 4px !important;
}
```

Do not use:

```css
::deep .muddy-file-explorer-grid .mud-table-dense .mud-table-row .mud-table-cell {
    padding: 2px 4px !important;
}
```

The second selector does not match the rendered MudBlazor DOM.

## Compact Grid Example

```css
::deep .muddy-file-explorer-grid {
    font-size: 0.8125rem;
}

::deep .muddy-file-explorer-grid.mud-table-dense .mud-table-row {
    height: 28px;
}

::deep .muddy-file-explorer-grid.mud-table-dense .mud-table-row .mud-table-cell {
    font-size: 0.8125rem;
    line-height: 1.1;
    padding: 2px 4px !important;
    padding-inline-start: 4px !important;
    padding-inline-end: 4px !important;
}

::deep .muddy-file-explorer-grid .mud-typography-body2 {
    font-size: 0.8125rem;
    line-height: 1.1;
}

::deep .muddy-file-explorer-grid .mud-icon-button.muddy-file-explorer-folder-icon-link {
    height: 20px;
    width: 20px;
    padding: 1px;
}

::deep .muddy-file-explorer-grid .muddy-file-explorer-folder-link {
    font-size: 0.8125rem;
    line-height: 1.1;
}
```

## Notes

MudBlazor table cell padding often requires `!important` because MudBlazor also
uses dense-table selectors with high specificity.
