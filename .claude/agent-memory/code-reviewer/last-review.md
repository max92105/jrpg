---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E5b2.md, E5b2 (the Cut panel: columns and rows; uncommitted)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1409 tests)

- open (Critical): TileBrowserViewModel.OnSelectedChoiceChanged closes the Cut, no test; add SelectedChoice_OtherImageWhileCutOpen_ClosesCut.
- open (Warning): SheetCutViewModel margin is symmetric (2*Margin) but TilesheetMarks/ColumnsIn need only one margin; 96 px, 8 cols, margin 12 -> 9 px tiles, preview and sheet show 9 columns, panel says 8. Add a problem when margin >= tile+spacing, or test-pin.
- open (Warning): OnCutFieldChanged -> ApplyCutCommand.NotifyCanExecuteChanged untested; broken wiring leaves Apply enabled and ToSheet throws (crash). Test CanExecuteChanged raised on Columns change.
- open (Suggestion): unchecked int overflow in SpanAlong (Margin 1,500,000,000, Columns 2 -> Problem null); cap inputs or use checked/long.
- open (Suggestion): rows problem, zero columns and negative margin messages untested.
- open (Suggestion): Set up while the Cut is open rebuilds the guess, losing typed fields; hide Set up while IsCutOpen.
