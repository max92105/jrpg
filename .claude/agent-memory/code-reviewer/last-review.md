---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/W6.md, slice W6 (docs indexes, rules, agent memory; rename one done plan; close the 2026-10-18 plan)
Round: 1 (verdict: Ready to commit)

- warning (open): architect/project_content_quirks.md:10,15 keeps the stale "portraitFile goblin.png vs Goblin.png" fact (fixed in 8111383; enemies.json says Goblin.png) and its Why says lookups "must not depend on case", the opposite of ArtReferenceCheck's exact-case rule; rewrite as "battle art is matched by exact case (ArtReferenceCheck)" and update the MEMORY.md hook.
- suggestion (open): architect.md:39 and plans/README.md:9 keep a "port mapping" design.md section, but the bullet defining what goes in it was removed; say what it maps or drop it.
- follow-up: done/2026-10-17-core-migration.md links ../decisions/000[2-4] (resolve to docs/plans/decisions/), broken before this slice.
