---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-forge, slice F2c (upgrade and reroll at the Forge; ForgeScene CarryOut/Craft; forge_upgraded,
forge_rerolled; ForgeSceneCraftTests). Round 1.
Verdict: Ready to commit. verify.ps1 all pass (2908 tests).

- Suggestion: CarryOut's `default: throw` guards a ForgeAction value that can't reach it (three values, all handled); drop it - fixed
- Suggestion: Craft re-derives Core's "first equal piece" rule with Enumerable.Range(...).First; a Core result naming the replaced index would keep the rule in one place (test 2 pins it meanwhile) - open
- Follow-up: F2b's Salvage cursor-stays-on-row: tested in ForgeSceneSalvageTests (middle row stays, last row moves) - fixed
- F2b Suggestion (notice survives opening next piece's actions): fixed (UpdateList Confirm clears Notice)
