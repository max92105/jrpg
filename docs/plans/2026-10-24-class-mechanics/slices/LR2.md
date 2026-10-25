# LR2. Saves keep lasting resources

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Risks" (save migrations). ADR 0024, ADR 0022, ADR 0007.

- **LR2. Saves keep lasting resources: format 3** (Core + Data, about 220 lines)
  - **Goal:**
    - A save keeps each member's lasting amounts. A format 2 save loads through a migration: its members get none, so `Restore` gives them their starting amounts.
    - Repair drops ids the class no longer has as lasting and clamps the rest to 0..max, naming what it changed.
  - **Changes:**
    - Core:
      - `Party/PartyMemberProgress` gains `IReadOnlyDictionary<string, int> LastingAmounts`;
      - `CaptureProgress` writes it;
      - `PartyMember.Restore` reads it, filling a missing id with the starting amount.
    - Data:
      - `Saving/SaveFile.CurrentFormatVersion` = 3;
      - `SaveMigrations.FromVersion2` adds an empty `lastingAmounts` to each member;
      - `SaveGameRepair` repairs the amounts.
    - Tests: the `new PartyMemberProgress(` sites (about 5) gain the argument.
  - **Tests:**
    - `SaveMigrationsTests`:
      - `Upgrade_FromVersion2_AddsEmptyLastingAmounts`
      - `Upgrade_FromVersion1_RunsBothSteps`
    - `FileSaveStoreTests`:
      - `Load_FormatTwoSave_Loads`
      - `Save_Always_WritesFormatThree`
    - `GameSessionSaveTests`: `Capture_Restore_KeepsLastingAmounts` (round trip)
    - `SaveGameRepairTests`:
      - `Repair_UnknownLastingId_DroppedAndReported`
      - `Repair_LastingAboveMax_Clamped`
  - **Verify by hand:** continue a save made before this slice: it loads.
