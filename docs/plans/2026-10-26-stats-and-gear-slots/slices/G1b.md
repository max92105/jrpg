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
