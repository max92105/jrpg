---
name: tautological-tests
description: Recurring in Core/Data reviews - tests whose assertion can't fail (count==count, "same seed twice gives same result" that passes even with the shuffle removed)
metadata:
  type: feedback
---

Seen in slice 4b (`Select(...).ToList().Count == source.Count`) and 5a (`TurnOrder_Constructor_SameSeed_ProducesSameOrder` and `SeededRandom_..._SameSeed_ReturnsSameSequence` compare two runs with each other, so they pass with no shuffle at all and don't catch a .NET `Random` algorithm change).

**Why:** The plan lists "same seed -> same result" tests, and they read well, but they only prove the code is deterministic, not that it does what it should. The plan's Determinism risk says seeded expectations are a baseline, so pin them.

**How to apply:** For any seeded test, ask "would this pass if the randomness were removed or the algorithm changed?" If yes, Warning: pin golden values (a specific order/sequence for the seed). Related: [[records-with-lists-equality]].
