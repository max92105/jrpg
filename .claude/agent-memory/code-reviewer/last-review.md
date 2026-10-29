---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-forge, slice F2b (salvage behind a ConfirmPrompt; ForgeScene Prompt/Notice; ForgeView and
ForgeDetailView Place gain Notice/Prompt; ForgeSceneSalvageTests). Round 1.
Verdict: Ready to commit. verify.ps1 all pass (2901 tests).

- Warning: ForgeScene.Salvage keeps the cursor on the same row (or the last) via Show(Cursor.Selected) but no test pins it; Show(0) passes every test - open
- Suggestion: Notice survives opening and cancelling the actions on the next piece, showing the previous salvage again; clear it when the actions open - open
