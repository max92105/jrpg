---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B6a.md, B6a (skills in battle: TargetChoiceFor/CanAfford, BattleSkillChoice, CommandMenu 3 levels, panel skill rows, skill icon art check)
Round: 1 (verdict: Ready to commit; verify all pass, 1259 tests)

- open (Suggestion): CommandPanelView.DrawRowLabel takes a (IconFile, Label, Opacity) tuple to stay under 4 params; a named readonly record struct would read better.
- open (Suggestion): DrawRowBacks re-derives rowCount from frame.Height; pass rowCount (callers already have it).
- open (Suggestion): menu tests run with a one-member party, so the heal_all test can't tell "every living ally" from "the actor"; no menu test for "MP but no target: Confirm does nothing" (Core CanUse test covers the rule).
