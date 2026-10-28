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
- **Note from G1b-1 review:** also drop `SaveGameRepair.ClampToMaxStack` on gear counts and `NewGameValidator`'s max-stack check on starting equipment here, and `GameSession.OwnedGearByBase` (the interim save bridge).
- **Done, with deviations:**
  - The move of the worn-gear repair into `Data/Saving/GearRepair` went first as its own commit (`SaveGameRepair` 255 → 219 → 221).
  - **`ContentCatalog.PieceFrom(saved)` instead of `GearPiece.FromSaved(saved, items, affixes)`:** the plan's signature would make `Items` reference `Loot` while `Loot` references `Items` (design.md's folder table). `ContentCatalog` already references both and has `ClassOf`; the planned `GearPieceTests.FromSaved_Affix_TakesItsStatFromThePool` is `ContentCatalogTests.PieceFrom_Affix_TakesItsStatFromThePool`.
  - **Pulled forward from S1b:** a saved affix the pool no longer has is dropped and reported (`GearRepairTests.Repair_AffixNoLongerInThePool_DropsItAndReports`), so such a save can't crash the load before S1b; S1b keeps the range clamp.
  - A piece whose base is no longer equipment goes back to the stacks as one copy, worn or owned (`Repair_OwnedBaseNoLongerEquipment_GoesBackToTheStacks`); a worn piece whose kind no longer fits joins the owned gear last.
  - `Read_NullGearEntry_ThrowsUnreadable` and `Read_NullAffix_ThrowsUnreadable` are three new rows of the null-entry theory (`gear`, `ownedAffixes`, `wornAffixes`). `GameSession.OwnedGearByBase` (G1b-1's bridge) is gone. `SaveGameRepair.ClampToMaxStack` and `NewGameValidator`'s cap stay: format 5's inventory holds no gear, and the backlog line on equipment's `maxStack` covers the validator.
  - The review added `GearRepairTests.Repair_WornAffixRaisingMaxHp_KeepsHpAndDropsTheUnknownAffix` (a worn affix raising max HP keeps the saved HP; a worn unknown affix is dropped) and split `GearRepair`'s piece check into its worn and owned paths. S1b's file and design.md say what moved. Tests 2603 → 2617.
