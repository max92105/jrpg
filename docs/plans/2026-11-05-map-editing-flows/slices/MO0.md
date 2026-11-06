# MO0. Moves only: the object drags

Builds on README, "Guiding rules" (pure moves); C1. No behavior change: the existing tests pass unchanged and none is added.

## Goal
`Shell/MainWindow.ObjectEditing.cs` (257) makes room before MO1, MO2 and SN1 grow the object drags: the drag fields (`_selectionStart`, `_movedObject`, `_ghostDefinition`) and `ShowObjectDragAt`, `ShowGhost`, `FinishObjectDrag`, `IsPastDragDistance`, `CancelObjectDrag` move, with their comments, into a new partial `Shell/MainWindow.ObjectDragging.cs` (summary: "The object drags on the map: the selection rectangle, the moved objects and the placing ghost."), about 75 lines. `ObjectEditing.cs` ends about 185.

## New types
None (one new file holding moved code).

## Tests
None new. Guard: the whole suite passes unchanged (same count before and after).

Size: about 75 lines moved, 10 of file header.

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5); Objects tab; choose an object.
2. The ghost follows the mouse; a click places it; Shift + drag moves a placed object; Ctrl + drag draws the dashed selection rectangle and selects; all as before.

- **Done:** as planned. `MainWindow.ObjectEditing.cs` ends at 185 lines, the new `MainWindow.ObjectDragging.cs` at 83; the test count is unchanged (4195). The summary of `ShowObjectDragAt` was rewrapped by the 120-column check, its words unchanged.
