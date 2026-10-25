---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/M1a.md, slice M1a (combatants carry class resources, per-turn gain)
Round: 1 (verdict: Ready to commit)

- Suggestion, open: TurnUpkeep.cs:8 `.Where(PerTurn != 0)` duplicates the `change != 0` guard; drop the filter, a plain foreach is enough.
- Suggestion, open: ResourceMeters.cs:23 message "No resource 'x' here." is vague; say "This combatant has no resource 'x'.".
