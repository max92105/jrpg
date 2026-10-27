---
name: default-masks-copy
description: A stat copied from the sheet into CombatStats goes untested when the test rules yield the same value as the record default
metadata:
  type: feedback
---

When a slice copies a sheet stat into `CombatStats` (MemberBattleStats/Combatant.FromEnemy), check the test rules
(TestStatRules.Neutral, builders) don't produce exactly the `CombatStats` default; if they do, deleting the copy line
passes every test. Seen H1 (accuracy 1000) and H2 (crit damage 1500).

**Why:** the player-facing stat (gear, Power) silently stops mattering in battle with no failing test.
**How to apply:** for each new copied field, grep the default and the neutral rules; flag as a Warning and propose a
battle test with a non-default rule value (e.g. CritDamagePercentPer100Power > 0). Related: [[tautological-tests]].
