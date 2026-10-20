---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, P4-1 (Party page: active party and reserve; PartyPage, PartyMemberCardView, UiTheme.Card, party_* messages, Party tab)
Round: 1 (verdict: Ready to commit; build 0 warnings, 1054 tests pass, format exit 0)

- open (Warning): cards built at page creation/move only; potion on Inventory then Party tab in same visit shows old HP/MP; known but not in plan slice note -> record deviation + follow-up (or refresh hook)
- open (Suggestion): PartyMemberCardView.CardOf(member, bool isLeader) bool param -> pass the leader / derive inside
- open (Suggestion): Notice stays after cursor moves (only cleared on Confirm)
- open (Suggestion): Card.Bars array paired by index with _barLabels -> named Hp/Mp VitalBar fields
- follow-up: PartyState.ReserveMembers in Core instead of Game deriving reserve with Where/Contains
