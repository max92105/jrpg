# S1a. Saves keep every piece with its rolls: format 5

Part of [the loot plan](../README.md). ADR 0029, ADR 0022 (bump and refuse until the first release). Design: "Gear pieces" (saves), "Content shapes". Right after G1b-2 (closes G1b-1's save gap) and after C3.

- **S1a. Saves keep every piece with its rolls: format 5** (Core + Data, about 260 lines with the affix pool's record; the move of the gear repair into `GearRepair` is mechanical and goes first, as its own commit, if the slice heads past 280)
  - **Goal:** a save keeps each worn and owned piece with its base, rarity and affixes (id and value), in order; a format 4 save is refused as older. On load each affix's stat comes from the affix pool. The repair keeps today's rules per piece (unknown base dropped, a worn piece whose kind no longer fits its slot goes back to the owned gear); affix repair is S1b.
  - **New types:**
    - `Items/SavedAffix(string AffixId, int Value)`: an affix as a save keeps it; its stat comes from content.
    - `Items/SavedGear(string ItemId, Rarity Rarity, IReadOnlyList<SavedAffix> Affixes)`: a piece as a save keeps it; equal by content.
    - `Loot/AffixDefinition`: one affix the pool may roll: `Id`, `Stat` (`StatKind`), `Min`, `Max`, `Kinds` (`IReadOnlyList<EquipmentKind>`), `Weight`. Its first reader is the load (the stat); R2 rolls it.
    - `Data/Saving/GearRepair`: repairs saved pieces against content, worn by slot and owned; the worn and gear parts of `SaveGameRepair` (`RepairWorn`, `RepairWornItem`, `BonusesOf`, the gear half of `RepairInventory`) move here, so `SaveGameRepair` (255) shrinks.
  - **Changes:**
    - `Items/GearPiece`: `ToSaved()`; `static GearPiece FromSaved(SavedGear saved, IReadOnlyDictionary<string, ItemDefinition> items, IReadOnlyDictionary<string, AffixDefinition> affixes)` (assumes a repaired save, as `GameSession.Restore` does).
    - `Content/ContentCatalog.Affixes` (`IReadOnlyDictionary<string, AffixDefinition>`, required): `ContentCatalogBuilder` defaults to empty; `ContentCatalogLoader` sets it empty until R3a reads `affixes.json`.
    - `Party/PartyMemberProgress.Worn`: `IReadOnlyDictionary<EquipmentSlot, SavedGear>`; `WornEquipment.ItemIds` becomes `Saved`.
    - `Saving/SaveGame.Gear` (`IReadOnlyList<SavedGear>`, required); `Inventory` holds stacks only.
    - `Session/GameSession`: `Capture` writes `Gear`; `Restore` adds each saved piece and wears each worn one through `FromSaved`.
    - `Data/Saving/SaveFile.CurrentFormatVersion` 4 → 5; `FileSaveStore.CheckNoNullEntries` covers `Gear`, each piece's `Affixes` and each worn value.
  - **Tests:**
    - `FileSaveStoreGearTests: SaveThenLoad_RolledPieceWorn_RoundTrips` (pins `"rarity": "rare"` and `{ "affixId": …, "value": … }` in the file, no stat)
    - `FileSaveStoreGearTests: SaveThenLoad_OwnedPiecesInOrder_RoundTrips`
    - `FileSaveStoreFormatTests: Read_FormatFour_ThrowsOlder` (the older-format test re-aimed)
    - `FileSaveStoreFormatTests: Read_NullGearEntry_ThrowsUnreadable`
    - `FileSaveStoreFormatTests: Read_NullAffix_ThrowsUnreadable`
    - `GearPieceTests: FromSaved_Affix_TakesItsStatFromThePool`
    - `GearRepairTests: Repair_UnknownBase_DropsPiece`
    - `GearRepairTests: Repair_WornKindNoLongerFits_MovesToGear`
    - `GameSessionGearTests: CaptureThenRestore_RolledPieces_SameRollsAndOrder`
    - `PartyMemberProgressTests: Equals_SameSavedGearInOtherOrder_IsTrue`
  - **Call sites:** `PartyMemberProgress` `Worn` dictionaries in about 11 test constructions over 8 files; gear entries in save tests' `Inventory` lists move to `Gear`; the format tests move to 5 (older 4, newer 6).
  - **Watch:** `GameSessionSaveTests` 270 (new tests in `GameSessionGearTests`); `SaveGameRepair.cs` must end below 230.
  - **Verify by hand:** delete the old test saves (refused with "made by an older version (format 4)"); save with a sword worn, a staff owned and potions, quit, load: all are back, and the file shows `worn.weapon` as an object with `itemId`, `rarity` and `affixes`, and a `gear` list.
