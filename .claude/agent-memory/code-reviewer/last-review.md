---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-11-code-health-cleanup, slice R2 (Editor Maps: the lines that do two things), 2026-11-12. Round 1.
Verdict: ready to commit.

- Warning (open): `MapTree.Shown` record and `CollisionShapes.InMapCells` signature are only-long lines wrapped
  against decision 539; Done note calls Shown "as planned" and omits the InMapCells signature. Record or revert.
- Suggestion (open): `InMapCell`'s `//` comment repeats the `InMapCells` summary just above; drop it.
- Follow-up: `MapDrawing.DrawCanopy` repeats `InMapCell`'s arithmetic (times CellPixels); `DrawObjectAt`'s sprite
  branch has no test (MapDrawingTests never sets FieldSpriteFile).
- Verified equal: ClosedOutline (outline[0], Skip(1) unstroked, closed), Rect(Point,Size) vs Rect(x,y,w,h),
  InMapCell arithmetic, IsOpen, withOutline/mapFolders lazy and enumerated once. CollisionShapes:34 is 130 columns.
- R1's two suggestions: not carried (R1 committed).
