# MS0. Moves only: map switching and the window's styles

Builds on README, "Guiding rules" (pure moves, files that must not grow); C1. No behavior change: the existing tests pass unchanged and none is added.

## Goal
Three files past or near 300 lines make room before phase 1 grows them:
1. `Shell/MainViewModel.cs` (484): `MapIds`, `MapEntries`, `TryOpenMap`, `StartNewMap`, `TryCreateMap`, `HasUnsavedChanges`, `OnMapIdsChanged`, `OnCurrentMapChanged`, `ShowMapList`, `ConfirmLeaveCurrentMap` and `ConfirmDiscard`, with their summaries, move into a new partial file `Shell/MainViewModel.MapSwitching.cs` (summary: "Which maps are open and shown: the map list, opening and making maps."), about 115 lines. `ConfirmClose` and `Save` stay (MS4 moves them to `MainViewModel.Saving.cs`). `MainViewModel.cs` ends about 370.
2. `Shell/MainWindow.xaml.cs` (571): `OnMapListSelectionChanged`, `OpenCurrentMap` and the body of `OnViewModelChanged`'s `MapEntries` case (as a method `ReselectShownMap()`, the case calling it) move into a new partial `Shell/MainWindow.MapSwitching.cs` (summary: "The map list: choosing and reselecting the shown map; later its filter, Back and Go to's arrival."), about 30 lines. `MainWindow.xaml.cs` ends about 545.
3. `Shell/MainWindow.xaml` (296): the `Window.Resources` styles and panel sizes (`MapListWidth` to `WarningLine`) move into a new resource dictionary `Shell/MainWindowStyles.xaml`, merged by the window (`<ResourceDictionary.MergedDictionaries>`); `BooleanToVisibilityConverter` stays with the window. `MainWindow.xaml` ends about 263.

## New types
None (three new files holding moved code).

## Changes
Only the moves above; `using` lines follow their code. Comments move with their members unchanged.

## Tests
None new. Guards: the whole `Jrpg.Editor.Tests` suite passes unchanged (same count before and after).

Size: about 170 lines moved, about 15 lines of file headers and the merged dictionary.

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5).
2. The window looks as before: the Tiles tool bar shows on the Tiles tab only, the Objects tool bar on the Objects tab; the side panel and the map list keep their widths and minimums; an error line (break a number in Map > Settings) is red.
3. Click another map in the list: it opens (the old question still comes when the map has changes, until MS1).

- **Done:** as planned. `MainViewModel.cs` ends at 390 lines, `MainWindow.xaml.cs` at 546, `MainWindow.xaml` at 261; the test count is unchanged (4099).
