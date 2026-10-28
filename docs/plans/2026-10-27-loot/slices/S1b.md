# S1b. A loaded piece's affixes are repaired against the pool

Part of [the loot plan](../README.md). ADR 0029, ADR 0011 (content is the source of truth), decision 360. After R3a (the pool) and S1a (`GearRepair`).

- **S1b. A loaded piece's affixes are repaired against the pool** (Data, about 130 lines)
  - **Goal:** content changes to the pool reach saved pieces on load: an affix id the pool no longer has is dropped, and a value outside the affix's current range is clamped into it; each is a line in `RepairedSave.Changes` (decision 360: never silent). A saved affix has no stat to repair: `FromSaved` reads it from the pool (S1a). A piece's rarity and affix count aren't checked against the table (a piece rolled under older numbers stays as rolled).
  - **Changes:** `Data/Saving/GearRepair` repairs each piece's affixes, worn and owned, with the catalog's `Affixes`.
  - **Tests** (`GearRepairAffixTests`):
    - `Repair_UnknownAffix_DropsItAndReports`
    - `Repair_ValueAboveRange_ClampsToMaxAndReports`
    - `Repair_ValueBelowRange_ClampsToMinAndReports`
    - `Repair_PieceThatFits_UnchangedAndNotReported`
  - **Call sites:** none outside `GearRepair`.
