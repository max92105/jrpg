# IU2. A unique item gives its own piece everywhere: dropped, stocked, given, restored

Core, about 230 lines (about 160 of them tests). Needs ADR 0058. Design: design.md, sections 2.2 and 2.4; decisions
865, 867, 868, 870, 871, 875.

## Goal
An item record marked unique, with fixed affixes, is always the same piece: `Rarity.Unique` with those affixes in
order. A drop table or a shop listing it gives that piece drawing nothing beyond the entry's chance and quantity; a map
gift or a starting item gives it through `Inventory.Add`; a restored save rebuilds it from the record, whatever the save
held. Its price is its base price plus its affixes' gold values (no change to `Pricing`). A base item still never rolls
Unique (decision 875: `rules.json` has no unique row and `LootRulesValidator` refuses one; nothing changes there).
Nothing loads a unique from a file yet (IU3): the tests build their records.

## Changes
- `Core/Items/ItemDefinition`:
  - `public bool IsUnique { get; init; }`: "a hand-made piece (ADR 0058): never rolled, its affixes fixed on the
    record".
  - `public IReadOnlyList<RolledAffix> FixedAffixes { get; init; } = [];`: "a unique's affixes in order, each value
    fixed (decision 699), each stat resolved from the pool at load; empty on every other item".
  - Equality: an item compares its fixed affixes in order, every other member as the record does (as `DropTable`,
    decision 604), with a matching `GetHashCode`; the summary says so.
- `Core/Items/RolledAffix`: the summary says a line is rolled, or fixed on a unique.
- `Core/Items/GearPiece`:
  - `public static GearPiece Unique(ItemDefinition item)`: "the piece a unique item always is: Unique with its fixed
    affixes in order (ADR 0058)"; `ArgumentException` naming the item when it isn't unique, or isn't equipment.
  - `public static GearPiece Unrolled(ItemDefinition item) => item.IsUnique ? Unique(item) : Plain(item);`: "the piece an
    item gives when nothing rolls: a unique's own, else a plain one (decision 344)".
  - `Plain` and `Rolled` unchanged.
- `Core/Items/Inventory.Add`: equipment becomes `GearPiece.Unrolled(item)` copies; the summary names uniques.
- `Core/Loot/GearRoll.Roll`: a unique item returns `GearPiece.Unique(item)` first, drawing nothing; the class summary
  adds it (decision 867).
- `Core/Loot/DropRoll`: the summary says a unique's copies come as its own piece.
- `Core/Content/ContentCatalog.PieceFrom`: a saved piece whose base is unique is `GearPiece.Unique(item)`, the saved
  rarity and affixes ignored (decision 868); the remark says why.

## Tests
- `Core.Tests/Items/GearPieceTests` (111 lines; records built in the test with `with` over `ItemDefinitionBuilder`'s):
  - `Unique_UniqueItem_IsUniqueWithItsFixedAffixesInOrder`.
  - `Unique_ItemNotMarkedUnique_Throws`.
  - `Unrolled_UniqueItem_IsItsUniquePiece`.
  - `Unrolled_OrdinaryItem_IsPlain`.
  - `Bonuses_UniquePiece_AddsItsFixedAffixesToTheBase`.
- `Core.Tests/Items/ItemDefinitionTests` (new; records built in the test):
  `Equals_SameFixedAffixesInOrder_AreEqual`, `Equals_FixedAffixesInAnotherOrder_AreNotEqual`.
- `Core.Tests/Items/InventoryGearTests` (100 lines): `Add_UniqueItem_AddsItsUniquePiecePerCopy`.
- `Core.Tests/Loot/GearRollTests` (156 lines): `Roll_UniqueItem_ReturnsItsPieceAndDrawsNothing` (a
  `FixedSequenceRandom` with no value: any draw throws).
- `Core.Tests/Loot/DropRollTests` (109 lines): `Roll_TableListingAUnique_DropsItsUniquePieceAfterTheChanceDraw` (the
  sequence holds the chance draw only).
- `Core.Tests/Session/ShopStocksTests` (84 lines): `ShopFor_ShopListingAUnique_StocksItsUniquePiece`.
- `Core.Tests/Trade/PricingTests` (58 lines): `PriceOf_UniquePiece_IsItsBaseAndFixedAffixValues`.
- `Core.Tests/Session/GameSessionUniqueTests` (new; a catalog built in the test with one unique, `GameSessionSaveTests`
  is at 270):
  - `Restore_SavedUniquePiece_IsRebuiltFromItsRecord` (the record's value differs from the saved one: the record's
    wins).
  - `Save_ThenRestore_UniquePieceComesBackEqual` (worn and owned).
  - `Restore_SavedUniqueWorn_KeepsItsSlot`.

## Verify by hand
- Nothing to see yet: no content holds a unique (IU3 adds the placeholder). Run the game: it starts and plays as before.
