---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/I2.md, slice I2 (EnemyTactics rolls crowd control
resistance against Taunt; status grants on fire_blast, recovery, palm_strike, eldritch_bolt)
Round: 1 (verdict: Ready to commit; verify all pass, 2526 tests)

- S1 EnemyTacticsTests.cs:63 "With one, the builder's goblin resists nothing" sits on the NoTaunt test but describes the taunt tests above; move it to ChooseCommand_OneMemberTaunting_AttacksThem; open (suggestion)
- S2 EnemyTacticsTests.cs:126 StubbornGoblinsTurn copies GoblinsTurn; one GoblinsTurn(Combatant goblin) or a resistance parameter; open (suggestion)
- S3 EnemyTactics.cs:29 new ChanceRolls per taunted choice; a `private readonly ChanceRolls _rolls = new(random);` field; open (suggestion)
