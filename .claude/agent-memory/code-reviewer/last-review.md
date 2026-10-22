---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/N3.md, N3 (New NPC dialog; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1537 tests)

- warning (open): NewNpcWindow preview calls TileImages.FieldSpriteFrameOf on any PNG listed; a field-sprite PNG under 48 px (or unreadable) throws on selection and crashes the editor.
- suggestion (open): "New NPC..." menu item has no IsEnabled binding (New object has CanStartNewObject); does nothing silently without content.
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: MainViewModel is 593 lines; split before more is added (carried, in backlog).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
