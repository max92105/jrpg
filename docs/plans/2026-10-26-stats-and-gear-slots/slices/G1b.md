# G1b. Saves keep all nine slots

Part of [the stats and gear slots plan](../README.md). Decision 298, ADR 0022 (bump and refuse until the first release), ADR 0028. Design: "Content shapes" (saves). After G1a-2; closes its known gap.

- **G1b. Saves keep all nine slots: format 4** (Core + Data, about 200 lines)
  - **Goal:** a save keeps what each member wears in every slot, keyed by slot (`"worn": { "weapon": "bronze_sword", "firstRing": "power_ring" }`), and a format 3 save is refused as made by an older version. The developer's test saves stop loading (ADR 0022).
  - **Changes:**
    - `Party/PartyMemberProgress`: `WeaponId` and `ArmorId` → `Worn` (`IReadOnlyDictionary<EquipmentSlot, string>`, empty slots absent); `Equals` and `GetHashCode` compare it by content, as `LastingAmounts`.
    - `Party/PartyMember.CaptureProgress` writes every worn slot; `Session/GameSession.RestoreMember` wears each (before `RestoreProgress`, P4).
    - `Data/Saving/SaveFile.CurrentFormatVersion` 3 → 4; the slot keys in camelCase under `SaveJsonOptions.Strict` (an unknown slot name makes the save unreadable, like any unknown field).
    - `Data/Saving/SaveGameRepair`: per slot, an unknown item is dropped, an item whose kind no longer fits its slot goes back to the inventory (today's rule, now per slot).
    - Test support: `PartyMemberBuilder` and the tests building a `PartyMemberProgress` (about 15 `WeaponId`/`ArmorId` sites in 5 files).
  - **Tests:**
    - `FileSaveStoreTests: SaveThenLoad_RingsInBothSlots_RoundTrips`
    - `FileSaveStoreTests: Load_FormatThree_IsRefusedAsOlder`
    - `SaveGameRepairWornTests: Repair_UnknownItemInRingSlot_DropsIt` (new file: `SaveGameRepairTests` is at 266)
    - `SaveGameRepairWornTests: Repair_HelmSavedInChest_ReturnsItToInventory`
    - `GameSessionSaveTests: CaptureThenRestore_NineSlots_WearsTheSame`
    - `PartyMemberProgressTests: Equals_SameWornInOtherOrder_IsTrue`
  - **Call sites:** `WeaponId`/`ArmorId` 15 in 5 test files, 6 in source; the existing format-3 round trip tests move to format 4.
  - **Watch:** `FileSaveStoreTests` (251 + 2 tests), `GameSessionSaveTests` (272 + 1: if it passes 280, the new test goes in `GameSessionGearTests`).
  - **Verify by hand:** delete the old test saves (refused with "made by an older version"); save with a member wearing a sword and chain mail, quit, load: both are still worn, and the save file shows a `worn` map. Rings by hand come with G2 (checked in Z).
- **Done, with deviations:**
  - **`PartyMemberProgress.Worn`** is an `IReadOnlyDictionary<EquipmentSlot, string>` compared entry by entry, like `LastingAmounts`, through one private `SameEntries`; `WornEquipment.ItemIds` feeds it from the member. The slot keys come out in camelCase through the enum converter's own naming policy, which covers dictionary keys both ways (the round trip test pins `"firstRing"` in the file); no key policy or custom converter was needed, and `LastingAmounts`' string keys stay as written.
  - **`SaveGameRepair.RepairWorn`** repairs slot by slot through the existing `RepairWornItem`, which now takes a non-null id; the restored sheet's bonuses are summed from the kept ids in `RepairMember` (three lines), the second sum the P4b review watched: `WornEquipment` is Core-internal and holds items, not ids, so the repair keeps its own.
  - **Test support:** the 11 `WeaponId`/`ArmorId` constructions in 8 files became `Worn` dictionaries; `SaveGameRepairTests` gained a `Wearing((slot, id), …)` helper (281 → 285 lines) and its three worn tests read the slot. `FileSaveStoreTests`' format tests moved to 4 (an older save is now format 3, a newer one 5); 251 → 274 lines.
  - **Tests:** `FileSaveStoreTests` 1 new, the older-format test re-aimed; `SaveGameRepairWornTests` 3 (the two planned plus a ring in both slots kept, on a catalog copy with a ring and a helm, which content lacks until G2); `GameSessionGearTests.CaptureThenRestore_NineSlots_WearsTheSame` (`GameSessionSaveTests` is at 270); `PartyMemberProgressTests` 2; the review added `Read_NullWornItem_ThrowsUnreadable` (a null in a worn slot is refused as a damaged file, not an `ArgumentNullException` in the repair) and `Read_UnknownSlotName_ThrowsUnreadable`. Count: 2,559 → 2,568.
  - **Hand check (yours):** your old test saves are refused with "made by an older version (format 3)": delete them; save with a member wearing a sword and chain mail, quit, load: both are still worn, and the save file shows a `worn` map with `weapon` and `chest`. Rings by hand come with G2 (checked in Z).
