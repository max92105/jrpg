# MO1. Select one object, nudge the selection with the arrow keys

Builds on design.md, "What exists" (object mode) and "Key decisions" 9; decisions 509 and 510. Needs MO0 (`MainWindow.ObjectDragging.cs`).

## Goal
In Objects mode: Ctrl + click (without dragging) selects the object drawn on top under the mouse, or clears the selection on empty ground; Ctrl + drag still selects a rectangle. Esc clears the selection. With objects selected and the map focused, the arrow keys move the whole selection one cell, Shift + arrow one art pixel (1/16 of a cell); each press is one undo step, restoring the exact positions. With nothing selected, arrows pan as today.

## New types
- `Editor/Maps/Editing/MoveObjectsEdit` (`internal sealed record MoveObjectsEdit(IReadOnlyList<MoveObjectEdit> Moves) : IMapEdit`): moves several placed objects as one step; each move keeps its from and to, so an undo puts back the exact position (an offset added and taken off a free position like 0.1 wouldn't). Apply in order, revert in reverse.

## Changes
- `Objects/ObjectPlacingViewModel` (163, ends about 195):
  - `public const double NudgeCells = 1;` and `public const double FineNudgeCells = 1.0 / 16;` (tunables; one Mana Seed pixel, exact in binary).
  - `public void SelectObjectAt(Point2D point)`: the top object there (`ObjectAt`) as the only selected one; none clears.
  - `public void ClearSelection()`.
  - `public void NudgeSelectedObjects(double cellsRight, double cellsDown)`: one `MoveObjectsEdit` over the selected objects still on the map; nothing (no step) with none selected.
  - The class summary adds "selecting and nudging them".
- `Shell/MainWindow.ObjectDragging.cs`, `FinishObjectDrag`: a selection drag released within Windows' drag distance (`IsPastDragDistance`, already there) calls `SelectObjectAt(start)` instead of `SelectObjectsIn`.
- `Shell/MainWindow.ObjectEditing.cs`, `HandleObjectKey`: becomes a switch on the key after its guard (Objects mode, a map open, no text box focused): Delete as today; Escape clears a selection (handled only when there was one); the arrows, when a selection exists and `MapViewport.IsKeyboardFocusWithin`, nudge by `NudgeCells`, or `FineNudgeCells` while Shift is held. Its summary and the partial's summary name the keys. `OnPreviewKeyDown` already asks `HandleObjectKey` before `HandleViewportKey`, so `MainWindow.xaml.cs` doesn't change.
- `Shell/MainWindow.xaml`: the Objects tab's `TabItem` gets a `ToolTip` on its own line ("Click places the chosen object; Shift + drag moves; Ctrl + click or Ctrl + drag selects; arrows nudge the selection, Shift for one pixel; Esc clears it; right click or Delete removes."), the comment above it updated to match (+1).
- `Shell/ControlsHelpWindow.xaml`, Objects: "Ctrl + click: select the object under the mouse (on empty ground: select nothing). Esc: clear the selection." and "Arrow keys, with objects selected: move them one cell; Shift + arrow: one art pixel. Each press is one Ctrl+Z step. With nothing selected, arrows pan."

## Tests
New `Objects/ObjectPlacingMoveTests` (`ObjectPlacingViewModelTests` is 198 lines; on the test world's forest with two objects placed in code, one over the other):
- `ObjectPlacingMoveTests: SelectObjectAt_TwoObjectsOverlapping_SelectsOnlyTheTopOne` (the lower one placed last in the list but drawn under, so the list order can't pass it).
- `ObjectPlacingMoveTests: SelectObjectAt_EmptyGround_ClearsTheSelection`.
- `ObjectPlacingMoveTests: ClearSelection_TwoSelected_EmptiesIt`.
- `ObjectPlacingMoveTests: NudgeSelectedObjects_TwoSelected_MovesBothByTheStep`.
- `ObjectPlacingMoveTests: NudgeSelectedObjects_FineStep_MovesByOneSixteenth`.
- `ObjectPlacingMoveTests: NudgeSelectedObjects_Undo_PutsBothBackExactlyInOneStep` (from a free position (2.3, 4.7): one undo, both back, equal to the doubles placed).
- `ObjectPlacingMoveTests: NudgeSelectedObjects_NothingSelected_RecordsNoStep` (the map stays clean).
- `ObjectPlacingMoveTests: NudgeSelectedObjects_SelectedObjectUndoneAway_MovesOnlyTheOthers` (a selected object's placement undone: `SelectedObjectUids` already drops it, the nudge moves the rest).
- `ObjectEditingTests: MoveObjectsEdit_ApplyThenRevert_RestoresTheExactPositions` (from (0.1, 0.2), the two-object list reverted in reverse).

Size: about 25 lines in the view model, 15 for the edit, 35 in the window, 4 in XAML and F1; 9 tests (about 115): about 195.

## Map
`docs/map.md`, Editor `Maps / Maps/Editing` row: `MoveObjectsEdit`; `Tiles, Objects` row: `ObjectPlacingViewModel` (selecting one with Ctrl + click, nudging the selection).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5); click the **Objects** tab.
2. Hold Ctrl and click one object (no drag): only it gets the cyan outline, even where two objects overlap (the one drawn in front wins). Ctrl + click on grass: the outline goes.
3. Ctrl + drag a rectangle over two objects; click the map once with the middle button (focus), then press → three times: both move three cells right. Shift + → moves them a hair (one art pixel).
4. Press Ctrl+Z once: the last press undoes; three more: they are back exactly where they were (check by eye against the grid).
5. Click the map with the middle button, then press Esc: the selection clears; → now pans the view.
6. Type in the layer box of the tool bar and press arrows: the caret moves, no object moves.

- **Done, with deviations:** the window's Ctrl + click is `SelectAfterDrag` in `MainWindow.ObjectDragging.cs` (a rectangle past the drag distance, else the object on top); Delete's old body is `DeleteSelectedOrHoveredObject`. The Objects tab's comment now says its tooltip names the gestures. Review: the exact-undo test starts from (0.1, 0.3) with both chests selected; Esc clears the selection only while the map has the keys, as the arrows (a toolbar drop-down keeps its own Esc); a test that a same-object move reverts last first.
