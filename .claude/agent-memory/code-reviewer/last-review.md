---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-escape-menu-third-pass/slices/F1.md, slice F1 (walk-through findings: Info toggles the item description, SectionTitle bands, Status redone, gliding pointers)
Round: 1 (verdict: Ready to commit)

- W1 docs/ui-reference/README.md:15, export-index.md:64, slices/Z.md:13, stats plan L1.md/design.md:170 — still name the deleted menu-status.webp; point them at menu-status.png; open
- W2 CategoryRailView:58, PartyColumnsView:87 (+ UiTextures:224) — third copy of `new Vector2(row.Area.X - row.PointerOffset, row.Area.Center.Y)`; add GlidingRow.PointerTip and use it in all three; open
- W3 PartySidebarView DrawPointer — `_cardsInView == 0` can't hold while PointedIndex is set; `_cardsInView` is a field used as a hidden parameter; pass the count, drop the guard; open
- S1 StatusSheetView layout comment — "180 px label column and 200 px value column" don't exist (CellWidth 380); "92 px under it" ambiguous; open
- S2 StatusSheetView — progress indices 0/1/2 bare, Next measured from Progress[^1] but labelled Progress[2]; name them; open
- S3 content/text/ui.json menu_hint_inventory — hardcodes "X (Y on a controller)"; ButtonLabels + {button} as cinematic_skip does; open
- F1 Status skills top now grows with max(slots, resources) rows; nine slots (stats plan) push skills off the page; follow-up
