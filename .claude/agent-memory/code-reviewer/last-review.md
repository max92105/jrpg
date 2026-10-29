---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-27-forge, slice M1 (Materials: ItemDefinition.IsMaterial, validator FindMaterialProblem, RarityMaterial
teal, InventoryRow/VictoryDropRow.IsMaterial, Materials tab, scrap/essence content, goblin drops).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (2817 tests).

- Warning: ShowCategory/ShowMaterials tab loop now in 3 test files (InventoryPageGearTests:92, InventoryPageTests:224,
  InventoryPageMaterialTests:52); move to one shared test support type - open
- Warning: InventoryPageMaterialTests.Update_MaterialsTab_ListsOnlyTheScrap asserts the whole tab == ["scrap"] over the
  content start inventory (G2); assert Contains scrap + DoesNotContain potion - open
- Suggestion: VictoryDropsView.RowsOf stack lambda ~200 cols; extract a StackRowOf beside RowOf(GearPiece) - open
- Suggestion: InventoryRow.IsMaterial summary says only the name is drawn in the color; the count is too - open
- Suggestion: TextColorOf_UnusablePlainItem_IsGreyed sets Rarity = Common, the default; drop it - open
