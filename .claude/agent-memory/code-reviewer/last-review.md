---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-menu-cleanup/slices/P2.md, P2 (PartyColumnsView, PartyColumns, PartyActionChoiceView out of PartyPage, no behavior change)
Round: 1 (verdict: Ready to commit; verify all pass, 1132 tests; draw geometry, notice/choice no overlap, ShowParty after every mutation checked)

- open (Suggestion): tick P2 in README.md:110; record in P2.md the deviations (Active/Reserve snapshots in PartyColumns, static ActiveCardArea instead of CardArea(bool), ChangeOrderEntry/EntryCount on the view)
- open (Suggestion): add PartyColumns/PartyColumnsView, PartyActionChoiceView to docs/map.md Menus row
- open (Suggestion): PartyColumnsView.CardColumn carries the whole Place as `Page` only for Cards/Moving; pass those two (or drop CardColumn.Page)
