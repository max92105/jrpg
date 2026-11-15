# EW0. Checking the event windows without the developer

Editor + script, about 70 lines (about 10 in `Hosting/App.xaml.cs`, about 60 in the script). Design: section 2 (the
script as it is), key decision 7, risks "Binding-trace noise" and "Clicking the map from a script". Decision 736.

## Goal
The implementer can open both event windows and read their binding errors without the developer: a Debug editor
writes WPF's binding errors to its error output, and `open-editor-windows.ps1 -EventWindows` opens the Events tab,
clicks the map until an "Event of cell" window and an "Events of" window have each opened, saves a capture of each
and closes it as Cancel. Its trace shows any binding error. Nothing in the windows changes.

## New types
None.

## Changes
- `Hosting/App.xaml.cs`: `OnStartup` calls `TraceBindingErrors()`, a `[Conditional("DEBUG")] private static` method
  setting `PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning` and adding a
  `ConsoleTraceListener(useErrorStream: true)` to `PresentationTraceSources.DataBindingSource.Listeners`. Its comment
  says why: the smoke script reads the editor's error output, and a binding error otherwise shows only in Visual
  Studio's Output window. The class summary gains one line: a Debug editor writes binding errors to its error output.
- `.claude/scripts/open-editor-windows.ps1` (read its header first: it closes its editor in `try`/`finally`, answers
  the unexpected-error box with `WM_COMMAND`, and needs `-Command` for several `-Windows`):
  - a `[switch]$EventWindows`; when set, the menu loop is skipped and the event pass runs inside the same `try`.
  - the `EditorBox` import gains `SetCursorPos(int x, int y)` and `mouse_event(uint flags, uint dx, uint dy, uint
    data, UIntPtr extra)` beside `PostMessage`, plus `SetForegroundWindow(IntPtr)`.
  - the pass: bring the main window to the front; find the tab item named "Events" and select it
    (`SelectionItemPattern`); take the map's screen area as right of that tab's `TabControl` (its parent in the
    automation tree) to the main window's right edge, between the tab control's top and bottom, inset by 8 px; then
    for points on a 24 px grid across it (at most 600 clicks), left-click (`SetCursorPos`, then `mouse_event` left down
    and up), wait 700 ms, and look for a dialog with `Find-Dialog`:
    - none: next point;
    - "Unexpected error": reported as ERROR and answered as the menu loop does;
    - a name starting "Event of cell" or "Events of" not seen yet: capture it to `-Out` (`EventOfCell.png`,
      `EventsOfObject.png`), print "Event of cell: opened, <file>" or "Events of: opened, <file>", close it with
      `WindowPattern.Close` (as Cancel: nothing applied);
    - a kind already seen: close it the same way, next point;
    - stop once both kinds were seen; after 600 clicks, print which kind was never reached.
  - the header gains the switch's line: "-EventWindows: open the Events tab and click the map until a cell's and an
    object's event window have each opened; captures both and closes them unchanged."
- `.claude/rules/editor.md`, the open-editor-windows line: "the event windows open from a map click: run
  `-EventWindows` after a change to `Events/` XAML".

## Tests
None: window and script code. The check is the run below, recorded in the Done note.

## Verify by hand (the implementer)
1. `dotnet build src/Jrpg.Editor`, then `powershell -NoProfile -ExecutionPolicy Bypass -File
   .claude/scripts/open-editor-windows.ps1 -EventWindows -Out <folder>`; take the whole output into a variable or a
   file.
2. The output names both windows as opened with their capture; the captures show the two dialogs as they are today.
3. The trace at the end lists any `System.Windows.Data Error` line: write each into the Done note as the baseline the
   later slices compare against (none expected; any found is a backlog line, not fixed here).
4. Run the script with no switch: the menu windows still open as before (the menu loop unchanged).
5. `git status` shows no change under `content/` (each window closed as Cancel, and the script never saves).
