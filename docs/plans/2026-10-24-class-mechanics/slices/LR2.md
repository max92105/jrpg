# LR2. Saves keep lasting resources

Part of [the class mechanics plan](../README.md). Design: [design.md](../design.md), "Risks" (save format). ADR 0024, ADR 0022, ADR 0007.

- **LR2. Saves keep lasting resources: format 3** (Core + Data, about 220 lines)
  - **Goal:**
    - A save keeps each member's lasting amounts. A format 2 save is refused as older (decision 229, ADR 0022); `Restore` gives a member missing an id its starting amount.
    - Repair drops ids the class no longer has as lasting and clamps the rest to 0..max, naming what it changed.
  - **Changes:**
    - Core:
      - `Party/PartyMemberProgress` gains `IReadOnlyDictionary<string, int> LastingAmounts`;
      - `CaptureProgress` writes it;
      - `PartyMember.Restore` reads it, filling a missing id with the starting amount.
    - Data:
      - `Saving/SaveFile.CurrentFormatVersion` = 3;
      - `SaveGameRepair` repairs the amounts.
    - Tests: the `new PartyMemberProgress(` sites (about 5) gain the argument.
  - **Tests:**
    - `FileSaveStoreTests`:
      - `Load_FormatTwoSave_IsRefusedAsOlder`
      - `Save_Always_WritesFormatThree`
    - `GameSessionSaveTests`: `Capture_Restore_KeepsLastingAmounts` (round trip)
    - `SaveGameRepairTests`:
      - `Repair_UnknownLastingId_DroppedAndReported`
      - `Repair_LastingAboveMax_Clamped`
  - **Verify by hand:** a save made after this slice keeps the lasting amounts; one made before it is refused as older.
- **Done, with deviations:**
  - Test names follow the existing ones: `Read_FormatTwoSave_ThrowsAsOlder` replaces the format-one case, `Write_File_HoldsTheFormatThreeEnvelope`, and `CaptureThenRestore_KeepsLastingAmounts`. The newer-format test moved to format 4.
  - `PartyMemberProgress` compares its lasting amounts by value, since a record compares a dictionary by reference, which would break every test comparing members.
  - Extra tests: `Restore_LastingAmountMissing_StartsAtStartsAt` and a file round trip with a lasting amount, `Read_AfterWrite_KeepsLastingAmounts`. The repository has no lasting resource until LR3, so the clamp test and the file test use a copy of the catalog in which Sarxian's class has one.
  - After review: the clamp test is a theory over both bounds (`Repair_LastingOutOfRange_Clamped`), and the drop message says "no longer has as lasting".
