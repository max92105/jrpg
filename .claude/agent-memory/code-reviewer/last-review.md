---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, P2b-2 (Item target portraits: ItemTargetPickerView cards, ItemTargetPicker Left/Right, ItemUseDialog passes icon)
Round: 1 (verdict: Ready to commit; build 0 warnings, 1031 tests pass, format exit 0)

- open (Warning): MessageIds.ItemTargetVitals + content/text/ui.json "item_target_vitals" now unused (only reader was the old row view) -> delete both
- open (Warning): rotated-pixel diamond draw now copied a 3rd time (DialogueBox.DrawDiamond, StatusPage frame, ItemTargetPickerView glow) -> one shared Ui draw (e.g. DrawDiamond(batch, pixel, center, diagonal, color))
- open (Suggestion): Left (moves like Up) has no test; only Right is pinned
- open (Suggestion): record the three deviations (96 px diamond, one row up to ~13 members, no pulse) in the P2b-2 slice note when ticking
