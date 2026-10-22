---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/N2.md, N2 (NPCs in the editor: drawn, Talk preset; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1531 tests)

- suggestion (open): PlacedObjectHits NPC hit test has no miss case just outside the cell (X = Position.X + Width).
- suggestion (open): zones radio tooltip "NPCs have no above-player zones" also shows on tile objects; bind the reason only when disabled.
- follow-up: TileImages.FieldSpriteFrameOf crops 48x48 unchecked; a sprite smaller than 48 px throws when drawn (no validator check).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (matters once sprites can be imported).
- follow-up: MainViewModel is 573 lines; split before more is added (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
