# NG2. The new game as typed: leader choices, starting items, gold, start, sprite

Editor, about 230 lines (about 120 of them tests). Needs NG1, PM2 (`RecordChoice`). Design: sections 1, 3; decisions
968, 969; the mode bar's WS12 recipe, steps 1, 2 and 9.

## Goal
The New game page's state and its apply, headless:
- **Leader choices** in the order the leader screen shows them: members picked by name from the draft's members,
  added after the chosen row, moved up and down, removed (at least one: Data's rule, shown on the line).
- **Starting items** as rows of an item (the draft's items by name) and a quantity box, added after the chosen row,
  removed, moved.
- **Starting gold** (a whole number box).
- **Start:** the map (the draft's maps by id), x and y (decimal boxes in cells, `double.IsFinite` checked), facing
  (Core's `Facing` values).
- **Player sprite:** an editable box over the draft's field sprite files.
The problem line is its typed boxes first, then Data's message (`CatalogProblems.OfNewGame`). Applying records one
`CatalogDraft.SetNewGame` step.

## New types
- `NewGame/StartingItemRow` (`ObservableObject`): one starting item as typed: the item and its quantity box.
  `RecordChoice Item`; `[ObservableProperty] QuantityText`; `Problem`; `InventoryEntry ToEntry()`.
- `NewGame/NewGameViewModel` (`ObservableObject`, `IEditedCatalog`): the new game as the New game page types it.
  - `NewGameViewModel(ContentCatalog content)`; `ObservableCollection<RecordChoice> Leaders`, `SelectedLeader`,
    `LeaderToAdd` and `LeadersToAdd` (members not listed); `ObservableCollection<StartingItemRow> StartingItems`,
    `SelectedItem`, `ItemToAdd`; `GoldText`; `MapId`, `MapIds`; `XText`, `YText`; `Facing`; `FieldSpriteFile`,
    `FieldSpriteFiles`.
  - Commands: `AddLeader`, `RemoveLeader`, `MoveLeaderUp`, `MoveLeaderDown`; `AddItem`, `RemoveItem`, `MoveItemUp`,
    `MoveItemDown` (each greyed when it can't act).
  - `IEditedCatalog`: `ChosenKey` is null (one record, nothing to choose); `Choose` keeps everything as it is.
  - `Problem` (raises its change), `CanApply`, `NewGameDefinition ToNewGame()` (throws on a problem).
- `NewGame/NewGameEditingViewModel(CatalogDraft catalog)` (`ICatalogEditing<NewGameViewModel>`): starts the New game
  page from the draft and applies its session as one step.

## Changes
- `Catalogs/CatalogDraft.Party.cs`: `SetNewGame(NewGameDefinition newGame)`: one step, nothing when the same by content
  (leader choices and starting items by sequence); no undo refusal (decision 954: catalog steps undo in order).
- `docs/map.md`, Editor table: a `NewGame` row ("The New game page's rules: leader choices, starting items, gold, start
  and sprite"; `NewGameViewModel`, `StartingItemRow`, `NewGameEditingViewModel`).

## Tests
- `NewGame/NewGameViewModelTests` (new; a draft over the test world `with` a new game built in the test: three leaders
  in an order that isn't id order, two items):
  - `Constructor_NewGame_ShowsLeadersInTheirOrder`; `Constructor_NewGame_ShowsItemsAndGold`.
  - `ToNewGame_LeaderMovedUp_WritesTheNewOrder`; `AddLeader_WithSelection_InsertsAfterIt`;
    `LeadersToAdd_LeavesListedMembersOut`.
  - `ToNewGame_TypedQuantity_SetsIt`; `Problem_QuantityNotANumber_NamesTheItem`.
  - `Problem_XNotFinite_NamesX` (`NaN`: refused, `.claude/rules/editor.md`); `ToNewGame_TypedStart_SetsMapCellFacing`.
  - `Problem_NoLeader_ShowsDataMessage`; `Problem_StartOutsideTheMap_ShowsDataMessage` (x past the map's width:
    `NewGameValidator`'s "must be inside map" line); `Problem_QuantityPastMaxStack_ShowsDataMessage`.
  - `Problem_TypedChange_RaisesProblemChanged`.
  - `ChosenKey_IsNull`; `Choose_AnyKey_ChangesNothing`.
- `Catalogs/CatalogDraftNewGameTests` (new): `SetNewGame_ChangedGold_RecordsOneStep`;
  `SetNewGame_SameNewGame_RecordsNothing` (lists rebuilt); `SetNewGame_Undo_RestoresIt`.
- `NewGame/NewGameEditingViewModelTests` (new): `Apply_EditedGold_ChangesTheDraft`.

## Verify by hand
None: the page comes in NG3.
