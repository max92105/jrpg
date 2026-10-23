---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-code-health/slices/P1.md, slice P1 (sidebar lists active + reserve, marks shown member, scrolls)
Round: 1 (verdict: Ready to commit)

- Suggestion, open: GameMenuSceneTests new NewMenu(session, sidebar, pages) repeats FrameOf's body; FrameOf(session, sidebar) overload.
- Suggestion, open: FirstVisible survives a switch to a page with no shown member (Status scrolled to 7th -> Inventory shows cards 2-7, leader hidden); reset to 0 when shown is null, or ask the developer.
- Suggestion, open: IMemberPage.Shown has no summary; one line ("the member on screen now") would help the next implementer.
