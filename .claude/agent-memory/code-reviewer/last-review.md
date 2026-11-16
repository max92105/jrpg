---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-enemy-behavior, slice EB1 (TacticTarget, TacticTargetCandidates, EnemyAction.On,
EnemyTargeting.ForRule + TauntingOrAll + HighestShareOf, EnemyTactics HasTarget/CommandAimedBy, AbsentWhenNullFilter
"on"). 2026-11-16. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): TauntingOrAll takes taunters from all of `legal`, not legal foes as its comment and decision 838
  say; filtering `Side != enemy.Side` there would make ForRule's `candidates[0].Side` guess unnecessary. Only reachable
  with an otherCombatant aim, which EB2's TacticTargetFit refuses.
- Suggestion (open): LowestShareOf/HighestShareOf multiply int HP by int MaxHp; overflow past MaxHp 46 340 (no cap in
  Data). HeavyHitRule already widens to long.
- Checked, no finding: eligibility draws nothing (HasTarget -> TacticTargetCandidates.For -> CombatantsMeeting, pure);
  ByTauntRule(candidates, candidates) keeps its draws; "among foes by side" correct for every fitting aim (conditions
  today name only the enemy's side; foe targets fit singleEnemy/attack only); ties keep the first (strict < and >);
  Combatant is a class so Intersect is by reference; replay test untouched.
- AN7 items dropped (committed slice).
