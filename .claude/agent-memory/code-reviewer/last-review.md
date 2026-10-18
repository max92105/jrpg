---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 2 (walking and collision)
Round: 1 (verdict: Ready to commit)

- open (Warning): KeepOnMap clamps to Width - radius, but floor(center + radius) = Width is off-grid, so the clamped right/bottom position always fails CanStandAt; player stops up to one step short of right/bottom edges (left/top reach exactly radius); only the left clamp is tested
- open (Warning): no cap on step length per Walk call; a delta >= ~0.27 s (step > 1 - 2*radius) jumps a one-cell wall or thin polygon (tests use 0.5 s steps)
- open (Suggestion): pure-axis blocked move retries the identical step (step with Y=0 == step); guard slide with step.X != 0 && step.Y != 0
- open (Suggestion): non-finite walkSpeed/playerRadius branch untested
- follow-up: WalkabilityGrid.IsCellWalkable uses LINQ (build time only, fine), none other
