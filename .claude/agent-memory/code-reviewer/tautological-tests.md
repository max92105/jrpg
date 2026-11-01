---
name: tautological-tests
description: Recurring in every review - tests whose assertion can't fail; the variants seen, one line each, and the question that exposes them
metadata:
  type: feedback
---

**Why:** a test that reads well but passes with the rule deleted protects nothing; the plans list such tests ("same seed, same result") because they sound right.

**How to apply:** for each new rule, guard, branch or chosen value in the diff, ask "which test fails if I delete this line?" If none, it's a Warning (Critical when several branches ship untested). The variants seen so far:

- **same-seed-twice** (5a, 5b): two runs compared with each other pass with the randomness removed; pin golden values.
- **count==count / oracle** (4b, 11g, stats E1): the test rebuilds the production computation; pin a golden prefix instead.
- **guard clause** (6a, 8, 1b, class-mechanics PL2b, RG1): a `||` clause, a validator message or a `throw` with no test; match each to a test (one theory row per clause).
- **boundary** (1b re-review, gold I1): `<=` tested only at the bound, or a range tested only on rejected values (0 and max + 1); test the accepted bounds too.
- **fixture doesn't reach the scenario** (part 2 4a, 5b, slice 7): coordinates, keys or identical inputs make the named case never happen; trace the fixture.
- **single recipient / neutral rules / fixture default** (battle C1, stats M1a, M2a): a total equals a share, a multiplier runs at 1, a stat defaults to 0; use two recipients and a non-neutral value.
- **stale equals expected** (demo G1a, gold L2b): the "refreshed" value was already the expected one; start from a state where it differs, one test per cache key.
- **same observable** (gold I2b, B3): two outcomes look alike through the assertion used (`IsShowingMessage`, "a consumable row"); assert the id or value only the right outcome has.
- **loop exit restated** (battle B1b): asserting the loop's own exit condition; ask what else differs.
- **cache by reference** (demo I1): `Assert.Same` on a value the runtime interns anyway.
- **constant oracle** (stats P3b, quests E3): asserting `Cap * 10`, or a list equal to the static it was set from, keeps green when the values change; pin the literals.
- **second overload** (gold S2/S3): the stack overload's false paths tested, the piece overload's not; check each overload.

Related: [[records-with-lists-equality]].
