---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-menu-cleanup/slices/E1.md, E1 (EquipmentSheet + StatPreviewText pure types; PartyMember.EquippedIn public)
Round: 1 (verdict: Ready to commit; verify all pass, 1125 tests)

- open (Suggestion): tick E1 in README.md:103; note "Slots is a static get-only property, not a field (CA)" in E1.md deviations
- open (Suggestion): add EquipmentSheet and StatPreviewText to docs/map.md Menus row
- open (Follow-up): private one-line Number(int) now copied in 6 Game files (EquipmentSheet, StatPreviewText, StatusPage, SaveSlotListView, MemberCardView...) -> one shared invariant formatter
- open (Follow-up): StatusPage slot labels could use EquipmentSheet.Slots/LabelIdOf (README:73)
