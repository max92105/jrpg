# SH3. The Shops window and the Database menu

Editor, about 170 lines (window code and XAML). Decision 583. Builds on SH2 and plan 1's SI4 (the status lines already moved out of `MainWindow.xaml`).

## Goal
Database > **Shops...** opens the Shops window: shops on the left with Add and Remove, the chosen shop on the right (id, name, its items with Add from a box of priced items, Remove, Up, Down), the problem line, OK greyed while it shows. The Quests menu becomes Database with Quests... and Shops....

## New types
- `Editor/Trade/ShopCatalogWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style): the window above, laid out as `QuestCatalogWindow`.
- `Editor/Shell/MainWindow.Database.cs` (partial): `OnEditShopsClicked` (start, show, apply), as `MainWindow.QuestEditing`.
- `Editor/Shell/MainViewModel.Database.cs` (partial): `ShopEditing`, `HasShopEditing`, built by `StartDatabaseEditing()`, which the constructor calls once where it builds `QuestEditing`.

## Changes
- `Shell/MainWindow.xaml` (286 after plan 1): the `Quests` menu header becomes `Database`, its entry `Quests...`, then `Shops...` (287).

## Tests
None new: window code; SH2 tests the rules.

## Verify by hand
1. `dotnet run --project src/Jrpg.Editor`. Database > Shops...: `general_store` with its eight items in order.
2. Add shop: id `smithy_wares`, name `Smithy`, add Bronze Sword and Wooden Shield; clear the name: the problem line shows Data's words and OK greys; type it back, OK.
3. Ctrl+Z: the shop goes; Ctrl+Y. Ctrl+S: `git diff content/shops.json` shows the new shop, and no other catalog file changed.
4. Place an NPC, give it the Shopkeeper preset (Events mode), pick `smithy_wares` in its open shop step, Ctrl+S, play: talking to it opens "Smithy" selling the two items.
5. Undo back past the shop while the map step that opens it is saved on another map: refused, naming the map.
6. Revert with `git checkout -- content` if this was only a check.

- **Done, with deviations:** `ShopCatalogViewModel.CanApply` greys OK live with the problem line (the editor rules' dialog pattern), so the window's code-behind only closes; `ShopEditing` is built in the constructor beside `QuestEditing`, the partial holding the properties; the Quests menu's wording became "Database > Quests..." in the summaries and the codebase map (decision 583).
