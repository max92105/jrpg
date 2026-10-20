---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-menu-cleanup/slices/U4.md, U4 (Ui/MemberCardView shared by PartyPage and ItemTargetPickerView)
Round: 1 (verdict: Ready to commit; verify all pass, 1117 tests)

- open (Suggestion): add MemberCardView to docs/map.md Ui row; tick U4 in README.md:99
- open (Suggestion): odd last card sits in the left column, not centered; note for question 112 when checking by hand
- open (Follow-up): band grows with rows and overflows 1080 px past ~7 rows (14+ members); question 112 / scrolling later
- fixed (U3 follow-up): picker no longer formats value texts it never draws
