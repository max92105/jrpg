---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 11h-2 (Map settings and resize)
Round: 1 (verdict: Ready to commit; build, 753 tests, format exit 0 all pass)

- open (Warning): MapSettingsWindow.xaml:22-24,36,41 int TextBoxes with UpdateSourceTrigger=PropertyChanged; clearing "20" by backspace leaves Width=2 (last parse), OK resizes to 2 (silently if nothing dropped); use string properties parsed in the VM so SizeProblem covers unparsable text
- open (Suggestion): ResizeMapEditTests.cs:91 Apply_ThenPaintOnANewCell_Works ("Works", paints on a non-existing layer -3); and SetMapSettingsEdit test lives in ResizeMapEditTests (move to SetMapSettingsEditTests, name Revert_RestoresTheSettings)
- open (Suggestion): MainViewModel.SettingsOfCurrentMap is a noun phrase; e.g. StartEditingSettings / EditSettingsOfCurrentMap; add TryApplySettings_Grow_DoesNotAsk test
- open (Suggestion): tick 11h-2 in plan; note placeholders (undoable settings, odds start 1/1), confirm callback generalized, objects already fully off-map dropped by any resize
- verified: refit logic (OnOpenMapChanged compares _gridDrawnFor size; ApplyViewport redraws grid on size change), undo/redo linear history consistent with ReplaceLayout snapshots, Catalog! safe, placing selection prunes dropped uids
