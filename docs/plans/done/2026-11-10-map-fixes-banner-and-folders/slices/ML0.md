# ML0. Moves only: the map list into `Shell/MapListPanel`

Editor, about 110 lines moved, no behavior change. Builds on design.md, "What exists" (the map list). Frees room in `Shell/MainWindow.xaml` (298 lines) before RN3 and MF3 add to the list.

## Goal
The map list (the filter box and the list) is its own user control with its own handlers; `MainWindow.xaml` and `MainWindow.MapSwitching.cs` shrink. Nothing changes on screen or in behavior; the existing tests and a hand pass are the guard.

## New types
- `Shell/MapListPanel` (user control, `MapListPanel.xaml` + `.xaml.cs`): the map list and its filter box beside the map. DataContext: the `MainViewModel` (it binds `Navigation.Filter`, `Navigation.ShownEntries`, and calls `TryOpenMap`, `Navigation.TryOpenFirstShown`).
  - Moves in: `MapFilterBox`, `MapList`, `OnMapListSelectionChanged`, `OnMapListUpdated`, `OnMapFilterKeyDown`.
  - Members the window still needs: `public void SelectMap(string? mapId)` (the window's `OpenCurrentMap` calls it; the panel remembers the id so its own `OnMapListUpdated` reselects it, since `_shownMap` stays the window's) and `public void FocusFilter()` (Find / Ctrl+M). `Escape` in the filter gives the keys back to the map: a `FilterLeft` event the window handles by focusing `MapViewport` (the panel never reaches into the window).

## Changes
- `Shell/MainWindow.xaml`: lines 179-191 become `<local:MapListPanel x:Name="MapListPanel" FilterLeft="OnMapFilterLeft" />` (the `xmlns:local` added on the root if missing): about 13 lines out, 2 in (ends about 287).
- `Shell/MainWindow.MapSwitching.cs` (94): the three handlers leave; `OpenCurrentMap` calls `MapListPanel.SelectMap(_shownMap?.Map.Id)`; `OnGoToMapExecuted` calls `MapListPanel.FocusFilter()`.
- `docs/map.md`: the Shell row adds `MapListPanel`.

## Tests
No new rule: the existing `MapNavigationTests` and `MainViewModel*` tests pass unchanged (counted before and after in the Done note).

- **Done:** as planned. 4468 tests before and after. `MainWindow.xaml` ends at 287 lines, `MainWindow.MapSwitching.cs` at 56; the panel keeps the selected id itself, so its reselect after a rows change reads no window field.

## Verify by hand
1. The map list shows as before; clicking a row opens the map; the open map stays selected after a save and after an edit marks its row " *".
2. Ctrl+M focuses the filter box; typing narrows the list; Enter opens the first map; Esc clears it and the arrows pan the map again; Down moves into the list.
3. Alt+Left goes back; Go to from a warp step selects the destination's row.
