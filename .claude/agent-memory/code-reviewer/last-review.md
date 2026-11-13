---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-shops-and-enemy-database, slice EN1 (enemies.json written: AbsentWhenFalse/EmptyFilter, null/zero
filter rows, ResistancesConverter.Write, CatalogProblems.OfEnemies/CheckEnemies, ContentWriter enemies,
CatalogDraft.SetEnemies). 2026-11-13. Round 1. Verdict: ready to commit. enemies.json relay checked: same values per
enemy (PowerShell ConvertFrom-Json), only order/layout differs. test-world enemies.json = live content (TestWorld.props).

- NP2 Suggestion (lookup read per dialog) not carried (committed slice).
- Warning (open): CatalogProblems.cs:85-96 OfEnemies is the third copy of the try/CheckX/catch-message block; extract
  `private static string? FirstProblem(Action check)`.
- Warning (open): AbsentWhenNullFilter.cs:9-15 / AbsentWhenZeroFilter.cs summary + `_addedLater` say "added after the
  content was written", the new enemy rows are decision 586 defaults; summary should name both reasons (map.md Content
  row same: "an optional field added later").
- Suggestion (open): no test that a changed phase tactic makes SetEnemies record a step (Phases.Zip branch).
- Suggestion (open): ResistancesConverter.Write floors a 1..9 permille to a written 0 (unreachable from content today).
