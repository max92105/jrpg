---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/PL1.md, slice PL1 (paladin conviction: heavy hits, enemy falls, Smite; cleric faith on heavy hit)
Round: 1 (verdict: Ready to commit; verify all pass, 1881 tests)

- Warning, open: no ResourceTriggers test of a light hit on a resource with PerHeavyHit set (e.g. paladin hpLost 19 of 100 -> no event); dropping the IsHeavy check would pass every test.
- Suggestion, open: OnHitTaken_HeavyHitOnCleric_LosesFaith asserts only the amount; could pin the single ResourceChanged(-1) event like its sibling.
- Follow-up: a felling heavy hit grants conviction to the paladin it kills (OnHitTaken runs before Fall); harmless while resources reset per battle.
