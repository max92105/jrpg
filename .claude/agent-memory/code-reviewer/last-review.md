---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-13-drop-table-editor, slice DT2 (decisions 597, 598: Editor/Loot DropEntryRow, DropTableViewModel,
tests with their own DropTableFixtures). 2026-11-13. Round 1. Verdict: ready to commit. No Critical/Warning.

- Suggestion (open): src/Jrpg.Editor/Loot/DropTableViewModel.cs:66 private `NameOf` -> `ItemNameOf` (reads like the
  `nameof` keyword, doesn't say whose name).
- DT1's carried items: backlog.md:210 stale clause fixed; OptionalFieldFilter stale summary fixed. Don't carry.
