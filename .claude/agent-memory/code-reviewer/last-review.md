---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-loot, slice B1 (tests/Jrpg.Simulations/Loot/LootSimulation.cs: goblin trio drops over 2,000 seeds,
rarity-set equality test, potion-rate test; Done note).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (2717 tests).

- Warning: LootSimulation.DropRollFor:59-65 copies GameSession.DropRollFor (private static); Simulations has
  InternalsVisibleTo, so make the session's builder internal (or a static on DropRoll) and call it - open
- Suggestion: PotionRateTolerance 0.03 is absolute potions per goblin; with a quantity range it shrinks to ~1-2 sd and
  the comment copies today's 6,000 goblins / 30 % - open
- Suggestion: pool rebuilt and re-sorted per seed (2,000 times); hoist to a static field - open
