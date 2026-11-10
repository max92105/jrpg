---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice UO1 (Ctrl + click again picks the next object under,
decisions 523, 524), 2026-11-10. Round 1. Verdict: ready to commit.

- Open (Warning): ObjectPicking.cs:423-431 the note and Next under survive a move of the picked object (Shift + drag,
  arrows, panel Apply, undo): "bench_1 selected: 2 of 2 objects here" stays after the bench left the spot; reset on
  the open map's change too, plus one test.
- Open (Warning): content/map-layouts/start.json (two new trees, tree_2_3 moved/relayered) is hand-check residue
  outside the slice; keep it out of the commit or note it.
- Open (Suggestion): ObjectPlacingViewModel.cs:161-163 doc says "next one under it when the top one is already the
  one selected"; it is the next under whichever selected object is in the stack.
- Open (Suggestion): `is [var top, ..] ? top : null` x3 (TopAt, ObjectAt, GrabbedObjectAt) where FirstOrDefault()
  says it; PickAt's `hits.ToList().IndexOf(picked)` re-finds the index NextUnder already had.
- DO2 items: not carried (new slice; DO2 committed).
