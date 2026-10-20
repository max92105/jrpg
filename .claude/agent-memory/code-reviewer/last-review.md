---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-menu-cleanup/slices/P1.md, P1 (PartyCursor extracted from PartyPage, no behavior change)
Round: 1 (verdict: Ready to commit; verify all pass, 1131 tests; navigation and notice clearing checked identical)

- open (Suggestion): tick P1 in README.md:109; note in P1.md that PartyPage keeps MoveInColumn/SwitchColumn wrappers for the notice and the private CardColumn record
- open (Suggestion): add PartyCursor to docs/map.md Menus row (next to PartyPage)
- open (Suggestion): ReturnTo with a non-zero row (cancelled swap) only covered indirectly via PartyPageTests; optional direct test
