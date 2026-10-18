---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 4b (Encounter rolls and per-map tables)
Round: 1 (verdict: Ready to commit)

Accepted (don't re-raise): RandomEncounters has no production caller until 4c; RollStep counts steps on maps without a table (unobservable, since map entry resets); graceSteps >= 0 is the plan's rule.

- open (Warning): MapContentReferences and MapReferences repeat 3 of 4 fields under names that don't say which is which; MapFolderReader copies them field by field and passes EncounterIds beside `references`. Suggest MapReferences(MapContentReferences Content, Layouts) or one record.
- open (Suggestion): validator accept-boundaries untested (graceSteps 0 and oneInSteps 1 accepted)
- open (Suggestion): "so battles never come back to back" in RandomEncounterTable/RandomEncounters docs is false for graceSteps 0, which the validator allows
