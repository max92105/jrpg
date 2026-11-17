# CL9. A resource's rules: Will scaling, backfire, strain and dominance

Editor, about 210 lines (about 100 of them tests, about 50 of XAML). Needs CL8. Design: section 3; decision 951.

## Goal
The last part of the Resources tab: four optional rules, each a box that turns it on with its numbers, so no resource
needs `classes.json` by hand:
- **Will scaling:** Will per point, and what it does (raise the start, raise the max, lower the gains: the
  `WillEffect` values, from Core's enum).
- **Backfire:** chance per point (%), damage (% of max HP).
- **Strain:** the resource whose start it lowers (the class's other resources as typed, by id) and per missing point.
- **Dominance:** the rival force (the class's other resources as typed) and its bonus (%).
Off, a rule writes nothing. Data's rules stay Data's: strain only on a lasting resource and never onto one; both forces
naming each other; Will's raise-start never on a lasting resource; the percent ranges.

## New types
Each in `Party/`, each an `ObservableObject` with `[ObservableProperty] bool IsOn`, its boxes, `string? Problem` (its
boxes, only while on, named "Backfire chance per point must be a whole number"), `X? ToRule()` (null while off; throws
on a problem), `From(X?)` and `CopyFrom`:
- `WillScalingBoxes`: one resource's Will scaling, on or off, as typed. `WillPerPointText`, `WillEffect Effect`.
- `BackfireBoxes`: one resource's backfire, on or off, as typed. `ChancePerPointText`, `DamagePercentText`.
- `StrainBoxes`: one resource's strain, on or off, as typed. `string LowersStartOf`, `PerMissingPointText`.
- `DominanceBoxes`: one resource's dominance, on or off, as typed. `string Over`, `BonusPercentText`.

## Changes
- `Party/ClassResourceViewModel` (about 150 lines after CL7, about 190 after): the four boxes as properties built from
  the record; `ToResource` writes `WillScaling`, `Backfire`, `Strain`, `Dominance` from them (no longer carried);
  `Problem` adds them in that order; a rule's change raises `Problem`.
- `Party/ClassViewModel.Resources.cs`: `IReadOnlyList<string> ResourceIds`, the class's resources' ids as typed, raised
  when a resource's id or the list changes: the strain and dominance boxes offer them (the resource's own id is
  Data's message if picked: "must name another resource").
- `Party/ClassCatalogPage.xaml`: a Rules group under Gains: four check boxes, each enabling its row of boxes; the Will
  effect combo over `Enum.GetValues<WillEffect>()` (Core's list, ADR 0045's rule); the two id combos editable over
  `ResourceIds`.

## Tests
- `Party/ResourceRuleTests` (new; resources from `ClassFixtures`, each rule's numbers distinct):
  - `From_ResourceWithBackfire_ShowsItOn` and `From_ResourceWithoutStrain_ShowsItOff`.
  - `ToRule_Off_IsNull` (backfire boxes holding numbers, unticked: nothing written).
  - `ToRule_WillScalingOn_WritesPerPointAndEffect` (effect `LowerGain`, not the default).
  - `ToRule_StrainOn_WritesTargetAndAmount`; `ToRule_DominanceOn_WritesRivalAndBonus`.
  - `Problem_OffRuleWithBadBox_IsNull` and `Problem_OnRuleWithBadBox_NamesIt` (both sides).
- `Party/ClassResourceListTests` (CL7's file): `ResourceIds_IdTyped_RaisesChanged`;
  `Problem_StrainOnNonLastingResource_ShowsDataMessage`; `Problem_DominanceOneSided_ShowsDataMessage`;
  `ToClass_UntouchedClass_LoadsTheSame` (a class with all four rules through `From` then `ToClass`: equal to the record
  by content, the round trip the page relies on).

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
2. Database > Classes, `druid`, Resources: Growth's Dominance is ticked, over `decay`, 50; Decay's names `growth`.
   `warlock`'s Instability has Backfire 5 and 10 and Will scaling "lower gain". `mage`'s Clarity has Strain on `mana`, 1.
3. Untick Decay's dominance: the line gives Data's "doesn't name ... back" message and Apply greys. Tick it again: clear.
4. Apply without changes, Save all: `git diff content/classes.json` is empty (every rule wrote back as read).
5. On `fighter`'s Stamina tick Backfire, type 10 and 20, Apply, Save all: `classes.json`'s Stamina gains
   `"backfire": { "chancePerPointPercent": 10, "damagePercentOfMaxHp": 20 }` (one field per line) and the game starts.
   Ctrl+Z, Save all to restore.

- **Note from CL7:** CL7 was split in three; `Party/ClassResourceViewModel` comes from CL7b and
  `Party/ClassResourceListTests` from CL7c (the "after CL7" and "CL7's file" above mean those).
- **Done, with deviations:** split on 2026-11-17, the whole slice estimated at about 450 lines with tests: CL9 built
  the shared base and the first two rules' boxes; strain and dominance moved to CL9b, the resource writing the four
  rules (`ClassResourceViewModel`, the list's Data-message and round-trip tests) to CL9c, `ResourceIds` and the Rules
  group to CL9d, which says the group goes in CL8's `Party/ClassResourcesView.xaml`, not the page. Decision 1016: the
  four boxes share `Party/ResourceRuleBoxes<TRule>` (46 lines: `IsOn`, `Problem` only while on, `ToRule` null while
  off and throwing on a problem, `Changed` raised through `OnTyped` from each typed field's change hook, after review,
  so a later computed property never raises it), and an off rule's boxes show numbers Data takes on any resource (Will
  per point 1 raising the max, backfire 1 and 1). `Party/WillScalingBoxes` (52) and `Party/BackfireBoxes` (56), each
  with `From` and `CopyFrom`, the record or the blank read once in the constructor. `Party/ResourceRuleTests` (270, 26
  tests: shown on and off, untouched equal, ticked on writing the blank, typed rules written, off writing nothing, the
  throw, each box's problem name, the problem only while on, each `Problem` notice, `Changed` once for every typed
  field and the tick, `CopyFrom` on and off). About 420 lines, above the 210 planned. No hand check: no page change.
