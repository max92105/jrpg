---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-24-class-mechanics/slices/RS3.md, slice RS3 (Camp Kit rest item used from the menu)
Round: 1 (verdict: Ready to commit)

- Warning: PartyMember.HasAnythingToRest lasting-resource branch untested (full HP, mana carried off StartsAt -> kit must be usable); open
- Suggestion: ItemUseDialog.Open now may use the item without opening; consider a name like OpenOrUse / UseOrPickTarget; open
- Suggestion: SkillCatalogValidatorTests.Validate_RestSkill_Throws uses EndsWith; pin full message like the item test; open
- Follow-up: InventoryPage.cs is 328 lines (was 322 before diff); split candidate; open
- Follow-up: docs/ui_ux_exemples webp swap is unrelated to RS3; commit separately; open
