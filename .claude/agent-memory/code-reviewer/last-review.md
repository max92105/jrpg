---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/FM1.md, slice FM1 (party formation line, MoveCommand, PlacesSwapped)
Round: 1 (verdict: Ready to commit)

- Warning, open: enemy-actor Move guard (InvalidOperationException, BattleState.Move) and TargetChoiceForMove's empty choice on an enemy turn have no test; Flee's equivalent guard is tested (BattleStateTests:181).
- Suggestion, open: Formation constructor public; make it internal so only BattleState builds the line (tests see internals already, Swap is internal).
- Suggestion, open: no Formation test for Swap of non-neighbours / unknown member / PositionOf throw at Formation level (covered only via Execute).
- Follow-up: BattleState at 319 lines; turn flow split recorded in slice note for PL2a.
