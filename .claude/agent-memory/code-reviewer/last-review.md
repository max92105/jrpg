---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E12c.md, E12c (plain drag selects sheet cells, live outline, New object button under the sheet; uncommitted)
Round: 1 (verdict: Ready to commit; build pass; 3 test failures all from the developer's own uncommitted forest.json/tiles.json, excluded from the slice and recorded in its note)

- suggestion (open): TileBrowserViewModel.cs:95 ChoiceText says "make an object from them below", so the VM's text depends on MainWindow placing the button under TileBrowserView; say "with New object from selection" instead, or note the coupling.
- suggestion (open): TileBrowserView.xaml.cs:21 _isDraggingCells is only reset on press; also clear it in OnSheetLostMouseCapture so the two press fields reset together.
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
- follow-up: Load_RepositoryContent_LoadsEveryTile and two LayerEntries tests pin repository content; update them when forest.json/tiles.json edits are committed.
- E13c suggestions (DropDownToggle arrow, MenuHeader PlacementTarget, ScrollThumb IsDragging) were left open in that slice, not re-raised here.
