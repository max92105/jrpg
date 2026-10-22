---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/G1b.md, slice G1b (prompt look: PromptFade, pill + key cap via PanelView, opacity)
Round: 1 (verdict: Ready to commit; build pass; 1566 tests pass; format pass)

- suggestion (open): PanelView.Draw has three empty `<param>` tags (batch/textures/area), added only so opacity can be documented; fill them or move the opacity note into the summary.
- suggestion (open): UiTheme.KeyCap repeats TextPrimary's F2F2F5 as a literal while its doc says "TextPrimary as a fill"; static init order (TextPrimary declared later) is the likely reason; say so or move KeyCap below TextPrimary and reference it.
- follow-up: G1a warning on unpinned RefreshInteraction calls (ContinueEvent/FinishEncounter) (carried).
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
- follow-up: I1 InputReader stick level-vs-edge device switch warning (carried).
