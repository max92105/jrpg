# NG2b. The new game's lists: leader choices and starting items

Editor, about 320 lines (about 170 of them tests). Needs NG2 (`NewGame/NewGameViewModel`, its fixture, the step).
Split from NG2 on 2026-11-17 (NG2's estimate passed 320 lines with its tests). Design: sections 1, 3; decisions 968,
969; the mode bar's WS12 recipe, steps 1 and 7.

## Goal
`NewGameViewModel` types its two lists, which NG2 keeps as the record's:
- **Leader choices** in the order the leader screen shows them: members picked by name from the draft's members,
  added after the chosen row, moved up and down, removed (at least one: Data's rule, shown on the line).
- **Starting items** as rows of an item (the draft's items by name) and a quantity box, added after the chosen row,
  removed, moved.
The problem line puts the quantity boxes before the gold (the page's order), then Data's message. `ToNewGame` keeps the
record's lists while the rows match them in order, so an untouched new game still equals its record (decision 1014's
rule for the class skills).

## New types
- `NewGame/StartingItemRow` (`ObservableObject`): one starting item as typed: the item, fixed, and its quantity box.
  `RecordChoice Item`; `[ObservableProperty] QuantityText`; `Problem` ("Quantity of Potion must be a whole number");
  `InventoryEntry ToEntry()`; `New(RecordChoice)` at quantity 1.

## Changes
- `NewGame/NewGameViewModel` (a `.Lists` partial, as `ClassViewModel.Skills`): `ObservableCollection<RecordChoice>
  Leaders` (a member the draft lacks shown as `RecordChoice.Missing`), `SelectedLeader`, `LeaderToAdd`, `LeadersToAdd`
  (members not listed, by name then id); `ObservableCollection<StartingItemRow> StartingItems`, `SelectedItem`,
  `ItemToAdd`, `ItemsToAdd` (items not listed: Data refuses one listed twice); commands `AddLeader`, `RemoveLeader`,
  `MoveLeaderUp`, `MoveLeaderDown`, `AddItem`, `RemoveItem`, `MoveItemUp`, `MoveItemDown`, each greyed when it can't
  act (`Events/ListOrder`); a list change and a typed quantity raise `Problem`'s change (the quantity only, never the
  row's own problem notice).
- `docs/map.md`, Editor `NewGame` row: `StartingItemRow`, the lists.

## Tests
- `NewGame/NewGameListTests` (new; `NewGameFixtures`, whose leaders are zanyr, omer, ragnir and items potion 5,
  elixir 1):
  - `Constructor_NewGame_ShowsLeadersInTheirOrder`; `Constructor_NewGame_ShowsItemsWithQuantities`.
  - `ToNewGame_LeaderMovedUp_WritesTheNewOrder`; `AddLeader_WithSelection_InsertsAfterIt`;
    `LeadersToAdd_LeavesListedMembersOut`; `RemoveLeader_LastOne_ShowsDataMessage` (NG2's planned
    `Problem_NoLeader_ShowsDataMessage`).
  - `ToNewGame_TypedQuantity_SetsIt`; `Problem_QuantityNotANumber_NamesTheItem`;
    `Problem_QuantityPastMaxStack_ShowsDataMessage` (and the max stack itself accepted).
  - `AddItem_WithSelection_InsertsAfterIt`; `ItemsToAdd_LeavesListedItemsOut`; each move greyed at its end (first row
    up, last row down) and with nothing chosen.
  - `Problem_ListChanged_RaisesProblemChanged`; `Problem_QuantityTyped_RaisesProblemChanged`.
  - `ToNewGame_RowsMatchingTheRecord_KeepsItsLists` (same references).
- `NewGame/StartingItemRowTests` (new): `ToEntry_TypedQuantity_ReadsIt`; `Problem_NotANumber_NamesTheItem`.

## Verify by hand
None: the page comes in NG3.
