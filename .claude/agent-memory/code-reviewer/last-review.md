---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-escape-menu-third-pass/slices/IT3.md, slice IT3 (aimed cards show HP now and after; FieldItemUse.HpAfterUse)
Round: 1 (verdict: Ready to commit)

- S1 PartySidebarView.FormatChangedPreviews — "candidates get a preview, the others none" has no test; a headless PartySidebarView test (as MemberCardViewTests builds MemberCardView) could pin it; open
- S2 PartySidebarView — previews refresh keyed on Picker reference, relying on ItemUse building a new picker in every AimFor; comment states it; key on the aim's HP-after lookup if that ever changes; open
