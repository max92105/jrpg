# MS1. Open maps stay open

Builds on design.md, "Key decisions" 1; decisions 497 and 498. Needs MS0.

## Goal
Every map opened in the session stays open with its unsaved changes: switching to another map never asks, and switching back shows the first map as it was left. Making a new map never asks either; the new map is listed at once and stays open until it is saved or the editor closes. The map list marks every map with unsaved changes with " *". Ctrl+S still saves the shown map (and the tiles). Until MS4, closing still asks the old yes/no question, now naming every changed map.

Known gaps until later slices of this phase: a hidden open map's unsaved use of a tile, object or quest isn't seen by an undo or a removal, and the purge (Remove sheet on a missing image) rebuilds only the shown map (MS2; the save refuses a broken map, naming the file); a map shown again keeps its changes but Ctrl+Z can't reach its earlier steps (MS3).

## New types
- `Editor/Maps/OpenMaps` (`internal sealed class`): the maps open in this session, each with its unsaved changes and history.
  - `IReadOnlyList<MapViewModel> All` (in opening order); `MapViewModel? Find(string mapId)`; `void Add(MapViewModel map)` (throws on an id already open).
  - `IReadOnlyList<MapViewModel> Dirty` (id order); `event EventHandler? DirtyChanged` (any open map's `IsDirty` changed, relayed from its `PropertyChanged`).

## First: `MapEntry` moves to `Maps/`
`Editor/Shell/MapEntry` → `Editor/Maps/MapEntry` with `public static MapEntry Of(MapDefinition map, bool isDirty)` ("Display name (id)", then " *" while dirty); `MainViewModelSaveTests.Save_NewMap_AddsItsEntry` gains the using.

## Changes
- `Shell/MainViewModel.MapSwitching.cs`:
  - `private readonly OpenMaps _openMaps = new();` (a field initializer, in the partial; MS2 hands it to the usage checks).
  - `TryOpenMap(mapId)`: shows the open one (`Find`) or opens and adds it; never asks; false only for an id neither saved nor open. Its summary changes accordingly.
  - `TryCreateMap(newMap)`: no question; the new map is added to the open maps and shown.
  - `StartNewMap`: the taken ids are the saved maps' and the open maps' (a second new map can't take the first's id).
  - `ShowMapList`: the saved maps plus the open maps not saved yet, each `MapEntry.Of(map, isDirty)` (an open map's own `Map` for its text); rebuilt on `_openMaps.DirtyChanged` (subscribed when the field is first used by `TryOpenMap`/`TryCreateMap`: the constructor isn't touched) and after a save.
  - `HasUnsavedChanges`: `_openMaps.Dirty.Count > 0 || Content is { IsDirty: true }`.
  - `ConfirmLeaveCurrentMap` and `ConfirmDiscard` leave (no caller).
- `Shell/MainViewModel.cs`: `ConfirmClose` asks once, naming every changed map ("Maps 'forest' and 'start' and the tiles have unsaved changes. Discard them?"), until MS4 replaces it (same length).
- `Shell/ControlsHelpWindow.xaml`, Anywhere: "Maps stay open: switch freely, a * in the map list marks unsaved changes. Ctrl+S saves the shown map and the tiles."

## Tests
New `Shell/MainViewModelOpenMapsTests` (the switching tests leave `MainViewModelTests`, 404 lines, re-pinned here):
- `TryOpenMap_NoUnsavedChanges_OpensItWithoutAsking` (moved).
- `TryOpenMap_UnsavedChanges_OpensTheOtherWithoutAsking` (was `TryOpenMap_UnsavedChangesAndUserKeepsThem_StaysOnTheMap`: the question is gone, `Questions` empty).
- `TryOpenMap_BackToAMapLeftUnsaved_ShowsItsChanges` (was `..._UserDiscards_OpensTheOther`: the painted tile is still there, `IsDirty` true).
- `TryOpenMap_BackToAMap_ShowsTheSameViewModel` (the same `MapViewModel` instance, so its history comes with it).
- `TryOpenMap_UnsavedCatalogChangeOnly_OpensWithoutAsking` (moved).
- `TryCreateMap_WithAnUnsavedMapOpen_CreatesWithoutAsking` and `TryCreateMap_NewMap_IsListedAndMarked` (its entry ends with " *").
- `StartNewMap_IdOfAnOpenNewMap_IsTaken` (the second dialog refuses the first new map's id).
- `MapEntries_MapPaintedThenUndone_LosesItsMark` (the mark follows `IsDirty` both ways: one test per side, painted then undone).
- `ConfirmClose_TwoMapsChanged_AsksOnceNamingBoth` (re-pinned from `ConfirmClose_UnsavedMapAndCatalogChanges_AsksOnceNamingBoth`).
- New `Maps/OpenMapsTests`: `Add_SameIdTwice_Throws`; `Dirty_TwoMapsOneChanged_ListsIt`; `DirtyChanged_MapPainted_IsRaised`.

Size: about 40 lines for `OpenMaps`, 50 in the partial and the main file, 8 for `MapEntry`; 13 tests (about 140, of which about 40 re-pinned lines): about 240.

## Map
`docs/map.md`, Editor `Shell` row: maps stay open (`OpenMaps`); `Maps` row: `OpenMaps`, `MapEntry` (one row of a map list, marked while unsaved).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5).
2. Paint a cell on the open map: its row in the map list gets a " *". Click another map: it opens with no question and is the one selected in the list. Paint there too: its row gets a " *" and stays selected.
3. Click the first map again: your painted cell is there. Both rows keep their " *".
4. Ctrl+S: the shown map's " *" goes; the other keeps it. In Visual Studio's Git Changes, only the saved map's layout file is listed (and `tiles.json` if a tile was new).
5. File > New map...: no question; the new map opens and is listed at once with a " *". Click another map and back: the new map is still there.
6. File > Exit: the question names every map with a " *"; answer No to stay.
