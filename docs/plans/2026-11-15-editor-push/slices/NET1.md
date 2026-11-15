# NET1. An unexpected error keeps the editor open: the policy with the same-error rule, the report file, a box naming the error, the report and what is unsaved, and the smoke script watching for it

Editor + script, about 420 lines (about 200 of them tests). Needs ADR 0050. Design: design.md, sections 1 (tunables)
and 6 (structure, key decisions 1 to 5).

The review kept this as one slice so the smoke script never runs blind to the box (decision 691). It is past the ~300
guide; if it must split at the slice step, split here and nowhere else:
- **NET1a:** `UnexpectedErrors` without the log (the repeat rule counting, not asking), `UnexpectedErrorQuestion`
  with "The report couldn't be written." for every error, `UnsavedChanges`, `App`, the test entry, the script, the
  rule and doc lines.
- **NET1b:** `UnexpectedErrorLog`, the report line, the repeat counts added on exit.

The script keys on the box's title, so it stays sighted after NET1a.

## Goal
Help > Raise a test error throws. Instead of closing, the editor writes a report and shows a box titled "Unexpected
error": the error's type and message, the report's path, what is unsaved, and "Yes: keep working. No: close the
editor (it asks about unsaved work first)." Yes keeps every open map, catalog change and undo step; No closes through
the usual close question. The same error again never asks and never writes a new file, so a render tick throwing every
frame costs one box. `open-editor-windows.ps1` reports a window whose opening raised the box as `ERROR` and checks the
net with the test entry.

## New types (all `Hosting/`, decision 691)
- `UnexpectedErrorAnswer` (enum: `KeepWorking`, `CloseEditor`): the developer's answer.
- `UnexpectedErrorReport` (record): one unexpected error and the file its report went to, if any.
  `UnexpectedErrorReport(Exception Exception, string? File)`; `File` null when the write failed.
- `UnexpectedErrorLog`: writes one exception's report to a new file and to the error output.
  - `UnexpectedErrorLog(string folder, string contentRoot, TextWriter errorOutput)`.
  - `string? Write(Exception exception, DateTimeOffset at)`: the report goes to `errorOutput` first, then to
    `editor-error-yyyy-MM-dd-HHmmss-fff.txt` (`-2`, `-3` while the name is taken) in the folder, created when missing;
    returns the path, or null on `IOException` or `UnauthorizedAccessException` (the report never raises an error of
    its own). The text: "Editor error at {at:O}", "Content: {contentRoot}", a blank line, `exception.ToString()`.
  - `void AddRepeatCount(string file, int count)`: appends "Came back once more." (1) or "Came back {count} more
    times." to a report, under the same catch.
