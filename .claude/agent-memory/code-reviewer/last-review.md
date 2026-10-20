---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-18-game-title-menu-movement.md, P3b (Changing equipment with a stat preview; WearableFor, EquipmentChoice, EquipmentChoiceView, EquipmentPick, StatPreview, EquipmentPage choice/notice/preview)
Round: 1 (verdict: Fix the Criticals; build 0 errors, 1046 tests pass, format exit 0)

- open (Critical): EquipmentPage.DrawPreview(..., bool isAttack) is a flag argument -> nested record PreviewValue(Text, Color), PreviewText(Attack, Defense), DrawPreview(batch, textures, position, PreviewValue)
- open (Warning): Remove row highlighted shows no preview; legacy previews null = empty slot (attack drop) -> preview the empty slot or record as deviation
- open (Suggestion): EquipmentChoice.VisibleRows doc names 44 px row height (view detail in pure type)
- open (Suggestion): record deviations in P3b slice note (WearableFor takes catalog, ConfirmRemove test lives on EquipmentPage, Remove no preview)
- open (Suggestion): highlight-change preview recompute untested at page level
