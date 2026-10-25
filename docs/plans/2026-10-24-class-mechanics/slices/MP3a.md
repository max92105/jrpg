# MP3a. Saves stop keeping MP: format 2

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Risks" (save format). Decisions 197 and 229; ADR 0022, ADR 0007.

- **MP3a. Saves stop keeping MP: format 2, older saves refused** (Core + Data, about 150 lines)
  - **Goal:**
    - Saves no longer hold MP.
    - A format 1 save is refused as "made by an older version of the game". There is no migration before the first release (decision 229, ADR 0022).
    - A member restored from a save starts with full MP: it is still in `Vitals` until MP3b, and nothing uses it.
  - **Changes:**
    - Core:
      - `Party/PartyMemberProgress` loses `Mp`;
      - `PartyMember.CaptureProgress` follows;
      - `Restore` sets MP to max, with a comment pointing to ADR 0022.
    - Data:
      - `Saving/SaveFile.CurrentFormatVersion` = 2;
      - `FileSaveStore.CheckFormatVersion` refuses a version below the current one, with its own reason; a newer one is still refused;
      - `SaveGameRepair` loses the MP clamp and its change line.
    - Tests: `FileSaveStoreTests` and `GameSessionSaveTests` lose `Mp` from their progress records; `SaveGameRepairTests` loses its MP cases.
  - **Tests:**
    - `FileSaveStoreTests`:
      - `Load_FormatOneSave_IsRefusedAsOlder` (a format 1 file written in the test)
      - `Save_Always_WritesFormatTwo`
      - `Load_FormatThreeSave_IsRefusedAsNewer`
    - `GameSessionSaveTests`: `Capture_Restore_RoundTripsWithoutMp`
  - **Verify by hand:** a new game saved after this slice continues, keeping HP, level and equipment. (The developer's format 1 test save was deleted on 2026-10-24.)
