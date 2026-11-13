# DT3. A table's entries edited

Editor, about 210 lines (about 120 of them tests). Design: "Tunables and placeholders", key decision 3. Decisions 597 (the item fixed once added), 598. Needs DT2.

## Goal
A table's entries are added from a box of every item, removed and moved; every typed change or list change is told through one event, which the window's list (DT4) will listen to.

## New types
- `Editor/Loot/DropItemChoice` (record): one item the entry box offers: `Id`, `Name`, `Label` "Potion (potion)".

## Changes
- `Editor/Loot/DropEntryRow`: `public static DropEntryRow New(DropItemChoice item)` with the tunable defaults (chance 10, quantities 1 to 1; named constants citing the design's table).
- `Editor/Loot/DropTableViewModel`:
  - `ItemToAdd` (`DropItemChoice?`, notifying `AddEntryCommand`).
  - `AddEntryCommand`: a new row after the chosen one, or last when none is chosen, then chosen; the same item may be added twice (no rule Data lacks: the README's guiding rules).
  - `RemoveEntryCommand`, `MoveEntryUpCommand`, `MoveEntryDownCommand` through `Events/ListOrder`, greyed while no entry is chosen; rows renumbered (`Index`) after each.
  - `event EventHandler? EntriesChanged`: raised when a row's `ChanceText`, `MinQuantityText` or `MaxQuantityText` changes or the collection changes; never on a row's `Problem` or `Index` notice (`.claude/rules/editor.md`, typed fields only). Rows added are watched, rows removed unhooked.

## Tests
- `DropTableEntryEditingTests` (new sibling of `DropTableViewModelTests`, named for this slice):
  - `AddEntry_SecondRowChosen_InsertsAfterItWithDefaults` (index 2, chance "10", quantities "1").
  - `AddEntry_NothingChosen_AddsLast`.
  - `AddEntry_ItemAlreadyListed_AddsASecondRow`.
  - `AddEntry_NoItemPicked_IsGreyed` (the guard: `CanExecute` false).
  - `RemoveEntry_Chosen_RemovesItAndRenumbers` (the last row's `Index` drops by one).
  - `RemoveEntry_NothingChosen_IsGreyed`.
  - `MoveEntryUp_Chosen_SwapsWithTheOneAboveAndRenumbers` (the moved row's `Index` and its old neighbour's both change: `[3]` becomes `[2]` and the other `[3]`).
  - `MoveEntryDown_Chosen_SwapsWithTheOneBelowAndRenumbers` (likewise, the other direction).
  - `MoveEntryDown_Chosen_KeepsItChosen` (the moved row stays `SelectedEntry`, so a second Down moves it again).
  - `EntriesChanged_RowChanceTyped_IsRaisedOnce` (typing raises the row's `ChanceText` and `Problem` notices: a relay reacting to every notice counts two).
  - `EntriesChanged_RowMaxTyped_IsRaisedOnce` (the last relayed box).
  - `EntriesChanged_EntryAdded_IsRaised`.
  - `EntriesChanged_RemovedRowTyped_IsNotRaised` (the unhook).

## Verify by hand
None: no window yet (DT5).
