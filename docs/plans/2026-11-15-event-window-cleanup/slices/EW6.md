# EW6. Keys for the lists

Editor + script, about 110 lines (about 90 of view code and XAML, about 20 in the script). Design: section 1
("Keys"), key decision 4, risk "Keyboard focus after a key". Decision 734. Needs EW4 (the final layout), EW2
(`DropDownMenu`, the Add step button) and EW0 (`-EventWindows`).

## Goal
With the step list focused: **Insert** opens Add step (arrows and Enter pick a kind), **Delete** removes the chosen
step, **Alt+Up** and **Alt+Down** move it. With the object window's event list focused: **Insert** adds an event,
**Delete** removes the chosen one, **Alt+Up** and **Alt+Down** move it. After each, the list keeps the keyboard on its
chosen row, so the keys can be pressed again. The buttons stay, their tooltips naming the keys.

## New types
- `Events/ListKeyCommand` (`internal static class`, view code): runs a list's command from a key and gives the
  keyboard back to the list's chosen row.
  - `public static void Run(ListBox list, ICommand command)`: does nothing when `command.CanExecute(null)` is false
    (`.claude/rules/editor.md`: the toolkit's `Execute` doesn't check), else executes it, then focuses the chosen row's
    container once the list has laid out its rows again (a removed or moved row's container is replaced, which drops
    the focus).

## Changes
- `Events/EventView.xaml` and `.xaml.cs`: the step list gets `x:Name="StepList"` and `KeyDown="OnStepListKeyDown"`.
  The handler reads the key as `e.Key == Key.System ? e.SystemKey : e.Key` (Alt turns an arrow into `Key.System`) and
  the modifiers: Delete runs `RemoveStepCommand`, Alt+Up `MoveStepUpCommand`, Alt+Down `MoveStepDownCommand`, each
  through `ListKeyCommand.Run`, and marks the key handled; Insert calls `DropDownMenu.Open(AddStepButton)` (an open
  context menu takes the keyboard, so the arrows move in it). A plain Up or Down stays the list's own.
- `Events/ObjectEventsWindow.xaml` and `.xaml.cs` (about 60 lines after EW3, about 80 after): the event list gets
  `x:Name="EventList"` and `KeyDown="OnEventListKeyDown"`: Insert `AddEventCommand`, Delete `RemoveEventCommand`,
  Alt+Up `MoveEventUpCommand`, Alt+Down `MoveEventDownCommand`, through `ListKeyCommand.Run`.
- Tooltips: the steps' Remove "Remove the chosen step (Delete)", Up "Move the chosen step up (Alt+Up)", Down "... down
  (Alt+Down)", Add step "Add a step after the chosen one (Insert in the list)"; the events' Add "Add an event (Insert
  in the list)", Remove, Up, Down likewise.
- `Shell/ControlsHelpWindow.xaml`, Events: "In an event dialog's step list: Insert opens Add step, Delete removes the
  chosen step, Alt+Up and Alt+Down move it. In the event list: Insert adds an event, Delete removes it, Alt+Up and
  Alt+Down move it."
- `.claude/scripts/open-editor-windows.ps1`, the `-EventWindows` pass: in the object window, before closing it, select
  the step list's first row (`SelectionItemPattern.Select`, then `SetFocus` on it), send Alt+Down
  (`[System.Windows.Forms.SendKeys]::SendWait('%{DOWN}')`, with `System.Windows.Forms` added to its `Add-Type`), wait,
  and save a second capture (`EventsOfObjectKeys.png`); then close as Cancel. The header's `-EventWindows` line adds
  "and moves the first step down with Alt+Down".
- `docs/map.md`, Events row: `ListKeyCommand`.

## Tests
None new: the commands are tested (`RemoveStepCommand`, `MoveStepUpCommand`, the event commands), and
`ListKeyCommand` is window code (it needs a laid-out `ListBox`, which editor tests never build). Every test passes
unchanged.

## Verify by hand
1. The implementer: `open-editor-windows.ps1 -EventWindows`: the key capture shows the first step moved to second
   place and still chosen (an object with one step shows no move: the script says which object it reached); no
   binding-error line beyond EW0's baseline; nothing under `content/` changed.
2. The developer (F5): **Events** tab, click an object with two events of three steps or more. Click the first step.
   **Alt+Down** twice: it moves to third place and stays chosen; **Alt+Up**: back to second.
3. **Delete**: the step goes and the next one is chosen; **Delete** again removes that one too (the keyboard stayed in
   the list). **Cancel** brings the steps back.
4. **Insert**: Add step opens with the keyboard in it; **Down** to Warp, **Enter**: a warp step after the chosen one.
5. Click in the dialogue's lines box and press **Delete**: it deletes a character, not the step.
6. Click the event list: **Insert** adds an event and chooses it; **Alt+Up** moves it up; **Delete** removes it.
7. In a cell's window, the step list keys work the same.
8. Hover each list button: the tooltip names its key. **F1**: the two key lines.
