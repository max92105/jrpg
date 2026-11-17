# CL9b. A resource's strain and dominance as typed boxes

Editor, about 230 lines (about 130 of them tests). Split from CL9 on 2026-11-17; needs CL9's `Party/ResourceRuleBoxes`.
Design: section 3; decisions 951, 1016.

## Goal
The last two rules' boxes, on CL9's base: on or off as the record has them, the other resource named as typed (an id,
Data checking it names another resource of the class), the number box read only while on.

## New types
Each in `Party/`, a `ResourceRuleBoxes<TRule>` as CL9's two, with `From(X?)`, `CopyFrom` and its blank (decision 1016):
- `StrainBoxes`: one resource's strain (ADR 0024). `string LowersStartOf` (no `Problem` notice: Data checks it),
  `PerMissingPointText` ("Strain per missing point"); blank: no resource named, 1 per missing point.
- `DominanceBoxes`: one resource's dominance (decision 195). `string Over`, `BonusPercentText` ("Dominance bonus");
  blank: no rival named, a bonus of 0.

## Tests
- `Party/ResourceRuleTests` (CL9's file, 254 lines: past 300 with these, so a new `Party/ResourceRivalRuleTests`):
  `From_ResourceWithoutStrain_ShowsItOff`; `ToRule_StrainOn_WritesTargetAndAmount`;
  `ToRule_DominanceOn_WritesRivalAndBonus`; each untouched rule equal to its record; each ticked on writing its blank;
  each number box's problem name and `Problem` notice; `Changed_RivalTyped_IsRaisedOnce` (a field with no `Problem`
  notice); `CopyFrom` for each.

## Verify by hand
None: the resource writes the rules in CL9c, the tab shows them in CL9d.
