---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WS6 (Edit... shows a page on a record: ShowPageOnRecord, ShowDatabasePageOnRecord,
TryLeaveShown; CatalogDialog, DialogBounds, TakeDropTables, RefreshDropTables gone; decision 928). 2026-11-17.
Round 1. Verdict: Ready to commit.

- Warning: MainViewModelDatabaseWorkspaceTests ShowDatabasePageOnRecord_DropTables_ShowsThePageOnTheTable targets
  "goblin", the only drop table in content/, so the shell dropping the key still passes; add a table, target it — open
- Suggestion: ADR 0060 line 87 still says "a Skills page's strip Edit... takes the same path"; none exists (WS1 note) — open
- WS5 Critical RegisterAnimatedPages name — fixed (RegisterPagesOnTheAnimationClock)
- WS5 Suggestions (ShowDatabasePage from World, _IsLit test name, script message) — fixed
