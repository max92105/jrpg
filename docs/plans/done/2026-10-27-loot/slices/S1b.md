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
- **Note from S1a:** the unknown-affix drop is done (`GearRepair.WithKnownAffixes`, tested by `GearRepairTests.Repair_AffixNoLongerInThePool_DropsItAndReports` and `Repair_WornAffixRaisingMaxHp_KeepsHpAndDropsTheUnknownAffix`), so `Repair_UnknownAffix_DropsItAndReports` is not needed here; what is left is the range clamp and the unchanged case. The stat is read by `ContentCatalog.PieceFrom`, not `GearPiece.FromSaved`.
- **Done, with deviations:** `GearRepair.WithKnownAffixes` (S1a) became `RepairAffixes`, which also clamps each value into its affix's current range and reports it (decision 360). The tests are in `GearRepairTests` beside S1a's (104 lines), not a new `GearRepairAffixTests`; the unknown-affix case was already there (S1a). Tests 2695 → 2698.
