# GB1. The Weapons and Armor tabs show no quantity column

Game, about 50 lines, hand check. Builds on decision 553 and design.md, "Structure".

## Goal
On the Inventory page, the Weapons and Armor tabs list only gear pieces, which have no count, so their list shows the Item column title alone; All, Consumables and Materials keep Item and Qty. Today `InventoryColumnView` builds one `ColumnTitles` with both and draws it on every tab.

## Changes
- `src/Jrpg.Game/Menus/InventoryFilter.cs` (38 → about 45): `public static bool ShowsCounts(InventoryCategory category) => category is not (InventoryCategory.Weapons or InventoryCategory.Armor);`, false for the two tabs whose rows are all pieces (ADR 0029). Summary: "Whether the category's rows carry a count, so its list has a Qty column." The class summary, which names only what each category shows, gains its second job (a type gaining a public member): "... and whether its list has a Qty column."
- `src/Jrpg.Game/Menus/InventoryColumnView.cs` (90 → about 100): keeps the categories it is given, builds two `ColumnTitles` (with and without Qty) once in its constructor, and draws the one `ShowsCounts` picks for the shown tab's category. Nothing is allocated per frame.
- `docs/plans/backlog.md`: delete the P1a line.

## Tests
- `InventoryFilterTests: ShowsCounts_Category_SaysWhetherRowsHaveACount` (a theory, one row per category: All true, Consumables true, Weapons false, Armor false, Materials true). Fails before the change (the method doesn't exist; it compiles red first). The Weapons and Armor rows fail against a body returning true, the others against one returning false.

The draw itself has no test (decision 551: a look is checked by hand). The runner's totals per project before and after in the Done note (Game.Tests five cases more).

## Verify by hand
The developer confirms the look (decision 553 is taken; the check is that it reads right).
1. Open the menu, Inventory: on All, the column titles read Item and Qty.
2. Weapons and Armor: only Item; the rows are as before (no count).
3. Consumables and Materials: Item and Qty, the counts under Qty.
4. Switching tabs back and forth: the titles follow at once, nothing flickers.

- **Done, with deviations:** as planned, the theory's rows given as the category's name (`InventoryCategory` is internal, so a public theory can't take it as a parameter) and parsed. Shown failing with Armor left out of the rule (the Armor row failed, the rest passed), then the rule as planned. `InventoryColumnView` builds `_titlesWithCounts` and `_titlesWithoutCounts` once from the shared `itemTitle` and `quantityTitle`, and reads the shown category from the rail (`place.Rail.Category`, the review: no second copy of the tabs). Totals per project: Game.Tests five more (4610 in all). The P1a line went.
