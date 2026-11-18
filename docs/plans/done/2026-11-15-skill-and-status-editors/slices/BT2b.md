# BT2b. The tier sheet rows and the Power boxes

Editor, about 160 lines (about 100 of them tests). Decisions 992, 993, 704. Design: design.md, sections 1
(`StartingPowers`) and 2.4. Split from BT2 at its start (estimate past 320 lines); builds on BT2's rows and
`Balance/BalanceTableViewModel`'s private `TypedBoxes`.

## Goal
The developer's tier sheet, headless: each `effects.json` record at three typed Powers with the typed modifier
(Power x `perPower` x (1 + modifiers), decision 704), through the same Core reading as the skills; the three Power
boxes and their problem line.

## New types
- `Editor/Balance/TierBalanceRow` (internal sealed record): one tier's id, kind line ("damage, fire") and `perPower`,
  its amounts at the three Powers, and whether a heal (the page mutes it, decision 994).
- `Editor/Balance/TierBalanceRows` (internal static): `Of(ContentCatalog content, IReadOnlyList<int> powers, int
  modifierPermille)`, the tiers in id order, each amount `EffectAmounts.PerHit` of an effect naming the tier over `new
  CombatStats(0) { Power = p, DamageModifierPermille = modifierPermille }`, so a heal ignores the modifier as the
  battle does.

## Changes
- `Editor/Balance/BalanceTableViewModel`: `FirstPowerText`, `SecondPowerText`, `ThirdPowerText` (`StartingPowers` 1,
  10, 50; whole numbers from 0 to a bound: `DamageFormula.MaxPower` is internal to Core, so either Core makes it public
  with a test, or the editor keeps a bound of its own, a decision from "Next free numbers"); `TypedBoxes` gains the
  Powers; `BoxesProblem` covers the Power boxes after the modifier; `TierRows` empty while a box can't be read; each
  box raises `TierRows` as it raises the other rows (the attribute lists then pass 130 columns: wrap them). Keep it
  under 300 lines (126 after BT2): a partial if it passes.
- `docs/map.md`: the Editor `Balance` line names the new types and the Power boxes.

## Tests
- `Editor.Tests/Balance/TierBalanceRowsTests` (new; over `BalanceFixtures.World` with its tiers):
  - `Of_SheetExample_PowerOneLowWithFourHundredSeventyThreePercent` (Power 1, `perPower` 2, modifier 473 %: 11, the
    developer's sheet, decision 704).
  - `Of_HealRecordWithAModifier_ModifierIgnored`; `Of_TwoTiers_InIdOrder` (the dictionary in the opposite order);
    `Of_DamageTier_KindLineNamesItsType`.
- `Editor.Tests/Balance/BalanceTableViewModelTests`: `New_Boxes_...` with the Powers 1, 10, 50;
  `TierRows_PowersTyped_ReadAtThosePowers`; `BoxesProblem_PowerNotANumber_SaysSo`; the Power bounds at each side (0
  and the max accepted, -1, max + 1 and a word refused), the refusals emptying every box-read list;
  `TypedBox_Changed_RaisesRowsAndProblem` over the three Power boxes, `TierRows` asserted for every box.
- **Done, with deviations:** `TierBalanceRow` 18 lines (`Amounts` as ints: a tier row has one hit, so no "x hits" text),
  `TierBalanceRows` 49 (kind line spelled as the Tier box spells it), `BalanceTableViewModel` 183 (Power boxes 1, 10,
  50 from 0 to `MaxTypedPower` 10000, decision 1022: an editor bound, Core's `DamageFormula` stays internal; the
  levels' and Powers' reads share one `ReadAll`). Tests (26 cases): `TierBalanceRowsTests` 63 (6, beyond the slice's
  4 a heal's kind line and three Powers in order), `BalanceTableViewModelTests` 285 (+20), `BalanceFixtures` 121
  (`fire_1`, `heal_1` in the world, heal first).

## Verify by hand
None: the page comes in BT3.
