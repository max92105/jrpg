---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B4c.md, B4c (CommandPanelView, CommandPanelPlacement, PointerBob, OutlinedShape/CommandHeaderShape)
Round: 1 (verdict: Ready to commit; verify all pass, 1211 tests)

- open (Suggestion): CommandPanelView._pointerTime never resets; PointerBob param doc says "since the pointer appeared". Reset when Targets goes from null to set, or reword the doc.
- open (Suggestion): OutlinedShape.Pixels names the path `outline` while NavTabShape/CommandHeaderShape name the edge color `outline`; rename the path param to `corners`.
- open (Suggestion): PointerBob tests only pin 0 and peak; add 0.225 s -> 4 px to pin the cosine ease.
- follow-up: none.
