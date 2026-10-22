---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E13b.md, E13b (dark theme, Shell/EditorTheme.xaml; uncommitted; E13c split off for ComboBox/ScrollBar/overflow templates)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1537 tests)

- critical (open): EditorTheme.xaml:75 ToolBar.ComboBoxStyleKey override has no Template/BasedOn, so tool bar combos (layer, tool, brush size) lose Aero2's flat tool bar template (probed: different template, Background=WindowBrush, Foreground=WindowTextBrush) and fall back to the light normal ComboBox with #DDDDDD text; delete the override.
- warning (open): Aero2 submenu popup SubMenuBorder hardcodes #FFF0F0F0 (probed); open menus show a light frame/separators, not in E13c's scope; add menus to E13c or accept by hand check.
- suggestion (open): interim ComboBox Foreground uses WindowBackgroundBrush as text color (role name misused); a DarkTextBrush or a comment, goes with E13c anyway.
- suggestion (open): EditorButton template drops the IsDefaulted look; OK/Create buttons no longer stand out.
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
