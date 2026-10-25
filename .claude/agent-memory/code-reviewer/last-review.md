---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/FM2.md, slice FM2 (Move command on screen, spots and panels from the formation)
Round: 1 (verdict: Ready to commit)

- Suggestion, open: Update_ConfirmMove_TargetsOnlyNeighbors uses two members, so it can't tell "neighbors" from "every other ally"; three members with an end actor would.
- Suggestion, open: ChooseTarget picks MoveCommand from the list cursor (`_ when Selected == Move`); a target cursor that remembers what opened it would drop the hidden coupling.
- Suggestion, open: PartyStatusView's draw order is a rule inside an untested view; a small pure ordering would make it testable.
- Follow-up: Move icon borrows Run.png (UiArtFiles) until it has its own art.
