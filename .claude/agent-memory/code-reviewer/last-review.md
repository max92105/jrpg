---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 3a (map events and world state)
Round: 1 (verdict: Ready to commit)

Accepted deviations (don't re-raise): WorldState restore ctor + enumerations deferred to 5a; EventRun throws for sourceless swap and unknown step; ActiveMap.IsDone has no production caller until 3d.

- open (Suggestion): EventRunTests Continue_GiveItemBeyondMaxStack_ReceiptReportsLostCopies never calls Continue (give is the first step, applied in the constructor); rename Constructor_... or lead with a dialogue step
- open (Suggestion): EventRun.WaitsAfterApplying reads as a pure predicate but mutates inventory/world; rename e.g. ApplyAndCheckWait
- open (Suggestion): plan 3a not ticked and the three deviations not yet in a slice note (DoD 5)
- follow-up: 3b validator must reject null events/cellEvents/steps/lines and null list entries (STJ lets them through)
