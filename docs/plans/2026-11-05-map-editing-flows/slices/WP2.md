# WP2. The warp step's side of the picker

Builds on design.md, "Key decisions" 6 and 7; decisions 506 and 508. Needs WP1 and MS2 (`OpenMaps.Layouts`).

## Goal
Headless: an event dialog opened on the map carries the maps a warp can lead to (every listed map, each open map as its unsaved draft); a warp step starts the picker on its own map and cell, takes the picked map and cell into its boxes, and says where it leads once its fields are valid (what Go to follows, WP4). Nothing visible yet (WP3).

## New types
- `Editor/Events/WarpDestination` (`internal sealed record WarpDestination(string MapId, GridCell Cell)`): where a warp step leads, once its fields are valid.

## Changes
- `Events/StepChoices`: `public WarpMaps WarpMaps { get; init; } = WarpMaps.Empty;` (an init member, as `ShopIds`); the summary names "the maps a warp can lead to, drawn by the picker".
- `Events/EventEditingViewModel(CatalogDraft catalog, OpenMaps openMaps)`: `Choices()` adds `WarpMaps`: the rows of every listed map (the saved maps and the open new ones, `MapEntry.Of`, by display name then id, as the map list), their layouts with each open map's layout in place of its saved one (`openMaps.Layouts`), and `catalog.Current`. `MainViewModel` passes `_openMaps` (the constructor line, one longer).
- `Events/EventStepViewModel` (263 lines): the warp members go in a new partial file `Events/EventStepViewModel.Warp.cs` (summary "A warp step's destination: picked on the map's picture, and followed by Go to"):
  - `public WarpDestinationViewModel StartPickingDestination(WarpMaps maps)`: on the step's `MapId`, with its cell when both numbers are valid.
  - `public void TakeDestination(WarpDestinationViewModel picked)`: sets `MapId`, `CellXText`, `CellYText`; throws `InvalidOperationException` with the picker's problem when it has one.
  - `public WarpDestination? Destination`: the map and cell when the step is a warp whose `Problem` is null; null otherwise.
  - The numbers are read with the step's `WholeNumber` rule (made `private static` and shared by both files, unchanged).
- `tests/.../Events/EventEditingViewModelTests`: its one construction takes an `OpenMaps`.

## Tests
New `Events/EventStepViewModelWarpTests` (`EventStepViewModelTests` is 264 lines):
- `StartPickingDestination_ValidStep_StartsOnItsMapAndCell`.
- `StartPickingDestination_CellHalfTyped_StartsWithNoCell`.
- `StartPickingDestination_NewStep_StartsWithNoMap`.
- `TakeDestination_PickedMapAndCell_SetsTheStepsFields` (then `ToStep()` is the `WarpStep` with them).
- `TakeDestination_PickerWithAProblem_Throws`.
- `Destination_ValidWarp_IsItsMapAndCell`; `Destination_CellHalfTyped_IsNull`; `Destination_NotAWarp_IsNull`.

In `Events/EventEditingViewModelTests` (170 lines):
- `StartEditingAt_HiddenOpenMapPainted_WarpMapsHoldItsDraftLayout` (paint on forest, show start, open an object's events on start: `Choices.WarpMaps.Layouts["forest"]` has the painted tile).
- `StartEditingAt_NewMapOpen_WarpMapsListIt`.
- `StartEditingAt_WarpMaps_ListTheMapsInTheMapListsOrder`.

Size: about 50 lines of code; 11 tests (about 115): about 165.

## Map
`docs/map.md`, Editor `Events` row: `StepChoices.WarpMaps`; `EventStepViewModel` (a warp step's destination, picked or followed).

## Hand check (yours)
Nothing to see yet (WP3).
