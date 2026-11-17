# CL7d. A class's resources: Add, Remove, Up and Down

Editor, about 220 lines (about 150 of them tests). Split from CL7c on 2026-11-17; needs CL7c's
`ClassViewModel.Resources`. Design: section 3; decision 950.

## Goal
The commands of the class's resource list: Add (the blank after the chosen resource, last when none, chosen), Remove
(any, decision 950: Data refuses while a class skill uses it), Up and Down (greyed at the ends and with nothing chosen).
Each list change reaches the class's `Changed` once, as the skills' list does (decision 1014).

## Changes
- `Party/ClassViewModel.Resources.cs`: `[ObservableProperty] ClassResourceViewModel? SelectedResource` (the first at
  start, as the skill rows); `[RelayCommand]` `AddResource`, `RemoveResource`, `MoveResourceUp`, `MoveResourceDown`
  through `Events/ListOrder`; `Resources.CollectionChanged` followed with `Catalogs/ListItemWatch.Follow`, so an added
  resource's `Changed` is heard and a removed one's no longer, re-raising the moves' greying, `Problem` and `Changed`.
- `docs/map.md`: the Editor `Party` row's `.Resources` partial names the commands.

## Tests
- `Party/ClassResourceListTests` (CL7c's file; or a new `ClassResourceCommandTests` if it would pass 300 lines):
  - `AddResource_WithSelection_InsertsAfterIt`; `AddResource_NothingSelected_AddsLast`.
  - `RemoveResource_SavedResource_RemovesIt` (decision 950: any); `RemoveResource_RemovedResourceTyped_RaisesNothing`.
  - `MoveResourceUp_FirstRow_IsGreyed`; `MoveResourceDown_LastRow_IsGreyed`; `MoveResourceDown_MovesItOneDown`;
    the commands' `CanExecuteChanged` on a selection change.
  - `Problem_AddedResourceWithoutId_NamesItsPosition` ("Resource #3: The id must not be empty.").
  - `Problem_ResourceUsedBySkillRemoved_ShowsDataMessage` (through the catalog's line).
  - `Changed_ResourceAdded_IsRaisedOnce`; `ToClass_AddedResource_WritesIt`.

## Verify by hand
None: the tab comes in CL8.
