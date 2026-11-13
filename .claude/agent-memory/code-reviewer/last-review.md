---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-13-drop-table-editor, slice DT4 (decisions 596, 598, 603, 607: Editor/Loot DropTableCatalogViewModel,
DropTableEditingViewModel; tests DropTableCatalogViewModelTests, DropTableEditingViewModelTests). 2026-11-13. Round 1.
Verdict: ready to commit. No Critical/Warning.

- Suggestion (open): DropTableCatalogViewModel.cs:116 comment "its own label and problem notices come with them" reads
  as if they were relayed; say they arrive and are dropped.
- Suggestion (open): DropTableCatalogViewModelTests.cs:41 OfferedItems test takes the first two of the whole list, so a
  live item named before "Aaa Charm" (test world = live content) breaks it; filter to the two charms.
- Follow-up (open): catalog list skeleton (Watch, unhook on Remove, RaiseProblemChanged, ToX throwing) now in
  Shop, Enemy and DropTable catalog view models; not in backlog.md.
- DT3's three suggestions (DropTableViewModel:115 comment, EntriesChanged_EntryMoved_IsRaised, MoveDown assert): fixed.
