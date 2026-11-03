---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-11-03-object-composer-and-animated-tiles, slice CO6 (Objects > Edit object; Replace cells leaves). Round 1.
Verdict: Ready to commit.

- open (Suggestion): ObjectComposingTests.cs:121 CanEdit_AfterTheChosenObjectIsRemoved_IsFalse sits among TryApply_*;
  move it next to the other CanEdit_* tests (after line 96)
- open (Follow-up, from CO4a): StartNew's column clip still untested (only StartNew_RectangleTallerThanMaxCells_...)
- fixed (from CO5): composer canvas takes focus on press (CanvasImage.Focus); MainViewModel double blank line gone;
  backlog's CanStartNewObject_* name is real again (MainViewModelTests:227); TileBrowserView lets _cut go with its browser
