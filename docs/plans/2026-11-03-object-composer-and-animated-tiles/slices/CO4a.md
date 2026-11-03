# CO4a. ObjectComposing: starting the composer and applying it

Builds on design.md, "Key decisions" 1 and 2, and decisions 455, 459 and 461.

## Goal
The menu entries' view model, still headless: New starts the composer from the rectangle chosen in the Tiles panel (sized to it and painted with it), or from an empty 3 x 3 canvas; Edit starts it on the object chosen in the Objects panel (not an NPC: it has no cells). Save makes the canvas's cells tiles where they aren't yet (walkable, outside the history, decision 113), then sets the object as one undo step and chooses it in the Objects panel. Edit keeps every placement and event (they name the object by id). `MainViewModel` holds it.

## New types
- `Editor/Objects/Composing/ObjectComposing` (`internal sealed partial class : ObservableObject`): Objects > New object and Edit object: starts the composer on the chosen cells or object, and applies it as one undo step.
  - `ObjectComposing(CatalogDraft catalog, ObjectBrowserViewModel objects, TileBrowserViewModel tiles)`.
  - `public const int NewCanvasCells = 3;`
  - `ObjectComposerViewModel StartNew()`; `bool CanEdit` (a chosen object with tiles, raised when the choice changes); `ObjectComposerViewModel? StartEdit()`.
  - `bool TryApply(ObjectComposerViewModel composer)`: false while `composer.Problem` isn't null, nothing changed.

## Changes
- `Editor/Shell/MainViewModel`: `public ObjectComposing? Composing { get; }`, built beside `Placing` (+3 lines; CO5 takes out about 35).

## Tests (new `Objects/Composing/ObjectComposingTests`)
- `ObjectComposingTests: StartNew_RectangleChosenInTheTiles_StartsTheCanvasWithIt`.
- `ObjectComposingTests: StartNew_NoRectangleChosen_StartsAnEmptyThreeByThreeCanvas`.
- `ObjectComposingTests: StartEdit_TileObjectChosen_StartsWithItsCellsAndItsId`.
- `ObjectComposingTests: CanEdit_NpcChosen_IsFalse` (and `CanEdit_NothingChosen_IsFalse`, `CanEdit_TileObjectChosen_IsTrue`).
- `ObjectComposingTests: TryApply_NewWithCellsNotTilesYet_MakesTheTilesAndTheObjectThenChoosesIt` (the case of `MainViewModelTests.CreateObject_CellsNotTilesYet_AddsTilesAndTheObjectThenChoosesIt`).
- `ObjectComposingTests: TryApply_NewWithATakenId_ReturnsFalseAndKeepsTheObject` (the case of `CreateObject_IdTaken_ReturnsFalseAndKeepsTheObject`).
- `ObjectComposingTests: TryApply_Undo_RemovesTheNewObjectAsOneStep`.
- `ObjectComposingTests: TryApply_EditOfAPlacedObjectWithEvents_KeepsThePlacementsAndEvents` (the case of `ObjectReskinningTests.Reskin_PlacedObjectWithEvents_KeepsThePlacementsAndEvents`, on the test world's chest).
- `ObjectComposingTests: TryApply_EditUndo_BringsTheOldPictureBack`.
- `ObjectComposingTests: TryApply_ScatteredCellsFromTwoSheets_MakesOneObjectOfBoth` (the developer's case: parts from anywhere).

Size: about 70 lines of code, 12 tests (about 200).

## Verify by hand
Nothing to see yet (CO5).

- **From CO1's review:** a rectangle wider or taller than `ObjectCanvas.MaxCells` makes `ObjectCanvas.Of` throw, while today's New object from selection accepts it: `StartNew` must not pass one through.
