---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-shell-cutover-and-map-editing.md, 3b-1 (Objects in depth order; 3b split into 3b-1/3b-2 in the same diff)
Round: 1 (verdict: Ready to commit; caller said this is the single review round)

Accepted (don't re-raise): object bodies drawn tile by tile, only canopies baked (3b-2); MapDrawList rebuilt once at scene construction until walking; player order = object count; placed objects use catalog definitions, swapped objects (ActiveMap.CurrentDefinitionOf) in part 3b; split into 3b-1/3b-2 judged sound.

- open (Warning): MapDrawList player-vs-object tie (feet == object base) untested; the "player order = object count" rule has no test pinning it
- open (Suggestion): MapView.DrawObject draws every object tile each frame, no on-screen cull (legacy and DrawLayer cull)
- open (Suggestion): tick 3b-1 checkbox in the plan when committing (DoD 5)
- open (Follow-up): swapped-object definitions (ActiveMap.CurrentDefinitionOf) not recorded in the plan's 3b-1 note or part 3b notes
