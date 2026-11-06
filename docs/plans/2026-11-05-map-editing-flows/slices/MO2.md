# MO2. Shift + drag moves the whole selection

Builds on design.md, "Key decisions" 9; decision 510; MO0 (`MainWindow.ObjectDragging.cs`) and MO1 (`MoveObjectsEdit`).

## Goal
Shift + drag on an object that is part of the selection moves every selected object by the mouse's travel, as one undo step; while dragging, the selection's cyan outlines follow the mouse and the grabbed object's ghost shows under it. Shift + drag on an object outside the selection moves it alone, as today. A press and release within the drag distance moves nothing (a Shift + double-click still opens the shapes).

## New types
None.

## Changes
- `Objects/ObjectPlacingViewModel` (about 195 after MO1, ends about 210): `public void MoveSelectedObjects(Point2D offset)`: one `MoveObjectsEdit` over the selected objects still on the map; nothing for a zero offset. `public bool IsSelected(string uid)`.
- `Shell/MainWindow.ObjectEditing.cs`, `HandleObjectButton`'s Shift branch: remembers whether the grabbed object is selected (the `_movedObject` tuple gains `IsSelection`).
- `Shell/MainWindow.ObjectDragging.cs`:
  - `ShowObjectDragAt`: while moving a selection, `SelectionMarks.RenderTransform` is a `TranslateTransform` of the travel in map pixels (no redraw per move), and the grabbed object's ghost shows as today.
  - `FinishObjectDrag`: past the drag distance, a selection moves with `MoveSelectedObjects(travel)`, a single object with `MoveObject` as today.
  - `CancelObjectDrag` resets the transform.
- `Shell/MainWindow.ObjectEditing.cs`, `DrawSelectionMarks`: resets `SelectionMarks.RenderTransform` (the marks are redrawn at their new places).
- `Shell/ControlsHelpWindow.xaml`, Objects: "Shift + drag: move an object; on a selected object, move the whole selection." (replaces "Shift + drag: move an object.").

## Tests
In `Objects/ObjectPlacingMoveTests` (MO1):
- `ObjectPlacingMoveTests: MoveSelectedObjects_TwoSelected_MovesBothByTheOffsetInOneStep` (one undo puts both back).
- `ObjectPlacingMoveTests: MoveSelectedObjects_ZeroOffset_RecordsNoStep`.
- `ObjectPlacingMoveTests: MoveSelectedObjects_AnUnselectedObject_StaysPut`.
- `ObjectPlacingMoveTests: IsSelected_SelectedAndUnselected_TellsThemApart` (one test per side).

Size: about 15 lines in the view model, 40 in the window; 4 tests (about 55): about 115.

## Hand check (yours)
1. Start the editor, Objects tab. Ctrl + drag over three objects.
2. Hold Shift and drag one of the three: all three outlines follow the mouse; let go: all three moved by the same amount. Ctrl+Z: all three return in one step.
3. Shift + drag an object outside the selection: it moves alone; the selection stays where it was.
4. Shift + double-click a selected object: its shapes open, nothing moves.

- **Done, with deviations:** MO1's `NudgeSelectedObjects(right, down)` became `MoveSelectedObjects(right, down)`, serving the arrows and the drag alike, and records nothing for a zero offset, in place of a second method taking a point; `IsSelected` has a test per side. Review: a wheel zoom during a Shift + drag of the selection snaps its outlines back until the mouse moves (`ApplyViewport` redraws the marks); left, since it is brief and `MainWindow.xaml.cs` would grow.