- `UnexpectedErrors`: decides, for each exception nothing caught, whether it is written and whether the developer is
  asked.
  - `UnexpectedErrors(UnexpectedErrorLog log, Func<UnexpectedErrorReport, UnexpectedErrorAnswer> ask)`.
  - `UnexpectedErrorAnswer Handle(Exception exception, DateTimeOffset at)`:
    - the same error as one already reported (type and `TargetSite`, decision 690): counted, nothing written, nothing
      asked, `KeepWorking`;
    - else written; then asked unless a question is open (decision 670: `KeepWorking` without asking). The "asking"
      mark is cleared in a `finally`, so a throwing `ask` doesn't block later questions.
  - `void AddRepeatCounts()`: for each error counted again, `log.AddRepeatCount(file, count)` (an error whose report
    wasn't written has no file and is skipped).
- `UnexpectedErrorQuestion`: the message box's text: the error, its report and what is unsaved.
  - `const string Title = "Unexpected error"`.
  - `static string Text(UnexpectedErrorReport report, string? unsavedChanges)`: "The editor hit an error it didn't
    expect: {type name}: {message}", "Report: {file}" or "The report couldn't be written.", the unsaved changes or
    "Nothing is unsaved.", "A window that was open may lose what wasn't applied.", "Yes: keep working, and save with
    Save all (Ctrl+Shift+S); if something looks wrong after, save and restart. No: close the editor (it asks about
    unsaved work first)."

## Changes
- `Hosting/App.xaml.cs`:
  - `OnStartup` builds the log over `Path.Combine(Environment.GetFolderPath(SpecialFolder.LocalApplicationData),
    "Jrpg", "Editor", "Errors")`, the content root and `Console.Error`, then `UnexpectedErrors`, and subscribes
    `DispatcherUnhandledException` before building the window; it keeps the `MainViewModel` in a field.
  - The handler passes `DateTimeOffset.Now` (the editor isn't Core; the wall clock is allowed, as the game's save
    stamp). While `MainWindow is not { IsLoaded: true }`, it writes the report and leaves `Handled` false: a startup
    throw ends the editor as today (decision 673). Otherwise `e.Handled = true`, `Handle`, and on `CloseEditor`
    `Dispatcher.BeginInvoke(MainWindow.Close)`, so the handler returns before the close question opens.
  - `ask` is a private static method showing `MessageBox.Show(UnexpectedErrorQuestion.Text(report,
    _viewModel.UnsavedChanges), UnexpectedErrorQuestion.Title, MessageBoxButton.YesNo, MessageBoxImage.Error)`.
  - `OnExit` calls `AddRepeatCounts()`.
- `Shell/MainViewModel.Saving.cs`: the private `UnsavedChangesQuestion()` becomes the public read-only property
  `UnsavedChanges` (null when nothing is unsaved), read by `ConfirmClose` as before and by `App`; its summary says
  both read it.
- `Shell/MainWindow.xaml`: under Help, `<MenuItem Header="Raise a test error" Click="OnRaiseTestErrorClicked"
  ToolTip="Throws an error on purpose, to check that the editor keeps your work." />` (292 -> 293 lines).
  `Shell/MainWindow.xaml.cs`: `OnRaiseTestErrorClicked` throws `InvalidOperationException("Test error from Help > Raise
  a test error.")` (decision 674, every build).
- `.claude/scripts/open-editor-windows.ps1`:
  - after invoking an entry, a dialog named `Unexpected error` prints `"$entry: ERROR (the editor's unexpected-error
    box; its report is in the trace below)"` and is answered by invoking its `Yes` button (found by name under the
    dialog), then the next entry runs; no screenshot of it;
  - `'Help>Raise a test error'` is last in the default `-Windows` list; for it the box is expected and prints `"... :
    the net caught it"`, anything else `"... : THE NET DID NOT CATCH IT"`;
  - the header comment: `ERROR` means the net caught a throw, `CRASHED` a throw before the main window loaded; the trace
    at the end is every report the editor wrote to the error output.
- `.claude/rules/editor.md`: the endings "The editor has no unhandled-exception handler, so a throw closes it and loses
  the draft" and "the editor has no unhandled-exception handler" become "a throw stops the command halfway and opens
  the unexpected-error box (ADR 0050)"; the last rule's script sentence gains "; an `ERROR` line names a window whose
  opening threw".
- `docs/plans/backlog.md`: the line "The editor has no `DispatcherUnhandledException` handler..." removed.
- `docs/map.md`: a `Hosting` row in the Editor table (missing today): "Startup and the unexpected-error net (ADR 0050)
  | `App`, `UnexpectedErrors`, `UnexpectedErrorLog`, `UnexpectedErrorQuestion`".

## Tests
`Hosting/UnexpectedErrorsTests` (new), the log over a temporary folder made in the test, each `ask` built in the test,
each exception thrown and caught in a local method so it has a `TargetSite`:
- `Handle_AnError_WritesThenAsksWithTheReport` (the file exists when `ask` runs).
- `Handle_DeveloperKeepsWorking_ReturnsKeepWorking`.
- `Handle_DeveloperCloses_ReturnsCloseEditor`.
- `Handle_OtherErrorWhileAsking_WritesWithoutAsking` (an `ask` that calls `Handle` with another type: two files, one
  question).
- `Handle_SameErrorAfterAnswer_DoesNotAskAgain`.
- `Handle_SameErrorAfterAnswer_WritesNoSecondReport`.
- `Handle_SameErrorWhileAsking_WritesNoSecondReport` (the render tick's case).
- `Handle_SameTypeFromAnotherMethod_Asks` (two local methods throwing the same type).
- `Handle_AskThrows_AsksTheNextOne` (the `finally`).
- `AddRepeatCounts_SameErrorThreeTimes_AddsTwoMoreToItsReport`.
- `AddRepeatCounts_SameErrorTwice_SaysOnceMore`.

`Hosting/UnexpectedErrorLogTests` (new), over a temporary folder deleted in `Dispose`:
- `Write_AnError_WritesItsTypeMessageAndStack`.
- `Write_AnError_NamesTheContentRootAndTime` (a fixed `DateTimeOffset`; the file name pinned).
- `Write_SameMillisecondTwice_WritesTwoFiles` (the second ends `-2`).
- `Write_FolderMissing_CreatesIt`.
- `Write_FolderIsAFile_ReturnsNullAndStillWritesTheOutput` (a `StringWriter` output holds the report).

`Hosting/UnexpectedErrorQuestionTests` (new):
- `Text_ReportWrittenAndChangesOpen_NamesTheErrorFileAndChanges`.
- `Text_ReportNotWritten_SaysSo`.
- `Text_NothingUnsaved_SaysSo`.

`Shell/MainViewModelSavingTests` (186 lines, about 210 after), over its existing setup:
- `UnsavedChanges_MapAndTilesChanged_NamesEach`.
- `UnsavedChanges_NothingChanged_IsNull`.
- The `ConfirmClose_*` tests pass unchanged (the rename's guard).

## Verify by hand
The developer, Debug build (`dotnet run --project src/Jrpg.Editor`; from Visual Studio, F5 stops in the debugger on the
throw first: Continue, or run with Ctrl+F5):
1. Paint one tile; the map's row shows " *".
2. Help > **Raise a test error**: the "Unexpected error" box names "InvalidOperationException: Test error from Help >
   Raise a test error.", a file under `%LocalAppData%\Jrpg\Editor\Errors\` and "Unsaved changes in map '...'".
3. **Yes**: the editor is as it was, tile and " *" included; Ctrl+Z removes the tile.
4. Help > Raise a test error again: no box this time (the same error). Open the errors folder: still one report.
5. Close the editor normally: the close question names the map; **Don't save**. The report now ends "Came back once
   more."
6. Restart, Raise a test error, **No**: the close question appears; **Cancel** keeps the editor open.

Claude, at the slice step (not committed):
7. Add `throw new InvalidOperationException("render test");` at the top of `MainWindow.OnRendering`, build, run: one
   box; Yes; no further box while the editor runs; File > Save all works (paint a tile first, then check its file
   changed); the errors folder holds one new report. Revert the line.
8. Build, then `powershell -NoProfile -ExecutionPolicy Bypass -File .claude/scripts/open-editor-windows.ps1`: every
   window prints "opened", the last line "Help>Raise a test error: the net caught it".
9. Give `ShopCatalogWindow.xaml` a broken value (`Width="abc"`) temporarily, rebuild, run the script with `-Windows
   'Database>Shops...'`: it prints `ERROR` with the XAML parse error in the trace. Revert.
