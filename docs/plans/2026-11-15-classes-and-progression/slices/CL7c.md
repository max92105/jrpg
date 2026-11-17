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

- **Note from CL7b:** `ClassResourceViewModel.DuplicateOf` moved here with its user, the class's Duplicate, to keep
  CL7b near its size: a copy of every text as typed (`Gains.CopyFrom`), the id kept since the class's skills name it,
  `IsNew` true as the draft holds no resource of the new class; tests `DuplicateOf_HalfTypedBox_CopiesTheTextsAndId`,
  `DuplicateOf_TypedSpeedResource_WritesTheSameRecord` and `DuplicateOf_TypedFixedAmounts_WritesTheSameRecord`. A
  resource's problem is `ProblemAmong(classmateIds)`: the class passes the other resources' ids (by position for a
  new one without an id), and `ToResource` checks only the boxes.
- **Done, with deviations:** split on 2026-11-17, the whole slice estimated at about 400 lines with tests: the
  selection, Add, Remove, Up, Down and the list's change relay moved to CL7d (note in CL8). CL7c built
  `ClassViewModel.Resources.cs` (48 lines): `Resources` from the record, each resource's `Changed` raising `Problem`
  and the class's `Changed` once, the first resource's problem through `Catalogs/RecordListProblem` ("Resource fury:
  ..."), `ResourcesAsTyped` keeping the record's own list while equal in order. `ClassViewModel` (181) takes the
  resources in its constructor, writes them in `ToClass`, and its `DuplicateOf` copies them as typed through
  `ClassResourceViewModel.DuplicateOf` (178, with the three tests, `ClassResourceViewModelTests` 293).
  `ClassCatalogViewModel`'s relay comment names the resource fields. `Party/ClassResourceListTests` (135, 14 tests:
  record order, untouched equal, typed name, the resource's problem and its place after the growth's, Data's start
  rule at both bounds and one past each, the notices, the copy as typed and new, a copied id taken, the copy watched).
