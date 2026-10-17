---
name: tautological-tests
description: Recurring in Core/Data reviews - tests whose assertion can't fail (count==count, "same seed twice gives same result" that passes even with the shuffle removed)
metadata:
  type: feedback
---

Seen in slice 4b (`Select(...).ToList().Count == source.Count`) and 5a (`TurnOrder_Constructor_SameSeed_ProducesSameOrder` and `SeededRandom_..._SameSeed_ReturnsSameSequence` compare two runs with each other, so they pass with no shuffle at all and don't catch a .NET `Random` algorithm change).

**Why:** The plan lists "same seed -> same result" tests, and they read well, but they only prove the code is deterministic, not that it does what it should. The plan's Determinism risk says seeded expectations are a baseline, so pin them.

**How to apply:** For any seeded test, ask "would this pass if the randomness were removed or the algorithm changed?" If yes, Warning: pin golden values (a specific order/sequence for the seed). Related: [[records-with-lists-equality]].

Seen again in 5b (`Execute_SeededBattleLoop_EndsInVictory`): stats make the hero win under any seed, and the one-member party makes `EnemyTactics`' seed irrelevant, so "Seeded" in the name promises something the test doesn't check. Also an unbounded `while (!battle.IsOver)` hangs instead of failing on a turn-advance bug; ask for a turn cap.

Seen again in 6a (guard-clause variant): `ThrowIfIllegalTargets`' `Distinct` check, `CanUse(item)`'s party-side check and `ApplyToTargets`' `wasAlive` guard could each be deleted with every test still passing, and the revive test asserted only `events[1]`. For each deliberate rule the caller lists, ask "which test fails if I delete this clause?"

Seen again in slice 7 (identical-inputs variant): `Start_PartyAndEnemies_CreatesCombatantsOnBothSides` builds three identical goblins, so "in encounter order" can't be checked; and the simulation's only assertion ("every battle ends") would still pass if the party never acted, since win rate is 0 %. When a test claims an order, check the inputs are distinguishable.
