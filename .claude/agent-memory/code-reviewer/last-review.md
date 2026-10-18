---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 4c (Pending encounters and the encounter-rate simulation)
Round: 1 (verdict: Ready to commit)

Accepted (don't re-raise): step = new cell; cell-event cells don't roll; separate turn SeededRandom; call cap 4x step limit; empty FixedSequenceRandom on battle-free test maps; event battles call FinishEncounter in part 3.

- open (Suggestion): simulation turns to any of 4 directions (can repeat the blocked one), plan says "random walkable direction"; record as deviation in slice note
- open (Suggestion): `seed + Seeds` reuses the seed-count constant as the turn-seed offset; name it (TurnSeedOffset)
- open (Suggestion): Walk_WhileEncounterPending_Throws also asserts TryInteract; split into TryInteract_WhileEncounterPending_Throws
- follow-up: 4b items still open (MapReferences/MapContentReferences naming; "never back to back" doc wording)
