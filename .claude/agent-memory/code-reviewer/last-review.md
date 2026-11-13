---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-13-drop-table-editor, slice DT1 (decisions 599, 600, 601, 604: DropTable equal by content,
AbsentWhenOneFilter, drop-tables.json written and relaid, CatalogProblems.OfDropTables, CatalogDraft.SetDropTables).
2026-11-13. Round 1. Verdict: ready to commit. No Critical/Warning.

- Suggestion (open): docs/plans/backlog.md:210 ContentWriter size line says "at 294 after DT1" and still "DT1 takes it
  to about 293 (approved 2026-11-13)" -> drop the stale clause.
- Follow-up: OptionalFieldFilter summary still says "the filters for fields added after the content was written" (stale
  since decision 586; now six filters).
- DT0's three suggestions: all fixed (theme comment "A catalog window's", EqualityComparer<T>.Default.Equals, DT0.md
  "then empty, removed too"). Don't carry.
