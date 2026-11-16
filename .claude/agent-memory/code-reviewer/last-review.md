---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skills-and-statuses-model, slice T1 (TieredEffect, Effect.Tier, CombatStats.Power,
DamageFormula.TieredAmount/MaxPower, EffectScaling tier branch). 2026-11-15. Round 1. Verdict: Ready to commit.

- Suggestion (open): CombatStats.Power doc says "an enemy's content power", but Combatant.FromEnemy sets none until T3;
  the slice's "(T3)" was dropped.
- Checked: TieredAmount's long product cannot overflow for int power x perPower <= 1000 (T2 bound); MaxPower x
  Calculate's 1.8e12 per power, and x1.5 dominance after, still fit a long.
- SW3's suggestions dropped (committed slice); its Frozen(Brush) follow-up is in backlog.md (line ~277).
