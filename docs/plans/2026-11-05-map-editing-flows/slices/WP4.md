# WP4. Go to a warp's destination

Builds on design.md, "Key decisions" 7; decision 508. Needs WP2 (`EventStepViewModel.Destination`), WP3 (the dialogs' drawing and routed event), MS3 (undo kept per map) and MS5 (`MainWindow.MapSwitching.cs` past the moves). Kept after the review: with maps kept open (decision 497), going to the destination loses nothing, so the devil's advocate's objection falls.

## Goal
The warp step's row gets **Go to** beside **Pick...**. It keeps the dialog's edits as OK does (a dialog with a problem stays open, saying what), then shows the destination map in the main window (opening it if needed, never asking) at 100 % centered on the destination cell, marked with a gold dashed square until the next click on the map or another map shows. A destination on the shown map just centers and marks. Alt+Left returns; Ctrl+Z on the first map still undoes the dialog's edits.

## New types
None (`WarpDestination` is WP2's).

## Changes
- `Viewport/ViewportTransform`: `public void CenterOn(Point map, Size view)`: the origin that puts a map pixel at the view's center, zoom unchanged.
- `Events/EventStepEditor`: `<Button Content="Go to" ToolTip="Keep these events, then show the destination map on its cell." .../>` raising a second bubbling routed event, `GoToWarpDestination`, as WP3's.
- `Events/ObjectEventsWindow` and `CellEventWindow`: handle it: when the dialog's `Problem` is null and the step's `Destination` isn't, `FollowedWarp = destination` and `DialogResult = true`; else the problem line shows as on OK. `public WarpDestination? FollowedWarp { get; private set; }` (view to view: the main window reads it after `ShowDialog`).
- `Shell/MainWindow.EventEditing.cs`: after applying a dialog's events, `if (window.FollowedWarp is { } warp) GoToWarpDestination(warp);` (both dialogs).
- `Shell/MainWindow.MapSwitching.cs`: `GoToWarpDestination(WarpDestination warp)`: `_viewModel.TryOpenMap(warp.MapId)`; zoom 1 at the view's center, `_viewport.CenterOn(the cell's center in map pixels, the view's size)`, `ApplyViewport()`; the mark placed and shown. It hides on the next mouse press on the map (a one-shot `MapViewport.PreviewMouseDown` handler that unsubscribes itself) and when another map shows (`OpenCurrentMap`, in this partial since MS0).
- `Shell/MainWindow.xaml`: `<Rectangle x:Name="DestinationMark" Stroke="Gold" StrokeDashArray="3 2" Visibility="Collapsed" />` in the overlay canvas (+1, ends about 278).
- `Shell/ControlsHelpWindow.xaml`, Events: "A warp step's Go to: keep the events, then show the destination map centered on its cell (gold square). Alt+Left comes back."

## Tests
- `ViewportTransformTests: CenterOn_MapPoint_PutsItAtTheViewCenter` (zoom 1).
- `ViewportTransformTests: CenterOn_AtZoomTwo_KeepsTheZoomAndCenters`.

Size: about 65 lines of code (`CenterOn` 5, editor 12, dialogs 16, main window 30, XAML and F1 2), 2 tests (about 25): about 90.

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5). Events tab, click a door with a warp, choose its warp step.
2. Change something in the dialog (the prompt), press **Go to**: the dialog closes, the destination map shows at 100 % with a gold dashed square on the arrival cell, in the middle of the view. No question is asked.
3. Press Alt+Left: you are back on the door's map. Press Ctrl+Z: the dialog's change undoes as one step.
4. Paint a cell, open the door's events again, press **Go to**, then Alt+Left: the painted cell is still there (maps stay open).
5. Clear the warp's map in the step's box, press **Go to**: the dialog stays open and its problem line says what is missing.
6. Click anywhere on the map: the gold square goes.
