# CL7b. One resource as typed: id, name, color, lasting, amounts and gains

Editor, about 300 lines (about 150 of them tests). Split from CL7 on 2026-11-17; needs CL7's `Party/ResourceGainsBoxes`
and `ClassFixtures.Fury`. Design: sections 1, 3; decision 950.

## Goal
One resource as the Resources tab will type it: its id (typed while new, fixed once the draft holds it: skills name it),
name, color text, the lasting box, its amounts (a max and a start, or "from speed" with the speed per charge, which turns
the other two off) and its gains (CL7's boxes). Its problem names the first box that isn't a whole number; Data's rules
(a start within the max, a color's shape, heavy-hit share 1 to 100) are the list's (CL7c). The four rules (Will scaling,
backfire, strain, dominance) are carried silently from the record until CL9.

## New types
- `Party/ClassResourceViewModel` (`ObservableObject`): one resource as the Resources tab types it.
  - `From(ResourceDefinition)`, `New()` (the blank, design section 1: id and name empty, color `#FFFFFF`, max 1,
    starts at 0, no gains, no rules), `IsNew`.
  - `[ObservableProperty]` `Id`, `Name`, `ColorText`, `IsLasting`, `TakesMaxFromSpeed`, `MaxText`, `StartsAtText`,
    `SpeedPerChargeText`; `ResourceGainsBoxes Gains`.
  - `Label` ("mana: Mana", "(new resource)"); `Problem`: the id rule while new (`ObjectIds.ProblemOf` against the
    class's other resources' ids; CL7b picks how the resource reads them, a method taking the ids or a reader its list
    hands it, and says which in its Done line), then the amount boxes that count (max and start, or the speed per
    charge), then `Gains.Problem`.
  - `ResourceDefinition ToResource()`: `_original with` the edited fields; with `TakesMaxFromSpeed`, `Max` and
    `StartsAt` are 0 and `MaxFromSpeed` is `new SpeedScaling(...)`; without it `MaxFromSpeed` is null; `Gains` from
    `Gains.Typed`. The rules are `_original`'s until CL9. `ResourceDefinition` and its sub-records hold no list, so an
    untouched resource equals its record by value.
  - `Changed` (typed fields only, the gains' typed boxes included through their `Problem` notice, as
    `ClassViewModel.OnBoxTyped`), for CL7c's list.
  - `DuplicateOf` for a class's Duplicate: the texts as typed (`Gains.CopyFrom`), never `ToResource`.

## Tests
- `Party/ClassResourceViewModelTests` (new; resources from `ClassFixtures`, `Fury` with every gain different):
  - `From_Resource_ShowsEveryGain`; `From_SpeedResource_TicksFromSpeed`.
  - `ToResource_UntouchedResource_EqualsTheRecord`.
  - `ToResource_TypedPerHitTaken_SetsIt`.
  - `ToResource_FromSpeed_ClearsMaxAndStart` (typed max 5 before ticking: written 0, with the speed scaling).
  - `ToResource_FixedAmounts_ClearsSpeedScaling` (a speed resource unticked).
  - `ToResource_UntouchedResource_KeepsItsRules` (a resource with backfire and Will scaling: carried by content).
  - `Problem_FromSpeed_IgnoresMaxBox` and `Problem_FixedAmounts_NamesMaxBox` (both sides); the same pair for the speed
    per charge box.
  - `Problem_GainNotANumber_NamesTheGain`.
  - `Problem_NewResourceWithClassmateId_NamesTheId`; `Problem_SavedResource_IgnoresIdRule`.
  - `Label_NewResourceWithoutId_SaysNewResource`; `Changed_GainTyped_IsRaised`; `DuplicateOf_HalfTypedBox_CopiesIt`.

## Verify by hand
None: the tab comes in CL8.
