# NG2c. The new game's starting items

Editor, about 220 lines (about 130 of them tests). Needs NG2b (the leader list, `ListOrder.CanMoveUp`,
`ListItemWatch.WatchRowsOf`). Split from NG2b on 2026-11-17. Design: sections 1, 3; decisions 968, 969; the mode bar's
WS12 recipe, steps 1 and 7.

## Goal
`NewGameViewModel` types its starting items, which NG2 kept as the record's: rows of an item (the draft's items by
name, those not listed: Data refuses one listed twice) and a quantity box, added after the chosen row, removed, moved.
The problem line puts the quantity boxes before the gold (the page's order), then Data's message. `ToNewGame` keeps the
record's list while the rows match it in order. A list change and a typed quantity raise `Problem`'s change once (the
quantity only, never the row's own problem notice), through `ListItemWatch.WatchRowsOf`.

## New types
- `NewGame/StartingItemRow` (`ObservableObject`): one starting item as typed: the item, fixed, and its quantity box.
  `RecordChoice Item`; `[ObservableProperty] QuantityText`; `Problem` ("Quantity of Potion must be a whole number");
  `InventoryEntry ToEntry()`; `New(RecordChoice)` at quantity 1.

## Changes
- `NewGame/NewGameViewModel.StartingItems` (new partial): `ObservableCollection<StartingItemRow> StartingItems`
  (an item the draft lacks shown as `RecordChoice.Missing`), `SelectedItem`, `ItemToAdd`, `ItemsToAdd`; commands
  `AddItem`, `RemoveItem`, `MoveItemUp`, `MoveItemDown`, each greyed when it can't act (`ListOrder.CanMoveUp`).
- `docs/map.md`, Editor `NewGame` row: `StartingItemRow`, the items list.

## Tests
- `NewGame/NewGameItemListTests` (new; `NewGameFixtures` gains its own items, so no repository item is pinned):
  - `Constructor_NewGame_ShowsItemsWithQuantities`; `ToNewGame_RowsMatchingTheRecord_KeepsItsList` (same reference).
  - `ToNewGame_TypedQuantity_SetsIt`; `Problem_QuantityNotANumber_NamesTheItem` (before the gold's);
    `Problem_QuantityPastMaxStack_ShowsDataMessage` and the max stack itself accepted.
  - `AddItem_WithSelection_InsertsAfterIt`; `ItemsToAdd_LeavesListedItemsOut` (by name); each move greyed at its end
    and with nothing chosen, re-raised by a move.
  - `Problem_ListChanged_RaisesProblemChanged`; `Problem_QuantityTyped_RaisesProblemChanged`; a row's own problem
    notice raising nothing.
- `NewGame/StartingItemRowTests` (new): `ToEntry_TypedQuantity_ReadsIt`; `Problem_NotANumber_NamesTheItem`.

## Verify by hand
None: the page comes in NG3.
- **Note from NG2b review:** count `Problem` raised exactly once on a list change (`NewGameLeaderListTests`'
  `RemoveLeader_Chosen_RaisesTheListsNotices` only checks it was raised; do the same for the items); and a comment on
  why value-equal rows (`RecordChoice` is a record) are safe for `IndexOf` (the loader refuses a repeated leader).
- **Done, with deviations:** both review notes taken (the leaders' notices test now counts one; the comment is on
  `Leaders`). `NewGameFixtures` replaces the test world's items with its own four (Tonic, Ember stacking to 3, Salve,
  Yarrow), so `CatalogDraftNewGameTests`' changed quantity moved to them. Beyond the list: the missing item shown, a
  move down written, Remove choosing the next row, Add's row at one copy, every command greyed in one theory, the
  choices' `CanExecuteChanged`, an added row's quantity relayed and a removed one's not, `ToEntry` throwing, two bad
  quantities naming the first row's item. Review: the third copies extracted, used by the leaders, the items and a
  class's skills (and its resources for the last): `RecordChoice.Of` (missing when the draft has no name) and
  `RecordChoice.ByName` (an add box's order), `ListOrder.KeptIfEqual` (decision 1014's kept list); the existing tests
  ran unchanged; the two rows' shared shape is a backlog line. Bigger than planned: about 175 lines of code, 330 of
  tests (38 tests). Sizes: `NewGameViewModel` 127, `.StartingItems` 108, `.Leaders` 87, `StartingItemRow` 42,
  `RecordChoice` 39, `ListOrder` 65, `ClassViewModel.Skills` 108, `.Resources` 120; tests `NewGameItemListTests` 244,
  `StartingItemRowTests` 35, `NewGameFixtures` 59, `RecordChoiceTests` 42, `ListOrderTests` 90.
