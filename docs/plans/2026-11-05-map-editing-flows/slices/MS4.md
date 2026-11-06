# MS4. Save all, and the close question

Builds on design.md, "Key decisions" 4 and Risks (Save all stops halfway); decisions 499, 500 and 501; C9. Needs MS1.

## Goal
File > **Save all** (Ctrl+Shift+S) writes the tiles and objects when changed, then every open map with changes in id order; the first map the game would refuse stops it, its reason in the error line, the maps written before staying saved (their " *" goes). Ctrl+S keeps saving the shown map and the tiles, and with no map shown still writes the tiles and objects. Closing the editor with anything unsaved asks once, in a small dark window listing what is unsaved: **Save all** (Enter), **Don't save**, **Cancel** (Esc); Save all closes only when everything was written. With nothing unsaved, it closes without asking.

## First: the move
`Save` (the command) and `ConfirmClose` move from `Shell/MainViewModel.cs` into a new partial `Shell/MainViewModel.Saving.cs` (summary: "Save, Save all and the close question."), about 45 lines; `MainViewModel.cs` ends about 325.

## New types
- `Editor/Shell/UnsavedChangesAnswer` (`internal enum`): the developer's answer when closing with unsaved changes: `SaveAll`, `Discard`, `Cancel`.
- `Editor/Shell/SaveChangesWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style): the dark Save all / Don't save / Cancel question.
  - `public static UnsavedChangesAnswer Ask(string question)`: shown over `Application.Current.MainWindow`; the title bar's X is Cancel; Save all is the default button, Cancel the cancel button.

## Changes
- `MainViewModel` constructor: a third parameter, `Func<string, UnsavedChangesAnswer> askToClose` ("asks what to do with unsaved changes when the editor closes"), kept in a field; `confirm`'s doc loses "such as whether to drop unsaved changes".
- `Shell/MainViewModel.Saving.cs`:
  - `Save()`: writes the catalogs when dirty (also with no map shown: C9), then the shown map when there is one; marks saved as today; `ShowMapList` after (the marks).
  - `[RelayCommand] void SaveAll()`: the catalogs when dirty, then each of `_openMaps.Dirty` (id order) through the same per-map write (`WriteMap`, `AddSavedMap`, `MarkSaved`); stops at the first refusal with `SaveError` set; `Art.Refresh()` once at the end.
  - `ConfirmClose()`: nothing unsaved: true; else asks `"Unsaved changes in map 'forest', map 'start' and the tiles and objects."` (only the parts that are unsaved); SaveAll: `SaveAll()` then true only when `SaveError` is null; Discard: true; Cancel: false. The stroke being dragged ends first, as today.
- `Hosting/App.xaml.cs`: `new MainViewModel(contentRoot, Shell.MainWindow.Ask, SaveChangesWindow.Ask)`.
- `tests/Jrpg.Editor.Tests/Shell/EditorTestContent`: `Open(root, bool answer = false)` also passes a close asker that records its question in `Questions` and answers `Discard` for true, `Cancel` for false; a new `Open(string contentRoot, UnsavedChangesAnswer closeAnswer)`.
- `Shell/MainWindow.xaml`: File menu `<MenuItem Header="Save all" Command="{Binding SaveAllCommand}" InputGestureText="Ctrl+Shift+S" />` and the key binding (+2, ends about 265).
- `Shell/ControlsHelpWindow.xaml`, Anywhere: "Ctrl+Shift+S: save every changed map and the tiles (File > Save all). Closing with unsaved changes: Save all, Don't save or Cancel."

## Tests (new `Shell/MainViewModelSavingTests`; the `ConfirmClose_*` tests leave `MainViewModelTests`, re-pinned to the new question)
- `Save_NoMapShownWithUnsavedTiles_WritesTheTiles` (C9's guard: `tiles.json` changed on disk, `Content.IsDirty` false).
- `SaveAll_TwoMapsChanged_WritesBothAndClearsTheMarks`.
- `SaveAll_OnlyTilesChanged_WritesTheTiles`.
- `SaveAll_SecondMapRefused_KeepsTheFirstSavedAndShowsTheReason` (ids "a" then "b": "a" written and clean, "b" dirty, `SaveError` naming "b"'s file).
- `SaveAll_NewMapOpen_WritesItAndListsItWithoutAMark`.
- `ConfirmClose_NothingUnsaved_ClosesWithoutAsking`.
- `ConfirmClose_TwoMapsAndTilesChanged_AsksOnceNamingEach` (the question's text pinned).
- `ConfirmClose_UserSavesAll_WritesAndCloses`.
- `ConfirmClose_UserSavesAllAndAMapIsRefused_StaysOpen`.
- `ConfirmClose_UserDiscards_ClosesWithoutWriting` (the forest file's bytes unchanged).
- `ConfirmClose_UserCancels_StaysOpen`; `ConfirmClose_MidStroke_Asks` (re-pinned).

Size: about 60 lines of code, the window about 45 (XAML 30, code 15), the helper 10, XAML and F1 3; 12 tests (about 120, of which about 30 re-pinned): about 240, besides the 45-line move. The tests are short; the window has no logic.

## Map
`docs/map.md`, Editor `Shell` row: `SaveChangesWindow` (Save all, Don't save or Cancel on closing), `MainViewModel.Saving` (Save, Save all, the close question).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5).
2. Paint a cell on map A and on map B (both get " *"). File > Save all (or Ctrl+Shift+S): both marks go; Visual Studio's Git Changes lists both layout files.
3. Paint on A and B again, then File > Exit: the dark window lists map A and map B. **Cancel**: the editor stays. Exit again, **Don't save**: it closes; Git Changes shows nothing new.
4. Start again, paint on A, mark a tile blocked, Exit, **Save all** (or Enter): it closes; Git Changes lists A's layout and `tiles.json`.
5. Start again, mark a tile blocked with no map shown (or with a fresh map), Ctrl+S: `tiles.json` is written.
