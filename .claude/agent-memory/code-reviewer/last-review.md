---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice BT2 (split; tier rows and Power boxes are BT2b): Editor `Balance/`
`EnemySkillBalanceRows`, `ClassBalanceRows`, `EnemyBalanceRows` and their rows; `BalanceTableViewModel` reads the boxes
once through a private `TypedBoxes`. 2026-11-18. Round 1. Verdict: Ready to commit.

- Carried from BT1c, fixed: `LevelProblem`'s parameter is `boxNumber`; the test no longer casts to `INotifyPropertyChanged`.
- Warning, open: `EnemySkillBalanceRows.NamedSkillIds` is the third copy of the tactics-then-phases walk
  (`EnemyDefinition.NamesSkill`, Data `EnemyCatalogValidator.FindPowerProblem`): move it into Core as an
  `EnemyDefinition` member (e.g. `NamedSkillIds` or `Actions`) with a Core test, the three callers reading it.
- Suggestion, open: `BalanceTableViewModel.Typed()` reads vaguely; `ReadBoxes()` (the struct's name blocks `TypedBoxes()`).

Checked: `ContentCatalog` is an immutable record with `IReadOnlyDictionary` members, held in a readonly field, so
`EnemyRows` built once can't diverge from the box-read lists; `content.Skills[id]` indexing matches BT1b's class rows.
