---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11i-2 (Give item, swap object and warp steps)
Round: 1 (verdict: Fix the Criticals; build 0 warnings, 805 tests pass, format exit 2)

- open (Critical): format ENDOFLINE at tests/Jrpg.Editor.Tests/Shell/MainViewModelTests.cs:250 (the added EventEditing.MapIds assert is an LF line); run dotnet format
- open (Warning): number boundaries half-pinned: Problem_ZeroQuantity_SaysSo only NotNull, quantity "1" never accepted in a test, CellYText-only bad value untested (Problem checking only X would let OK through, then ToStep throws in Apply and crashes the editor)
- open (Suggestion): ProblemText in ObjectEventsWindow stays stale after the problem is fixed until OK is pressed again
- open (Suggestion): Label/ToStep use `_ =>` as the Warp arm; name EventStepKind.Warp and make `_` throw UnreachableException
- open (Suggestion): hand-check shared ComboBoxes (SelectedItem TwoWay) when switching selected step between kinds don't write null/"" back into the other step's id
- verified: Apply only reached after OK, which refuses while Problem != null; RelativeSource DataContext.Choices.* resolves to EventListViewModel.Choices; OnMapIdsChanged + ctor init keep EventEditing.MapIds in sync (tested); validator rejects "" item/object/map ids on save
