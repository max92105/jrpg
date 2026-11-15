# NET2 (optional). A custom error window: Keep working, Close editor..., Show report, Copy details

Editor, about 200 lines (about 60 of them tests). Needs NET1. **Only if the developer asks for it after using NET1's
box** (decision 691); otherwise this slice is dropped at the retro. Design: design.md, section 6 (extension points).

## Goal
The message box gives way to a dark-themed window titled "Unexpected error" (the same title, so the smoke script keeps
finding it): the headline, the error, the report line, what is unsaved, the full details folded under an expander, and
four buttons. **Keep working** (the default, Enter and Esc, and the title bar's X) closes it; **Close editor...**
closes it and then the editor through the close question; **Show report** opens Explorer with the report selected
(greyed when there is none); **Copy details** puts the details on the clipboard.

## New types (in `Hosting/`, so `Shell` never reads `Hosting`)
- `Hosting/UnexpectedErrorViewModel`: the error window's lines: the error, its report line, what is unsaved, the
  details. Built from `UnexpectedErrorReport` and `MainViewModel.UnsavedChanges`; its lines are
  `UnexpectedErrorQuestion`'s, which it reuses rather than copies (the box's yes/no sentence stays the box's).
- `Hosting/UnexpectedErrorWindow` (`.xaml` + `.xaml.cs`): the error window: its four buttons.
  - `public static UnexpectedErrorAnswer Ask(UnexpectedErrorViewModel viewModel, Window? owner)`: modal, `Owner` the
    active window when there is one, so it opens over a Database window that threw.
  - Style `EditorWindow`, `SizeToContent="Height"`, `Width="560"` (a `double` literal, never an `x:Static` int); the
    details in a read-only `TextBox` with `MaxHeight="200"` under a closed `Expander`.
  - Show report: `Process.Start("explorer.exe", $"/select,\"{file}\"")`.
  - Copy details: `Clipboard.SetText` inside `catch (ExternalException)`, which sets "The clipboard is busy; try
    again." under the buttons: the window never throws into the net it belongs to.

## Changes
- `Hosting/App.xaml.cs`: `ask` shows the window instead of the box.
- `UnexpectedErrorQuestion`: the parts the window shows become separate members (`Failure`, `ReportLine`,
  `UnsavedLine`) beside `Text`, which joins them, so the window and any later box share one wording.
- `.claude/scripts/open-editor-windows.ps1`: answers by the window's `Keep working` button instead of the box's `Yes`.

## Tests
`Hosting/UnexpectedErrorViewModelTests` (new):
- `Details_InnerException_IncludesIt`.
- `HasReportFile_ReportNotWritten_IsFalse`.
`Hosting/UnexpectedErrorQuestionTests` (NET1's): `Failure_AnError_NamesItsTypeAndMessage`,
`ReportLine_ReportWritten_NamesTheFile`, `UnsavedLine_NothingUnsaved_SaysSo`; the `Text_*` tests pass unchanged.

## Verify by hand
1. Paint a tile; Help > Raise a test error: the dark window shows the error, the report and the unsaved map.
2. Details: the stack scrolls. Copy details, paste into Notepad: the same text. Show report: Explorer selects the file.
3. Keep working (and Esc, and the X): the editor is as it was. Close editor...: the close question; Cancel stays.
4. Run `open-editor-windows.ps1`: every window opens; the test entry's line says the net caught it.
