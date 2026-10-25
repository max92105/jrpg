---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/RG3.md, slice RG3 (Ready: extra turn after a chosen combatant)
Round: 1 (verdict: Ready to commit)

- Warning: TurnOrder.Advance skips TakeReadiedAfter after an extra turn, so a second Ready on the same anchor plays a full cycle late; open
- Warning: ItemCatalogValidator accepts targeting otherCombatant on a menu-usable item; FieldItemUse.CanUse (Self relation) is then always false; open
- Suggestion: TurnFlow.AdvanceToNextActor comment "the lost turn still runs its upkeep, so the stun counts down" is false for a skipped extra turn; open
- Suggestion: Execute_StunnedOnExtraTurn_TurnSkipped could pin that the stun did not count down on the lost extra turn; open
