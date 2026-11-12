# SH2. The Shops window's rules

Editor, about 200 lines (100 of them tests). Decisions 581, 582, 584. Builds on SH1.

## Goal
The Shops window's state, headless: every shop in id order, the ones added this session after them; a shop's id (typed while new), name and items in order, added from the priced items (sorted by name, id beside it), removed and moved; the problem line; the shops it gives the draft.

## New types
- `Editor/Trade/ShopViewModel` (internal sealed partial, `ObservableObject`): one shop as edited: `Id` (typed while `IsNew`), `Name`, `ItemIds` (an `ObservableCollection<string>`), `SelectedItem`, `ItemToAdd` (from the offered items), commands Add item, Remove item, Move up, Move down (`Events/ListOrder`); `Problem` (an empty new id or one breaking `ObjectIds`' rule; then nothing else: Data's message is the list's); `ToShop()`.
- `Editor/Trade/PricedItemChoice` (internal sealed record): one item the picker offers: id, name and price, `Label` ("Potion (potion), 20 g").
- `Editor/Trade/ShopCatalogViewModel` (internal sealed partial, `ObservableObject`): every shop, `SelectedShop`, Add shop and Remove shop (only while new, decision 581), `OfferedItems` (the draft's items with a price, sorted by name), `Problem` (a shop's own problem by id or position, then `CatalogProblems.OfShops` on the whole list), `ToShops()`.
- `Editor/Trade/ShopEditingViewModel` (internal sealed): `Start()` and `Apply(edited)` (`CatalogDraft.SetShops`), as `Quests/QuestEditingViewModel`.

## Changes
None outside the new folder.

## Tests
- `ShopViewModelTests` (new): `From_Shop_ReadsNameAndItemsInOrder`; `AddItem_ChosenItem_AppendsAfterSelected`; `AddItem_AlreadyListed_IsGreyed` (Data refuses a doubled item; the command's `CanExecute` says so first); `MoveUp_SecondItem_SwapsWithFirst`; `Problem_NewShopWithoutId_AsksForIt`; `Problem_SavedShop_IdNotChecked`.
- `ShopCatalogViewModelTests` (new): `Shops_Draft_InIdOrder`; `OfferedItems_OnlyPricedOnes_SortedByName`; `RemoveShop_SavedShop_IsGreyed`; `RemoveShop_NewShop_Removes`; `Problem_EmptyName_IsDataMessage`; `Problem_TwoNewShopsSameId_SaysSo`; `ToShops_Unchanged_EqualsDraft`.
- `ShopEditingViewModelTests` (new): `Apply_AddedShop_IsOneUndoStep`; `Apply_Unchanged_RecordsNothing`.

## Verify by hand
None: the window is SH3.
