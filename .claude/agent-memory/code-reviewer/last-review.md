---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/P1.md, slice P1 (party sidebar: PartySidebar, SidebarCard, MenuFrame, IMenuPage.ShowsPartySidebar, GameMenuScene)
Round: 1 (verdict: Ready to commit)

- warning (open): Equipment sheet width jumps when the choice opens: narrower by 484 (sidebar) vs 560 (EquipmentChoiceView.PanelWidth), so the sheet's two columns shrink/shift ~38 px; align widths or keep the sheet width fixed.
- suggestion (open): the sidebar ignores PageTransition offset/opacity, so it pops in at full opacity while the page slides and fades.
- follow-up: the 6 clamped-share copies and 2 cubic ease-out copies (backlog) can move to Ui/Easing (carried from T1).
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
