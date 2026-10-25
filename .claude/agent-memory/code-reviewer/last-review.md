---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/RG4.md, slice RG4 (rogue kit: Ready and Expose, Exposed status)
Round: 1 (verdict: Ready to commit)

- Warning: BattleTurnSequenceTests.Update_AfterReady_SameMemberStepsForwardAgain passes even if Ready is not quick (goblin acts, then readied turn awaits); pin UpcomingActors; open
- Suggestion: EffectLanding.cs:21 ~185-column call; extract a `damageTakenPercent` local; open
- Suggestion: CommandMenuTests name says "BothSides" but the solo-Lazarus fixture only has enemies; open
- Closed (RG3, commit f351825): TurnOrder.Advance checks readied turns after an extra turn too (lastActor rule), and ItemCatalogValidator rejects otherCombatant targeting on items. Don't re-raise them.
