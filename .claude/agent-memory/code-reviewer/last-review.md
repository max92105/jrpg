---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice SI4 (FieldSpriteWindow, WalkPreview, SpriteSheetMarks, StatusBand,
MainWindow/MainViewModel.FieldSprites). 2026-11-12. Round 1. Verdict: fix the Criticals.

- Critical (open): Sprites/WalkPreview.cs:95 static mutable `_lastCells` (no static mutable state); make it an instance
  owned by the window, or keep the last cells in the window.
- Critical (open): Shell/MainViewModel.FieldSprites.cs:37 `StartFieldSpriteEditing` builds the view model, doesn't start
  a session (clashes with FieldSpriteEditingViewModel.Start); rename CreateFieldSpriteEditing or inline.
- Warning (open): FieldSpriteWindow.xaml:82-90 SelectedIndex two-way over a RowChoices list rebuilt on Rows/DataContext
  change: a row past the new count is cleared and -1 written back (typing Rows "12" via "1"; 7-row -> 4-row image).
- Warning (open): FieldSpriteWindow.xaml.cs:262-268 + WalkPreview.cs:99: each render tick runs Problem 5 times (catalog
  check + list) and LINQ; cache the FieldSpriteSheet on setup change, tick compares WalkCellAt only.
- Suggestion (open): setup problem shown twice (xaml:104 and :41); OnOkClicked guard redundant with greyed OK.
- Follow-up: SI2 FieldSpriteSetupViewModel.From integer-divides spare pixels (still open, not in backlog).
- SI3 items: CopySetupFrom split check fixed; RefreshAfterImport stale size fixed; ToSheets merely-chosen fixed.
