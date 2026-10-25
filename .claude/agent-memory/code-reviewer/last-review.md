---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/FT1.md, slice FT1 (fighter stamina + Shield Bash stun; statuses on screen)
Round: 1 (verdict: Ready to commit; verify all pass, 1861 tests)

- Warning, open: PartyStatusPanel.Statuses is a StatusTagsText, beside Combatant.Statuses (a StatusSet); rename StatusTags.
- Suggestion, open: no test that two TurnSkipped in one action share one Skip beat and one ShowsAt (ActionPlayback.AddSkipCue).
- Suggestion, open: PlaceholderCue.OpacityAt's new "before its moment is 0" branch has no test.
- Follow-up: Shield Bash on the very next actor grants and ends the stun in one Execute, so the "STUN 1" tag never shows (hand check wording / design).
- Follow-up (from ST2, still open): BattleState size; IsOver inside AdvanceToNextActor untested until a damage-over-time status.
