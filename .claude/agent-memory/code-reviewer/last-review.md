---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WS1 (the mode bar: World/Database, decisions 916, 922, 925).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical).

- Warning: MainWindow.xaml Ctrl+H KeyBinding (`ToggleCollisionCommand`) still fires in Database (window KeyBindings bypass the OnPreviewKeyDown return), toggling the hidden map's collision; CanExecute on IsWorldShown — open
- Suggestion: test `ShowDatabase_StrokeBeingDragged_RecordsItBeforeLaterCatalogSteps` could also assert the shop is gone (names what was undone) — open
- Follow-up (EC5): `EncounterEditing_TestWorld_StartsOnItsEncountersInIdOrder` still pins TestWorld's four ids — open
- EC5 suggestion "ImageAddedToTheFolderAfterStart" rename — fixed
