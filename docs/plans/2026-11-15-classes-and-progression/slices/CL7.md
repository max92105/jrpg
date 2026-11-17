# CL7. One resource as typed: amounts and gains

Editor, about 210 lines (about 100 of them tests). Needs CL6. Design: sections 1, 3; decision 950.

## Goal
The view model of a class's resources: the list in order (Add the blank after the chosen one, Remove any, Up and Down)
and one resource as typed: its id (typed while new, fixed once the draft holds it: skills name it), name, color text,
the lasting box, its amounts (a max and a start, or "from speed" with the speed per charge, which turns the other two
off) and its gains (per turn, per hit taken, per heavy hit with the heavy-hit share of max HP, per hit landed, per ally
fallen, per enemy fallen, per battle fought). Its problem names the first box that isn't a whole number; Data's rules
(a start within the max, a color's shape, heavy-hit share 1 to 100) reach the line through `CatalogProblems.OfClasses`.
The four rules (Will scaling, backfire, strain, dominance) are carried silently from the record until CL9. No page
change yet (CL8): the class's record now comes from the boxes.

## New types
- `Party/ResourceGainsBoxes` (`ObservableObject`): a resource's eight gains and heavy-hit share as typed boxes.
  `ResourceGainsBoxes(ResourceGains)`; eight `...Text` properties; `Problem` in the form's order ("Per hit taken must be
  a whole number"); `ResourceGains ToGains()` (throws on a problem); `CopyFrom`.
- `Party/ClassResourceViewModel` (`ObservableObject`): one resource as the Resources tab types it.
  - `From(ResourceDefinition)`, `New()` (the blank, design section 1), `IsNew`.
  - `[ObservableProperty]` `Id`, `Name`, `ColorText`, `IsLasting`, `TakesMaxFromSpeed`, `MaxText`, `StartsAtText`,
    `SpeedPerChargeText`; `ResourceGainsBoxes Gains`.
  - `Label` ("mana: Mana", "(new resource)"); `Problem`: the id rule while new (`ObjectIds.ProblemOf` against the
    class's other resources' ids), then the amount boxes that count (max and start, or the speed per charge), then the
    gains.
  - `ResourceDefinition ToResource()`: `_original with` the edited fields; with `TakesMaxFromSpeed`, `Max` and
    `StartsAt` are 0 and `MaxFromSpeed` is `new SpeedScaling(...)`; without it `MaxFromSpeed` is null. The rules are
    `_original`'s until CL9.
  - `DuplicateOf` for a class's Duplicate.

## Changes
- `Party/ClassViewModel.Resources.cs` (new partial): `ObservableCollection<ClassResourceViewModel> Resources`,
  `[ObservableProperty] ClassResourceViewModel? SelectedResource`, `[RelayCommand]` `AddResource`, `RemoveResource`,
  `MoveResourceUp`, `MoveResourceDown`; `ToClass` writes `Resources = [.. Resources.Select(r => r.ToResource())]`;
  `Problem` adds the resources' first problem ("Resource mana: Max must be a whole number"); a resource's typed change
  and the list's change raise `Problem`.
- `Party/ClassViewModel.DuplicateOf`: resources copied as typed.

## Tests
- `Party/ClassResourceViewModelTests` (new; resources built in `ClassFixtures`, every gain a different number):
  - `From_Resource_ShowsEveryGain`.
  - `ToResource_TypedPerHitTaken_SetsIt`.
  - `ToResource_FromSpeed_ClearsMaxAndStart` (typed max 5 before ticking: written 0, with the speed scaling).
  - `ToResource_FixedAmounts_ClearsSpeedScaling` (a speed resource unticked).
  - `ToResource_UntouchedResource_KeepsItsRules` (a resource with backfire and Will scaling: carried by content).
  - `Problem_FromSpeed_IgnoresMaxBox` and `Problem_FixedAmounts_NamesMaxBox` (both sides).
  - `Problem_NewResourceWithClassmateId_NamesTheId`.
- `Party/ClassResourceListTests` (new; a `ClassViewModel` over `ClassFixtures`):
  - `AddResource_WithSelection_InsertsAfterIt`; `RemoveResource_SavedResource_RemovesIt` (decision 950: any).
  - `Problem_ResourceUsedBySkillRemoved_ShowsDataMessage` (through the catalog's line).
  - `Problem_StartAboveMax_ShowsDataMessage` and `Problem_StartEqualToMax_IsNull` (both sides of Data's range).
  - `Problem_ResourceBoxTyped_RaisesProblemChanged`.

## Verify by hand
None: the tab comes in CL8. CL4's hand check 3 still passes (the resources go through the boxes unchanged).

- **Done, with deviations:** split on 2026-11-17, the whole slice estimated at about 700 lines with tests: CL7 built
  `Party/ResourceGainsBoxes` (106 lines), whose gains read through `Typed` (null while a box doesn't read, as
  `ClassStatsBoxes`) rather than a throwing `ToGains`; its labels "Per turn" to "Per battle fought" and "Heavy hit
  share". `ClassFixtures` (116) gains `Fury`, a resource with every gain different and one a drain, and the gain boxes'
  `TextsOf`. `Party/ResourceGainsBoxesTests` (113, 11 tests: the numbers, `Typed` untouched, typed and unreadable, the
  problem's first, share and last box and its order, a drain read, `CopyFrom` with a half-typed box, the notices).
  `Party/ClassResourceViewModel` and its tests moved to CL7b, the class's list and `ClassResourceListTests` to CL7c,
  with notes in CL8 and CL9. No hand check: no page change here.
