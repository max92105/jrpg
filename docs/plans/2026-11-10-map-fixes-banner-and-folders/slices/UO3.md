# UO3. Events mode reaches the ground under objects

Editor, about 120 lines. Builds on the Events mode as it is (`Shell/MainWindow.EventEditing.HandleEventButton`, `Events/EventEditingViewModel`). Decision 526 (Q7).

## Goal
In Events mode, Ctrl + click opens the clicked cell's event dialog whatever object covers the cell (a new event when the cell has none), and Ctrl + right click removes the cell's event. A plain click still opens the object's events; a plain right click still never removes a cell's event hidden under an object.

## Changes
- `Shell/MainWindow.EventEditing.cs` (124 lines):
  - `HandleEventButton` reads `Keyboard.Modifiers == ModifierKeys.Control` once and routes to two extracted helpers so its switch doesn't grow: `EditCellEventAt(GridCell cell)` (the cell dialog and its Go to, today's second case moved out) and `HandleCellEventButton(MouseButton button, GridCell cell)` (Ctrl: left opens the cell, right removes its event). State the complexity before and after in the Done note.
  - Its summary names the Ctrl gestures.
- `Shell/MainWindow.xaml`, the Events tab's text (same line): "...Ctrl + click opens the event of the cell under an object; Ctrl + right click removes it."
- `Shell/ControlsHelpWindow.xaml`: one line.
- No view model change: `StartEditingCellAt` and `RemoveCellEventAt` already take a cell and ignore objects.

## Tests
The routing is window code (no new type, `.claude/rules/editor.md`); the rules it calls are tested. Add to `Editor.Tests/Events/EventEditingViewModelTests` only if under 250 lines, else `EventEditingCellTests` (new):
- `StartEditingCellAt_CellUnderAnObject_OpensTheCellsEvent` (an object placed over a cell with an event).
- `RemoveCellEventAt_CellUnderAnObject_RemovesIt`.
- `RemoveCellEventAt_CellUnderAnObject_Undo_PutsItBack`.

## Verify by hand
1. Events tab; click a free cell, give it a dialogue step; OK (a cyan or gold square marks it).
2. Objects tab; place a crate over that cell.
3. Events tab; click the crate: the crate's events open (as before); Cancel.
4. Ctrl + click the crate: the cell's event dialog opens with the dialogue step; Cancel.
5. Right click the crate: nothing removed. Ctrl + right click: the cell's square goes; Ctrl+Z brings it back.
6. Ctrl + click a crate-covered cell with no event: an empty cell event dialog opens.
7. F1 lists both gestures.
