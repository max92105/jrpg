# WS1. The mode bar

**Stage 3**, at row 11's rank, after rows 7-10 (decision 936).

**Goal:** two buttons at the left of the tool bar row, World and Database. Database hides the map, the side panel, the
map list, the World menus and the tool bar's bands, and shows an empty Database page; the map's keys, object commands
and undo steps are out of reach there. The Database editors still open in `CatalogDialog` from the Database menu.
Design: [design.md](../design.md), sections 2 and 3 (the undo rule's last paragraph); decisions 916, 922, 925, 927.

## New types
- `Workspaces/EditorWorkspace`: which half of the editor the main window shows (`World`, `Database`).
- `Workspaces/ModeBar` (user control): the World and Database buttons, the shown one lit (a keyed button style
  `BasedOn` the theme's `Button`, its trigger on `Tag` setting `SelectionBrush` and bold; each button's `Tag` bound to
  `IsWorldShown` or `IsDatabaseShown`, `Command` to `ShowWorldCommand` or `ShowDatabaseCommand`).
- `Workspaces/DatabaseWorkspaceView` (user control): in this slice only a centered line, "The Database editors open
  from the Database menu until they move here." WS5 replaces it.

## Changes
- **Recount first** `Shell/MainWindow.xaml` (294 on 2026-11-15, about 288 after WD1, plus whatever rows 7-10 added
  outside the Database menu): this slice adds about 8 lines; if that crosses 300, it first moves the
  `BooleanToVisibilityConverter` `VisibleWhenTrue` into `Shell/MainWindowStyles.xaml` (frees 5).
- `Shell/MainViewModel.Workspaces.cs` (new partial): `[ObservableProperty] EditorWorkspace Workspace` (private set,
  `World` at start) with `[NotifyPropertyChangedFor]` `IsWorldShown` and `IsDatabaseShown`; `[RelayCommand]
  ShowWorld()` (sets `History.OpenMap` back to the shown map, `Placing.IsMapShown` true) and `ShowDatabase()` (sets
  `History.OpenMap` null, so Ctrl+Z reaches only catalog steps and the map's wait as another map's do; `Placing.IsMapShown`
  false); `[RelayCommand] GoBackToMap()`: World first, then `Navigation.GoBackCommand`.
- `Tiles/ObjectPlacingViewModel` (its `.Selection` partial): `bool IsMapShown` (true at start), read by the delete
  command's CanExecute, which is re-raised when it changes, so Edit > Delete selected objects greys in Database mode.
- `Shell/MainWindow.xaml`: the tool bar row becomes a `DockPanel` with the `ModeBar` docked left and the `ToolBarTray`
  (its `Visibility` on `IsWorldShown`); the five-column map `Grid` and a `DatabaseWorkspaceView` (`IsDatabaseShown`)
  share one `Grid`; the View, Tiles, Objects, NPCs and Map menus get `Visibility` on `IsWorldShown` (attributes on
  their existing lines); two `KeyBinding`s, Ctrl+Shift+W and Ctrl+Shift+D; the Alt+Left binding and Map > Back to
  previous map bind `GoBackToMapCommand`; the `xmlns:workspaces` line.
- `Shell/MainWindow.xaml.cs` (268 → about 274): `OnPreviewKeyDown` returns after `base` while `IsDatabaseShown`
  (before the Space line), so no tool, object, pan or zoom key reaches the hidden map; `OnNewMapExecuted` and
  `OnGoToMapExecuted` run `ShowWorldCommand` first and stop when World isn't shown (WS3 makes that refusable).
- `Shell/MainWindow.ObjectCopying.cs`: `OnCopyCanExecute`, `OnCutCanExecute`, `OnPasteCanExecute` are false while
  `IsDatabaseShown`.
- `Shell/ControlsHelpWindow.xaml`: one line, "Ctrl+Shift+W / Ctrl+Shift+D: World (maps) / Database (the catalogs);
  the buttons at the left of the tool bar do the same."

## Tests
New file `tests/Jrpg.Editor.Tests/Shell/MainViewModelWorkspaceTests.cs` (glob it first), on `EditorTestContent`'s test
world:
- `MainViewModelWorkspaceTests: Workspace_EditorOpened_IsWorld`
- `MainViewModelWorkspaceTests: ShowDatabase_FromWorld_ShowsDatabase` (asserts `Workspace` and `IsWorldShown` false)
- `MainViewModelWorkspaceTests: ShowDatabase_FromWorld_RaisesIsWorldShownAndIsDatabaseShown`
- `MainViewModelWorkspaceTests: ShowWorld_FromDatabase_ShowsWorld`
- `MainViewModelWorkspaceTests: ShowDatabase_ObjectsSelected_GreysDeleteSelectedObjects`
- `MainViewModelWorkspaceTests: ShowWorld_ObjectsSelected_LightsDeleteSelectedObjectsAgain`
- `MainViewModelWorkspaceTests: GoBackToMap_DatabaseShown_ShowsWorldAndGoesBack` (two maps opened first)
- `MainViewModelWorkspaceTests: Undo_DatabaseShownAfterMapStroke_UndoesTheCatalogStep` (a shop set on the draft, then a
  map edit; Database shown; Undo takes the shop step and the map edit stays)
- `MainViewModelWorkspaceTests: Undo_WorldShownAgain_ReachesTheMapStep`

## Size
About 130 lines of code and XAML, 100 of tests: about 230.

## Verify by hand
1. `dotnet run --project src/Jrpg.Editor`. The tool bar row starts with World (lit) and Database.
2. In the Objects tab, select a placed object (Ctrl + click). Press Database (or Ctrl+Shift+D): the map, map list and
   side panel are gone, the menus show File, Edit, Database, Help; Edit > Delete selected objects is grey.
3. Press Delete, the arrows, B, F, Ctrl+0, Ctrl+X: nothing. Press World (Ctrl+Shift+W): the object is still there and
   selected, the view unmoved.
4. Paint a tile in World; Database; Ctrl+Z: the tile stays (no catalog step to undo: greyed); World; Ctrl+Z: the tile
   goes.
5. Database mode: Ctrl+M shows World with the map filter focused; Alt+Left shows World on the previous map; Ctrl+N
   shows World and New map (Cancel it).
6. Database > Shops... still opens the Shops dialog in either mode.
7. `.claude/scripts/open-editor-windows.ps1`: every default entry "opened" as before, no `ERROR`, no binding-error line.
- The World-mode look (tile panels) is the developer's check (Mana Seed art); an agent may screenshot the main window
  only while Database mode is shown.
