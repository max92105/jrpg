# CL9c. A resource writes its four rules from the boxes

Editor, about 200 lines (about 120 of them tests). Split from CL9 on 2026-11-17; needs CL9b's `StrainBoxes` and
`DominanceBoxes`. Design: section 3; decision 951.

## Goal
The resource's rules come from the boxes, no longer carried from the record: off writes nothing, on writes the rule as
typed; the boxes' problem joins the resource's, and Data's rules (strain only on a lasting resource and never onto one,
both forces naming each other, Will's raise-start never on a lasting resource, the percent ranges) reach the class's
line through `CatalogProblems.OfClasses`, unchanged.

## Changes
- `Party/ClassResourceViewModel` (178 lines): `WillScaling`, `Backfire`, `Strain`, `Dominance`, the four boxes built
  from the record; `ToResource` writes the four rules from `ToRule()`; the boxes' problem after the gains', in that
  order; each rule's `Changed` raises the resource's `Changed`; `DuplicateOf` copies them as typed (`CopyFrom`). The
  class summary no longer says the rules are carried.
- `ClassResourceViewModelTests`' `ToResource_UntouchedResource_KeepsItsRules` comment and `ClassFixtures.Surge`'s
  summary no longer say no box edits the rules.

## Tests
- `Party/ClassResourceViewModelTests` (293 lines: the new tests go to a new `Party/ClassResourceRuleTests`):
  `From_ResourceWithBackfire_ShowsItOn`; `ToResource_BackfireUnticked_WritesNone`;
  `ToResource_TypedRules_WritesEach` (the four); `Problem_GainAndRuleNotNumbers_NamesTheGainFirst`;
  `Changed_RuleTyped_IsRaisedOnce`; `DuplicateOf_HalfTypedRules_CopiesThem` (each of the four).
- `Party/ClassResourceListTests` (CL7c's file): `Problem_StrainOnNonLastingResource_ShowsDataMessage`;
  `Problem_StrainOntoLastingResource_ShowsDataMessage`; `Problem_DominanceOneSided_ShowsDataMessage`;
  `ToClass_UntouchedClassWithEveryRule_EqualsTheRecord` (a class with all four rules through `From` then `ToClass`:
  the round trip the page relies on; `ClassFixtures` gains two rival forces and a lasting resource with strain).

## Verify by hand
None: the tab shows the rules in CL9d. CL8's hand checks still pass (an untouched class writes back as read).
- **Note from CL9b review:** if this slice's tests need `NoticesOf`/`ChangesOf` (copied in `ResourceRuleTests` and
  `ResourceRivalRuleTests`), move them to one shared test type in `Party/` rather than a third copy; and check
  `ClassResourceViewModel.DuplicateOf` calls every rule's `CopyFrom`.
