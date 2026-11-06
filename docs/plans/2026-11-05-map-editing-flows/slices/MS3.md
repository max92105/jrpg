# MS3. Undo follows each open map

Builds on design.md, "Key decisions" 3; decision 503; ADR 0014 (one undo order over the map and the catalogs). Needs MS1 and MS2 (the refusals read every open map, which keeping steps across switches relies on).

## Goal
Each open map keeps its undo steps when another map is shown. Ctrl+Z undoes the newest step of the shown map or of the tiles and objects, skipping (and keeping) other maps' steps; Ctrl+Y the same way. A new edit anywhere still ends every redo, as today. Switching maps forgets nothing, so going back to a map and pressing Ctrl+Z undoes its last change.

Why redo survives a switch now: it was cleared because a catalog redo (a removal) could take away what a map dropped and saved since uses; maps aren't dropped any more, and a map can only start using a record through an edit, which ends every redo.

## New types
- `Editor/Shell/UndoStep` (`internal readonly record struct UndoStep(UndoTarget Target, string? MapId)`): one step in the undo order: the catalogs, or which open map. `MapId` is null for the catalogs.

## Changes
- `Shell/UndoJournal`:
  - `Record(UndoStep step)` (clears every undone step, as today).
  - `bool CanUndo(string? shownMapId)`, `UndoStep? NextUndo(string? shownMapId)`, `UndoStep Undo(string? shownMapId)`: the newest done step whose target is the catalogs or whose map is the shown one; the others stay in place. `CanRedo`, `Redo` the same over the undone steps.
  - `ForgetMapSteps` leaves; `Clear` (the purge) and `ForgetUndoneCatalogSteps` stay.
  - The summary: "the order of the steps recorded in the open maps' histories and the catalogs' (ADR 0014); Undo and Redo go to the newest step of the shown map or the catalogs."
- `Shell/CombinedHistory`: `OpenMap`'s setter no longer forgets; map steps are recorded with the shown map's id; Undo and Redo pass the shown map's id and send a map step to the shown map's `UndoCommand`/`RedoCommand`; `NotifyUndoRedoChanged` also runs when the shown map changes (Undo can become possible or not). Its summary drops "another map starts with none of the old one's steps".
- `MainViewModel.cs`: nothing (`History.OpenMap` is set in the MapSwitching partial's `OnCurrentMapChanged`).
- `Shell/ControlsHelpWindow.xaml`, Anywhere: "Ctrl+Z / Ctrl+Y: undo / redo the shown map's changes and the tiles' and objects', newest first; each open map keeps its own."

## Tests
`Shell/UndoJournalTests` (66 lines), re-pinned and added:
- `Undo_MapStepThenCatalogStep_UndoesTheCatalogFirst` (re-pinned to `UndoStep`s on map "a", shown "a").
- `ForgetMapSteps_UndoneCatalogStep_CanNoLongerBeRedone` and `Undo_AfterAnotherMapOpened_SkipsTheClosedMapsSteps` leave with `ForgetMapSteps`; replaced by:
- `Undo_StepsOnTwoMaps_UndoesOnlyTheShownMapsAndTheCatalogs` (done: a1, catalog, b1; shown "a": catalog, then a1; b1 stays).
- `Undo_OtherMapShownAgain_ItsStepsAreStillThere` (shown "b" after: b1 undoes).
- `CanUndo_OnlyAnotherMapsSteps_IsFalse`.
- `Redo_UndoneOnMapA_RedoesOnlyWhenAIsShown`.
- `ForgetUndoneCatalogSteps_MapStepUndoneToo_KeepsTheMapStep`, `Redo_AfterANewStep_HasNothing`: re-pinned to `UndoStep`s.

Through `MainViewModel`:
- `MainViewModelOpenMapsTests: Undo_AfterSwitchingAwayAndBack_UndoesTheMapsLastPaint`.
- `MainViewModelOpenMapsTests: Undo_OnAnotherMap_DoesNotTouchTheHiddenMap` (painted on A, shown B with no steps: Ctrl+Z greyed; A unchanged).
- `CombinedHistoryObjectTests: Redo_RemovalUndoneThenAnotherMapOpened_CanStillExecute` (re-pinned from `..._CannotExecute`: the reason is above).
- `CombinedHistoryObjectTests: Redo_RemovalUndoneThenObjectPlacedOnAnotherMap_CannotExecute` (the placement is an edit, which ends the redo).
- `MainViewModelUndoTests: Undo_CatalogStepAfterSwitchingMaps_UndoesIt` and the three "used on map" refusals run unchanged (their maps were saved before the switch).

Size: about 50 lines of code (journal 30, history 15, record 3, F1 1); 9 new or replaced tests and 4 re-pinned (about 130): about 180.

## Map
`docs/map.md`, Editor `Shell` row: one undo order over the open maps and the catalogs, each map keeping its own steps (`CombinedHistory` over `UndoJournal`, `UndoStep`).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5).
2. Paint a cell on map A; open map B and paint a cell there; open map A again.
3. Press Ctrl+Z: A's cell goes. Ctrl+Z again: greyed if nothing else was done on A or the tiles (B's cell is untouched).
4. Open map B: Ctrl+Z undoes B's cell. Ctrl+Y redoes it.
5. Mark a tile blocked (Tiles > Mark blocked, click a sheet cell); open map A; Ctrl+Z: the mark is undone from map A too (a tiles step undoes from any map).
