---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB5 (Editor/Battle/EnemyTacticsViewModel, EnemyTacticsViewModelTests,
EnemyTacticsChangedTests, map.md Battle row, EB5 Done note). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open, optional): the two test files repeat the same catalog load and goblin-with-rules setup; a shared
  fixture would keep the two bosses' purposes side by side (only two copies, so not a Growth finding).
- Carried from EB4: Label "Below  % HP" for an empty threshold: fixed (ShownThreshold shows "?").
- Carried from EB4: Renumber loop follow-up: fixed (ListOrder.Renumber exists and both view models use it).
- Carried from EB4: no test removes a phase's last rule: dropped (by design; Data's check reaches the page in EB6).
- Checked, no finding: Changed raised after NamePhaseFields (owner sees renamed fields); Field set never raises
  RulesChanged; ctor wiring before CollectionChanged hook raises nothing; removed list unwatched (tested); NewThreshold
  floor at 1 tested with "1", "2", "3", "x"; nullable SelectedList recorded in the slice note.
