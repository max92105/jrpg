---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-forge, slice F1b (ForgeList of owned pieces, ForgeScene takes text/ui/session, ForgeView rows under
DrawGlidingSelection, forge_empty text; ForgeListTests x3, ForgeSceneTests.Update_Down_MovesTheList).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (2883 tests).

- Suggestion: NewSession (removes the new game's loose pieces) copied in ForgeListTests and ForgeSceneTests; share it
  before F1c adds a third copy - open
- Suggestion: Show_OwnedGear's IndexOf ordering assert repeats what Assert.Equal(Gear, Pieces) already proves - open
