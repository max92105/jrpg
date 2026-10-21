---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B8a.md, B8a (Escaped: EscapedNotice, BattleScene.DrawEscaped/UpdateOutcomeScreen)
Round: 1 (verdict: Ready to commit; verify all pass, 1294 tests)

- open (Suggestion): DrawEscaped copies BattleBannerView's band drawing (Banner gradient + two HighlightLine hairlines + centered text); a third copy (e.g. B8b defeat) should become a shared band shape in Ui.
- open (Suggestion): the Confirm-to-leave path is only unit-tested on EscapedNotice, not through BattleScene (scene tests use the 10 s Settle).
- open (Follow-up): max-level member shows "EXP 0 /0" and an empty bar (carried from B7a).
- open (Follow-up): victory cards grow down by index/2 with no cap; >6 active members run into the footer (carried from B7b1).
