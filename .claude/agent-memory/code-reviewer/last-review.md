---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B7b2.md, B7b2 (victory timing: VictoryEntrance, LevelUpPop, ExperienceBarAnimation.SinceLastLevelUp, "Next" hint)
Round: 1 (verdict: Ready to commit; verify all pass, 1289 tests)

- open (Suggestion): cubic ease-out + clamped share copied in LevelUpPop.ScaleAt and VictoryEntrance.EaseOut/ShareOf; share one (third copy would be a Warning).
- open (Suggestion): VictoryEntranceTests header says the bars wait "0.5 s past the last card's start"; code/legacy wait 0.5 s past firstCardDelay + stagger*count (0.1 s after the last card's start).
- open (Suggestion): VictoryScreenTests.Update_Done_ShowsNextHint asserts in the Arrange section; split into its own fact or drop (BarsWait covers it).
- open (Follow-up): max-level member shows "EXP 0 /0" and an empty bar (carried from B7a).
- open (Follow-up): cards grow down by index/2 with no cap; >6 active members run into the footer (carried from B7b1).
