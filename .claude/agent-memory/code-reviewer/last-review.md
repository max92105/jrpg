---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice CO3 (ObjectComposerViewModel, ObjectNaming). Round 1.
Verdict: Ready to commit.

- open (Warning): OnPartChanged's Canvas/SizeProblem/CategoryProblem arms untested; only the IdProblem arm is pinned,
  so dropping nameof(CanvasPainting.Canvas) leaves "Paint at least one cell." up after the first stroke
- open (Suggestion): Problem_EditWithItsOwnId_IsNull doesn't test the IsIdEditable gate (ForEdit hands naming no
  taken ids); drop the gate or say it guards an older id that fails ObjectIds' pattern
- open (Suggestion): Title has no test ("New object" / "Edit object 'chest'")
- fixed (carried from CO2): EndStroke same-picture check, RowsText tests, TypedCounts summary
- open (Follow-up, CO5): a Shift+drag rectangle over 32 columns or rows makes ObjectCanvas.Of throw when the
  composer opens from it; CO5's slice still doesn't say how it's clamped or refused
