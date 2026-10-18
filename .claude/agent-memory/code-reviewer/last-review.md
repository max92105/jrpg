---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 3d (Interaction and touch)
Round: 1 (verdict: Fix the Criticals, then commit)

Accepted choices (don't re-raise): touch disarmed at map entry, any object touch event disarms; cell events edge-triggered, start cell counts as entered; Walk/TryInteract throw InvalidOperationException while an event runs; ContinueEvent on WarpStep throws NotSupportedException with TODO(slice 4a); Location allocates a record per call.

- open (Critical): ExplorationState.TryStartCellEvent "once and done" branch has no test; add Walk_ReenterOnceCellEventDone_DoesNotStartIt
- open (Warning): cell (touch) events don't disarm object touch; sign cell in front of a touch door -> door fires right after the dialogue while the player keeps pushing (plan: "since the last touch event")
- open (Suggestion): InteractionFinder looks up CurrentDefinitionOf for objects without events every moving frame; skip Events.Count == 0 first (legacy HasEvents)
- open (Suggestion): TryInteract-while-event-running throw is untested (Walk's is)
- open (Suggestion): Game will read Location per frame; consider exposing Position/Facing or caching when 4a wires it
