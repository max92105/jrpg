---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11j-2 (Cell events)
Round: 1 (verdict: Ready to commit; build 0 warnings, 832 tests pass, format exit 0)

- open (Suggestion): ObjectEventsWindow.xaml:12 VisibleWhenTrue resource now unused after the step fields moved to EventStepEditor; delete it
- open (Suggestion): SetCellEventEditTests.cs:20 SecondEventOnSameCell_ReplacesFirst lacks the Method_ part; Apply_SecondEventOnSameCell_ReplacesFirstInPlace
- open (Suggestion): right click on an object in event mode silently removes a cell event hidden under it (cell under an object can't be opened by left click); consider ignoring right click when an object is hit
- verified: SetCellEventEdit index stays valid on a linear undo stack (ResizeMapEdit swaps whole layouts); MapDraft ctor fills cell events via ReplaceLayout; EventStepEditor RelativeSource Window binding resolves Choices in both dialogs; ids cell_x_y can't collide with {uid}_{n}
