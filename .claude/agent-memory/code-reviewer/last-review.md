---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-sprite-cells-and-mirrors, slice SC1 (FacingSequence, FacingSequences, FacingCycle.Sequences,
FieldSpriteSheet reading sequences, TileAnimation.LoopFrameAt, AbsentWhenNullFilter `sequences` moved from SC2).
2026-11-15. Round 1. Verdict: Ready to commit.

- Suggestion (open): SC1.md Done note's second line runs past 120 columns; rewrap.
- Follow-up (open): ArtReferenceCheck.cs:121 keeps a `(long)` cast on RowsNeeded, already long before SC1.
- Checked: FacingSequence's custom Equals/GetHashCode carries content equality up through FacingCycle and the sheet
  (tests pin both); ColumnsNeeded/RowsNeeded callers are Data checks and import only, never Update/Draw; empty
  sequence would divide by zero / index out of range, but Data refuses it from SC2 (slice note).
