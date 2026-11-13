---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-13-drop-table-editor, slice DT3 (decisions 597, 598: Editor/Loot DropItemChoice, DropEntryRow.New,
DropTableViewModel entry commands and EntriesChanged; tests DropTableEntryEditingTests). 2026-11-13. Round 1.
Verdict: ready to commit. No Critical/Warning.

- Suggestion (open): src/Jrpg.Editor/Loot/DropTableViewModel.cs:115 comment narrates what the handler does; say why
  (Index is Data's entries[i]) or drop it.
- Suggestion (open): no test that a move or a remove raises EntriesChanged (DT4's problem line names entries by
  position); add EntriesChanged_EntryMoved_IsRaised.
- Suggestion (open): DropTableEntryEditingTests RemoveEntry_NothingChosen_IsGreyed asserts Remove and MoveUp, not
  MoveDown; add the third CanExecute assert.
- DT2's NameOf -> ItemNameOf suggestion: fixed. Don't carry.
