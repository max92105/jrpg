---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WD4 (Loot/DropTableCatalogPage in CatalogDialog; DropTableCatalogViewModel implements
IEditedCatalog, its CanApply removed; HasDropTableEditing removed; EditDropTables(owner, id) keeps decision 602).
2026-11-16. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Suggestion, open: tests/Jrpg.Editor.Tests/Loot/DropTableCatalogViewModelChoiceTests.cs:16 rebuilds the base file's
  WithTables + _wolfDrops/_spare inline; move WithTables to DropTableFixtures and use it from both.
- Fixed (from WD3): docs/map.md Shell row no longer lists `.QuestEditing`.
- Follow-up, open (carried from WD2, still true by grep): .claude/rules/editor.md:25 says a dialog's OK greys "through
  CanApply"; CatalogDialog greys on IEditedCatalog.Problem.
- Checked: no DropTableCatalogWindow/HasDropTableEditing/ChooseTable/"Drop tables window" left in src, tests, map or
  rules; page binds AncestorType=loot:DropTableCatalogPage; Choose/ChosenKey mirror Shop/Quest view models; Problem
  notification still asserted after the CanApply assert went; the menu entry has no IsEnabled, as recipe step says.
