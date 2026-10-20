---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B4a.md, B4a (CommandEntry, CommandMenu, BattleTargetCursor, shared test BattleScenarios)
Round: 1 (verdict: Fix the Criticals; verify all pass, 1198 tests)

- open (Critical): BattleTargetCursor.StartIndex actor-is-candidate branch (decision 56) and the walk-back past trailing allies have no test; add Start_ActorIsCandidate_SelectsActor (TargetChoice of party+enemies incl. actor).
- open (Critical): CommandMenu Run -> FleeCommand has no test; add Update_ConfirmRun_ProducesFlee (Down twice, Confirm).
- open (Suggestion): MovePrevious wrap (index 0 -> last) untested; one test pins the negative modulo.
- open (Suggestion): Start_SingleEnemy_SelectsLastLivingEnemy runs on three goblins; "SingleEnemy" means single-target choice, rename to Start_SingleTargetChoice_... (slice-given name, developer's call).
- follow-up: none.
