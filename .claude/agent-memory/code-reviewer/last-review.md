---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/G1a.md, slice G1a (interaction prompt: Core InteractionInFront + Game InteractionPromptView/PromptPlacement)
Round: 1 (verdict: Ready to commit; build pass; 1558 tests pass; format pass)

- warning (open): RefreshInteraction calls in ContinueEvent and FinishEncounter are unpinned; AfterWarp_IsRefreshedOnTheNewMap expects null, which was already the stale value from TryInteract, and InteractEventFinishingAtOnce_IsRefreshed expects the same record it had before. Fix: an arrival facing an interact object (assert its uid), a repeatable talk event that ends (assert non-null), a FinishEncounter test facing an object.
- suggestion (open): test name WalkingTowardSameObject differs from the slice's WalkingAlongSameObject (fine, tests the same rule).
- follow-up: prompt box covers the player sprite when standing right above the object (slice note, G1b).
- follow-up: MainViewModel is 593 lines (carried).
- follow-up: TileImages.Forget doesn't drop cached sprite frames (carried).
- follow-up: E12a re-cut stale SelectedCells can reach CreateObject (carried).
- follow-up: I1 InputReader stick level-vs-edge device switch warning (carried from I1).
