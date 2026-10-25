# MP3a. Saves stop keeping MP: format 2 and the first migration

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Risks" (save migrations). Decision 197; ADR 0022, ADR 0007.

- **MP3a. Saves stop keeping MP: format 2 and the first migration** (Core + Data, about 230 lines)
  - **Goal:**
    - Saves no longer hold MP. A format 1 save loads through a migration that drops each member's `mp`.
    - A member restored from a save starts with full MP: it is still in `Vitals` until MP3b, and nothing uses it.
  - **New types:**
    - Data `Saving/SaveMigrations` (internal static): brings a save's JSON tree up to the current format, one version step at a time: `Upgrade(JsonObject root, int fromVersion)`. Each step is a private method named after its versions (`FromVersion1`).
  - **Changes:**
    - Core:
      - `Party/PartyMemberProgress` loses `Mp`;
      - `PartyMember.CaptureProgress` follows;
      - `Restore` sets MP to max, with a comment pointing to ADR 0022.
    - Data:
      - `Saving/SaveFile.CurrentFormatVersion` = 2;
      - `FileSaveStore`: after the version check, a file below 2 goes through `SaveMigrations.Upgrade` before deserializing, and a newer one is still rejected;
      - `SaveGameRepair` loses the MP clamp and its change line.
    - Tests: `FileSaveStoreTests` and `GameSessionSaveTests` lose `Mp` from their progress records; `SaveGameRepairTests` loses its MP cases.
  - **Tests:**
    - `SaveMigrationsTests`:
      - `Upgrade_FromVersion1_DropsMemberMp`
      - `Upgrade_FromVersion1_KeepsEverythingElse`
    - `FileSaveStoreTests`:
      - `Load_FormatOneSave_Loads` (a format 1 file written in the test)
      - `Save_Always_WritesFormatTwo`
      - `Load_FormatThreeSave_IsRejected`
    - `GameSessionSaveTests`: `Capture_Restore_RoundTripsWithoutMp`
  - **Verify by hand:** continue a save made before this slice: it loads, and HP, level and equipment are kept.
