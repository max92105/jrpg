---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, P4-2 (Changing the order: PartyOrderEdit.Swap, PartyPage action choice "Change order"/"To reserve", swap mode, party_change_order/party_to_reserve)
Round: 1 (verdict: Ready to commit; build 0 warnings, 1063 tests pass, format exit 0)

- open (Suggestion): PartyPage.Moving (PartyMember?) reads like a bool state -> MovingMember
- open (Suggestion): UpdateSwap Cancel does ActiveMembers.ToList().IndexOf(moving) -> keep the moving member's row in a field when the swap starts
- open (Suggestion): Update_LeaderSelected_OffersNoMove could also assert LeavesOnCancel stays true (no choice opened for the leader)
- follow-up: Core query (e.g. PartyState.CanLeave / leader's place) so Game stops restating the leader rule (member == Leader check, row 0 skipped)
- follow-up (from P4-1, still open): PartyState.ReserveMembers in Core; cards refresh hook
