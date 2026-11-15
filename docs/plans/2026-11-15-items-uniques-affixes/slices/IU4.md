# IU4. A saved unique follows its record, the Forge never rerolls one, and a unique is named alone

Data + Core + Game, about 200 lines (about 130 of them tests), plus one `ui.json` line. Needs IU3. Design: design.md,
sections 2.2 and 2.7; decisions 868, 869, 871.

## Goal
The save repair reads a unique's record: a saved piece of a unique base takes the record's rarity and affixes (reported
only when they differed), its affixes never clamped to the pool's ranges; a saved `unique` piece whose base is no
longer unique becomes plain, reported. Shops' saved stocks follow the same rule (they share `RepairAffixes`). At the
Forge, Reroll on a unique is refused with "A unique's affixes are fixed", the one new refusal; Upgrade and Salvage stay
refused by decision 339's rules (Highest rarity; Nothing to salvage), now tested on a real unique. The Forge's and the
Shop's questions name a unique by its own name ("Chief's Cleaver", not "Unique Chief's Cleaver").

## Changes
- `Data/Saving/GearRepair.RepairAffixes`: first, a unique base returns `saved with { Rarity = Unique, Affixes = the
  record's }`, adding "{owner}: unique '{id}' follows its record now." when the saved piece differed; a saved `unique`
  rarity on a base that isn't unique returns a plain Common piece with no affixes, adding "{owner}: '{id}' is no longer
  a unique, so it became a plain piece."; every other piece is clamped as today. The class summary names both; split
  into `RepairUnique` and the clamp helper if the method passes 20 lines.
- `Core/Crafting/ForgeRefusal`: `FixedAffixes` ("a unique's affixes are fixed on its record and never rerolled,
  decision 699").
- `Core/Crafting/Forge.QuoteReroll`: `piece.Base.IsUnique` refuses with `FixedAffixes` before the other refusals.
- `Game/Crafting/ForgeRefusalNames`: the `FixedAffixes` arm; `Game/Ui/MessageIds.Crafting`:
  `ForgeRefusedFixedAffixes = "forge_refused_fixed_affixes"`; `content/text/ui.json`: "A unique's affixes are fixed".
- `Game/Ui/PieceName.Of`: a `Rarity.Unique` piece is its `Base.Name` (a unique's name is its own, decision 871); the
  summary says so.
- `GearRepair.cs` is 136 lines on 2026-11-15, about 160 after.

## Tests
- `Data.Tests/Saving/GearRepairTests` (104 lines; a catalog built in the test with one unique):
  - `RepairAffixes_UniqueBaseSavedWithOtherValues_TakesItsRecordsAndReports`.
  - `RepairAffixes_UniqueBaseSavedAsItsRecord_ReportsNothing`.
  - `RepairAffixes_UniqueValueAboveItsAffixsRollRange_IsNotClamped` (a record value of 9 on a 2..5 affix, decision
    866: the record wins and nothing is clamped).
  - `RepairAffixes_SavedUniqueOnAnOrdinaryBase_BecomesPlainAndReports`.
- `Data.Tests/Saving/ShopStockRepairTests` (76 lines): `Repair_StockedUniqueSavedWithOtherValues_TakesItsRecord`.
- `Core.Tests/Crafting/ForgeRerollTests` (88 lines): `QuoteReroll_UniquePiece_IsRefusedAsFixedAffixes`,
  `TryReroll_UniquePiece_ChangesNothingAndDrawsNothing` (no material taken; a `FixedSequenceRandom` with no value).
- `Core.Tests/Crafting/ForgeUpgradeTests`: `QuoteUpgrade_UniquePiece_IsRefusedAsHighestRarity` (today's rule, now with a
  real unique).
- `Core.Tests/Crafting/ForgeSalvageTests`: `QuoteSalvage_UniquePiece_IsRefusedAsNoYield`.
- `Game.Tests/Crafting/ForgeRefusalNamesTests` (19 lines): its every-refusal check covers the new arm; add
  `IdOf_FixedAffixes_IsItsOwnLine` if it has no such theory.
- `Game.Tests/Ui/PieceNameTests`: `Of_UniquePiece_IsItsOwnName` (a unique built in the test, with affixes).

## Verify by hand
- With IU3's hand-check edit (the Cleaver in the `goblin` table) made again: win it, then talk to a forge master (any
  map event with an Open Forge step), choose the Cleaver: Reroll is greyed with "A unique's affixes are fixed", Upgrade
  with "Highest rarity", Salvage with "Nothing to salvage". At a shopkeeper, Sell the Cleaver: the question names
  "Chief's Cleaver" with 220 gold. Revert the table.
- Save the game holding the Cleaver, change its `power` value to 4 in `content/items.json`, load the save: the Cleaver
  shows +4 Power. Revert.
