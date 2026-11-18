---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice BT1 (Core's readings for the balance table; Editor half split to BT1b).
2026-11-17. Round 1. Verdict: Ready to commit. No Critical, no Warning, no Suggestion.

Checked by hand: `MemberBattleStats.Of(PartyMember)` keeps all 15 init fields plus the AttackDamage ctor argument;
AttackType expression unchanged (`member.Weapon?.DamageType ?? member.Rules.UnarmedDamageType`). BT3 note's
"Problem must stay null" holds: `DatabaseWorkspaceViewModel.CanApply` (line 125) and `ApplyOrRefuse` (136), reached
from `TryApplyShown` on every leave, refuse on a non-null `Problem`.

XP4's three Suggestions (ExperienceCurve.Steps comment, map.md ProblemOf, Fill's Problem per row): from the previous
plan; not re-checked here.
