---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-05-free-levels-and-picker-width, slice FL3 (last slice), round 1 (2026-11-05). Verdict: ready to commit.

- Fixed: the plural "Choices" test name is now PaintAt_OnAnEmptyLevel_RaisesItsChoiceText.
- Fixed (from FL2): the Level_SetOnAnEmptyLevelAboveTheBase_IsChosen auto-property round-trip test is gone from the tree.
- Checked: GridSplitter themed by EditorTheme's implicit style (no local Style); no stale "Name" display path or "420" wording elsewhere.
