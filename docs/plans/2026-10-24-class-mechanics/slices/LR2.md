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
