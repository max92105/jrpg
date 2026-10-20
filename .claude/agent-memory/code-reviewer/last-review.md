---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, P2a (Field item targets in Core: TargetChoice<T>, FieldItemUse.TargetChoiceFor, CanUse needs a legal target)
Round: 1 (verdict: Ready to commit; build 0 warnings, 1017 tests pass, format exit 0)

- open (Warning): FieldItemUse.Use InvalidOperationException doc + message say "not usable there, or not owned" but now also thrown for a revive with nobody fallen -> add "or nobody it can be used on"
- open (Suggestion): TargetChoiceFor's ArgumentException (item with no effect) has no test
- open (Suggestion): tick P2a, note test names drop the FieldItemUse_ prefix, the extra CanUse_ReviveWithSomeoneFallen test, and the Arrange line added to Use_FallenAllyItemOnLivingMember
