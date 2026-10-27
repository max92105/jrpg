---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/E1.md, slice E1 (Equipment page Changes section: StatComparison, StatComparisonText, EquipmentComparison; PartyMember SheetWith/SheetWithout; decision 337 keeps Attack/Defense lines)
Round: 1 (verdict: Ready to commit; verify all pass, 2417 tests)

- W1 src/Jrpg.Game/Menus/StatComparison.cs:21-24 lines kept when permille differs but the written whole percent doesn't (wizard robe, Will 8: Light/Dark resistance "2 % > 2 %" in rise color) -> compare what's written, or show a decimal (developer's call); open
- W2 tests/Jrpg.Game.Tests/Menus/EquipmentChoiceTests.cs Comparison_* rebuild ShowChoice's pairing by hand; no test highlights the choice's Remove row through the page -> page test: sword worn, Confirm on weapon row, Attack After < Before; open
- S1 src/Jrpg.Core/Party/PartyMember.cs:217,237 sheet built in RebuildStats and SheetWearing -> Sheet = SheetWearing(new Worn(Weapon, Armor)), drop WornBonuses; open (suggestion)
- S2 src/Jrpg.Game/Menus/EquipmentSheetView.cs:32 ChangeLinesPerColumn (4) and StatComparisonText.MostLines (8) must agree -> derive one from the other; open (suggestion)
- S3 src/Jrpg.Game/Menus/StatComparison.cs:43 Change.Before doc says StatSheet.ValueOf, but Attack/Defense come from CombatStats; open (suggestion)
