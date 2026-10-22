---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/P2.md, slice P2 (party sidebar look: SidebarCard snapshot, PartySidebar.Leader, PartySidebarView, MenuFrame.SidebarView, UiTheme.SidebarPanel)
Round: 1 (verdict: Ready to commit)

- suggestion (open): PartySidebarView keeps two parallel lists (_formattedFrom, _cards) indexed together; one list of (SidebarCard Source, MemberCardView.Card Card) pairs removes the keep-in-sync risk.
- suggestion (open): SidebarCard doc says "A new record means the card needs drawing anew"; the view draws every frame, it means formatting anew.
- follow-up: P1 warning still open: Equipment sheet width jumps when the choice opens (484 vs 560).
- follow-up: P1 suggestion (sidebar ignores PageTransition opacity) fixed by this slice.
- follow-up: clamped-share/ease-out copies to Ui/Easing; MainViewModel 593 lines; TileImages.Forget cache; E12a stale SelectedCells (carried).
