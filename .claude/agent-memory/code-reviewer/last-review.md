---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E13c.md, E13c (dark templates in new Shell/EditorTemplates.xaml; XAML only; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1537 tests)

- suggestion (open): EditorTemplates.xaml:8/165-168 ComboBoxToggle reused as tool bar overflow at Width=14; arrow (8px + 6px right margin) doesn't fit 12px inside the border, gets clipped/off-center; rename DropDownToggle, center the arrow or widen.
- suggestion (open): EditorTemplates.xaml:211 MenuHeader popup has no PlacementTarget, so it places against the padded Grid: drop-down opens 8px right and 3px below the header highlight; target the Frame border.
- suggestion (open): EditorTemplates.xaml:95 ScrollThumb has no IsDragging trigger; thumb reverts color mid-drag when the mouse leaves it.
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
- follow-up: tool bar Separator/CheckBox/TextBox keep Aero2 tool bar styles (ToolBar.SeparatorStyleKey not themed); check in E13c hand check.
- E13b round-1 items resolved by E13c: ToolBar.ComboBoxStyleKey now BasedOn a full template; open menu frame/separators dark; OnLightTextBrush removed.
