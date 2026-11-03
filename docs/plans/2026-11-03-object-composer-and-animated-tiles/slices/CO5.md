# CO5. The composer window; Objects > New object opens it

Builds on design.md, "Key decisions" 2 and 3, and the README's picks 1, 3, 4, 5 and 8.

## Goal
Objects > New object... (the menu and the Objects tool bar, enabled while the content loaded) opens the composer: the Tiles panel's sheet view on the left (the same choice of sheet and cells as the panel), on the right the id and category boxes, the size boxes, the Erase toggle, Undo and Redo, the canvas at the map's cell size with its grid, the problem line, Save and Cancel. A left press or drag on the canvas stamps the chosen cells, a right press or drag erases; Ctrl+Z and Ctrl+Y undo strokes inside the window. Save (enabled without a problem) makes the object, switches to the Objects tab and opens the Shapes window (decision 124). New object from selection, its dialog and its Tiles tab button leave: a rectangle chosen in the Tiles panel now starts the composer painted with it.

## New types
- `Editor/Objects/Composing/ObjectComposerWindow` (XAML + view-only code-behind): the window: the picker, the boxes and the canvas, mouse strokes to `CanvasPainting`. Built with the `ObjectComposerViewModel`, the shared `TileImages` (the main window's `TileBrowser.Images`) and `Func<ObjectComposerViewModel, bool> tryApply`, as `NewObjectWindow` was; style `EditorWindow`, dark templates through the theme.
- `Editor/Objects/Composing/ObjectCanvasDrawing` (static): the canvas's picture: each painted cell's image at the map's cell size, with the grid. `static DrawingImage Draw(ObjectCanvas canvas, TileImages images)`.

## Changes
- `Editor/Tiles/TileImages`: `ImageSource CellImageOf(SheetCell cell)` (the crop by cell, `MissingArtMark` for a missing image or a cell past it); `TileImageOf` calls it.
- `Editor/Shell/MainWindow.ObjectEditing.cs`: `OnNewObjectClicked` opens `ObjectComposerWindow` over `Composing.StartNew()`, then the Objects tab and `EditShapes(Placing.StartEditingChosenShapes())`.
- `Editor/Shell/MainWindow.xaml`: the menu entry and tool bar button read "New object..." and enable with the content; the Tiles tab's "New object from selection..." button leaves.
- `Editor/Shell/MainViewModel`: `CanStartNewObject`, `StartNewObject`, `CreateObject`, the `SelectedCells` listener for them leave (about −35 lines; `AddAndChoose` stays for `CreateNpc`).
- `Editor/Tiles/TileBrowserViewModel.ChoiceText`: the rectangle's line says "New object starts from them" (one sentence; the file stays at 330).
- `Editor/Shell/ControlsHelpWindow`: the composer's lines (left drag paints the chosen cells, right drag erases, Ctrl+Z and Ctrl+Y undo strokes in the composer).
- Removed: `Objects/NewObjectViewModel`, `Objects/NewObjectWindow`, `ObjectFromCells.Build` (its `TilesOf` stays for `ObjectReskin` until CO6), `NewObjectViewModelTests` (its cases are `ObjectNamingTests`, CO3), `ObjectFromCellsTests`' `Build` tests (covered by `ObjectCanvasTests.Of_Rectangle_*` and `ObjectComposingTests.TryApply_New*`).
- Tests switched to the composer (mechanical): `CombinedHistoryObjectTests` (2 tests) and `MainViewModelUndoTests` (1 test) make their object through `Composing!.StartNew()` and `TryApply`; `MainViewModelTests`' `CreateObject_CellsNotTilesYet_*` and `CreateObject_IdTaken_*` leave (their cases are `ObjectComposingTests`, CO4) and `CanStartNewObject_RectangleChosen_IsTrue` leaves with the member (New object is always there).

## Tests
- `TileImagesTests: CellImageOf_CellOfASheetWithoutItsImage_IsTheMark`.
- `TileImagesTests: CellImageOf_CellInsideTheImage_IsCropped`.
- Test count before and after the switch: the removed tests' cases are named above; every other test passes unchanged.

Size: about 90 lines of XAML, 90 of code-behind, 45 for the drawing, 15 elsewhere, 2 tests and about 40 lines of test switch; about 35 lines and two files removed (about 230 net). Watch: if it passes 250 at the slice step, `ObjectCanvasDrawing` and `CellImageOf` move into a slice before it (design.md, "Risks").

## Verify by hand
- With nothing chosen in the Tiles panel, Objects > New object...: an empty 3 x 3 canvas. Choose a Mana Seed sheet in the composer's picker, click a cell, click canvas cells: they show it at 3x. Shift + drag a rectangle in the picker, click a canvas cell: the rectangle is stamped from there, clipped at the edge. Pick a cell from another sheet and paint it beside: both show. Right drag erases; the Erase toggle makes the left button erase. Ctrl+Z undoes the last stroke, Ctrl+Y redoes it. Type 6 columns: the canvas widens, cells stay; type 2: cells past it drop; Ctrl+Z brings them back. Type a taken id: the problem line names it and Save is greyed.
- Save with id `test_tree` and category `Tree`: the Objects tab shows it, the Shapes window opens; draw the outline, OK; place it on the map. Ctrl+Z in the main window removes the object (one step). The Tiles panel still shows the sheet and cell last chosen in the composer.
- Choose a rectangle in the Tiles panel first, then New object...: the canvas starts with it at its size.
- F1 lists the composer's gestures.
