# CL7c. A class's resources as typed: the list, Add, Remove and moves

Editor, about 250 lines (about 140 of them tests). Split from CL7 on 2026-11-17; needs CL7b's
`Party/ClassResourceViewModel`. Design: section 3; decision 950.

## Goal
The class's resources as the Resources tab will list them: in order, Add (the blank after the chosen one), Remove
(any, decision 950: Data refuses while a class skill uses it), Up and Down. Data's rules (a start within the max, a
color's shape, heavy-hit share 1 to 100, a resource a skill uses) reach the line through `CatalogProblems.OfClasses`.
No page change yet (CL8): the class's record now comes from the resources as typed.

## Changes
- `Party/ClassViewModel.Resources.cs` (new partial): `ObservableCollection<ClassResourceViewModel> Resources`,
  `[ObservableProperty] ClassResourceViewModel? SelectedResource`, `[RelayCommand]` `AddResource`, `RemoveResource`,
  `MoveResourceUp`, `MoveResourceDown` (`Events/ListOrder`, as the skills' partial); the list followed with
  `Catalogs/ListItemWatch.Follow` so an added resource's `Changed` is heard and a removed one's no longer.
- `ToClass` writes the resources as typed, but keeps the record's own `Resources` list while the typed ones equal it
  in order (as `SkillsAsTyped`, decision 1014): `ClassDefinition` compares its lists by reference, so an untouched
  class must hand back the same list to equal its record.
- `Problem` adds the resources' first problem ("Resource mana: Max must be a whole number"; by position while new
  without an id); a resource's typed change and the list's change raise `Problem` and `Changed` once.
- `ClassViewModel.DuplicateOf`: resources copied as typed (`ClassResourceViewModel.DuplicateOf`), no longer from the
  record; the class summary no longer says resources are carried.
- `docs/map.md`: the Editor `Party` row's `ClassViewModel` names the `.Resources` partial.

## Tests
- `Party/ClassResourceListTests` (new; a `ClassViewModel` over `ClassFixtures`):
  - `AddResource_WithSelection_InsertsAfterIt`; `AddResource_NothingSelected_AddsLast`;
    `RemoveResource_SavedResource_RemovesIt` (decision 950: any); `MoveResourceUp_FirstRow_IsGreyed`;
    `MoveResourceDown_MovesItOneDown`.
  - `ToClass_UntouchedClass_EqualsTheRecord` (the scholar, whose one resource keeps the record's list).
  - `ToClass_TypedResourceName_WritesIt`.
  - `Problem_ResourceBoxNotANumber_NamesTheResource`.
  - `Problem_ResourceUsedBySkillRemoved_ShowsDataMessage` (through the catalog's line).
  - `Problem_StartAboveMax_ShowsDataMessage` and `Problem_StartEqualToMax_IsNull` (both sides of Data's range).
  - `Problem_ResourceBoxTyped_RaisesProblemChanged`; `Changed_ResourceAdded_IsRaisedOnce`;
    `RemoveResource_RemovedResourceTyped_RaisesNothing`.
  - `DuplicateOf_HalfTypedResource_CopiesIt`.

## Verify by hand
None: the tab comes in CL8. CL4's hand check 3 still passes (the resources go through the boxes unchanged).
