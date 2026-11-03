# CO6. Objects > Edit object; Replace cells leaves

Builds on decision 449, design.md, "Key decisions" 2, and the README's picks 1 and 8.

## Goal
Objects > Edit object... (the menu, the Objects tool bar, enabled while `Composing.CanEdit`) opens the composer on the object chosen in the Objects panel: its cells painted, its id shown fixed, its category editable. Save replaces its picture, size and category as one undo step, keeping its id, outline, zones (stretched with a new size), placements and events; the Shapes window opens when its size changed. Objects > Replace cells of the chosen object leaves: Edit does what it did, with any cells from anywhere. Double-click in the Objects panel keeps opening the Shapes window.

## New types
None.

## Changes
- `Editor/Shell/MainWindow.ObjectEditing.cs`: `OnEditObjectClicked` opens `ObjectComposerWindow` over `Composing.StartEdit()`, then `EditShapes(Placing.StartEditingChosenShapes())` when `ShouldOpenShapesAfterSave` (about 12 lines).
- `Editor/Shell/MainWindow.xaml`: "Edit object..." in the Objects menu and tool bar, enabled by `Composing.CanEdit`, with a tooltip ("Repaint the chosen object; its id, outline, zones, placements and events stay. Ctrl+Z puts the old picture back."); the "Replace cells of the chosen object" entry leaves.
- `Editor/Shell/MainViewModel`: `Reskinning` leaves (about −6 lines).
- `Editor/Shell/ControlsHelpWindow`: no gesture changes (menu entries only); the Replace cells line, if listed, leaves.
- Removed: `Objects/ObjectReskinning`, `Objects/ObjectReskin`, `Objects/ObjectFromCells` and their tests `ObjectReskinningTests`, `ObjectReskinTests`, `ObjectFromCellsTests`. Their cases are in the composer's tests: keeping id, category, outline and zones (`ObjectComposerViewModelTests.ToObject_EditOtherSize_*`), placements and events (`ObjectComposingTests.TryApply_EditOfAPlacedObjectWithEvents_*`), undo (`TryApply_EditUndo_*`), the NPC refused (`CanEdit_NpcChosen_IsFalse`), cells not tiles yet (`TryApply_NewWithCellsNotTilesYet_*`, the same `TileIdsOf` path), a sheet without its image (`CanvasPaintingTests.PaintAt_CellsOfASheetWithoutItsImage_*`).
- `docs/map.md`'s Tiles, Objects line: the composer's types in, `NewObjectViewModel`, `ObjectFromCells`, `ObjectReskinning` out.

## Tests
- `ObjectComposingTests: CanEdit_AfterTheChosenObjectIsRemoved_IsFalse` (the menu greys when the choice goes).
- Test count before and after: the removed files' cases are named above.

Size: about 30 lines of code and XAML, 1 test; about 150 lines and six files removed (about 130 counted).

## Verify by hand
- Choose `chest` (or a re-skinned Mana Seed object) in the Objects panel, Objects > Edit object...: its cells show on the canvas, its id is fixed. Repaint one cell with a part from another sheet, Save: every placed chest shows it; in the game the chest still gives its item and opens. Edit again, add a column, Save: the Shapes window opens with the outline stretched; adjust, OK. Ctrl+Z twice in the main window: the outline, then the old picture come back.
- Choose an NPC: Edit object is greyed. The Objects menu no longer lists Replace cells.
