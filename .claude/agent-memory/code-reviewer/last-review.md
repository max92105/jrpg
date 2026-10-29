---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-forge, slice F1c (SheetRowLook moved to Ui with its constants; ForgeRow with affix lines;
MaterialStock/MaterialCount; ForgeView detail column; forge_materials, forge_no_affixes).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (2886 tests).

- Suggestion: ForgeView looks the rarity name up in Draw (keeps _messages only for it); put it in ForgeRow - open
- Suggestion: Show_RolledPiece_RowHasItsAffixLines checks only "+4 "/"+2 " prefixes; pin AffixText.Of's whole line - open
