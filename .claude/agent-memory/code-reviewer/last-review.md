---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/P4a.md, slice P4a (StatKind, StatKinds, StatBonuses, PrimaryStats/StatSheet.ValueOf, EquipmentStats.Bonuses, Data StatBonusesConverter, validator bonus bounds, items.json bonuses)
Round: 1 (verdict: Ready to commit)

- W1 src/Jrpg.Core/Stats/StatBonuses.cs:4 summary says accuracy is whole points; design.md:118 and StatSheet (AccuracyPermille) make it a percent, so P4b's x10 rule would read gloves' +5 as 0.5 %; open
- S1 tests/Jrpg.Core.Tests/Stats/StatSheetTests.cs:49 PrimaryValueOf_NotAPrimary_Throws tests PrimaryStats.ValueOf from StatSheetTests; move to PrimaryStatsTests.ValueOf_NotAPrimary_Throws; open (suggestion)
- S2 src/Jrpg.Core/Stats/StatBonuses.cs:20 Values hands out the inner Dictionary (castable, shared by static None); store a FrozenDictionary; open (suggestion)
- S3 src/Jrpg.Data/Items/StatBonusesConverter.cs:48 Write has no caller and no test (no writer serializes items); open (suggestion)
- F1 (carried) design.md:105 lists DamagePercent, StatusPercent, PhysicalResistance, MagicalResistance; follow-up
- F2 (carried) src/Jrpg.Game/NewGame/LeaderProfile.cs second max-HP path beside MemberSheet; follow-up
