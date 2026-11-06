# WP3. The warp destination picker window

Builds on design.md, "Key decisions" 7; WP1's `WarpDestinationViewModel` and WP2's step members. Tunables: the picker's cell size and window size.

## Goal
In both event dialogs, the warp step's row gets a **Pick...** button. It opens the picker: the map list on the left, the chosen map's picture on the right (its tiles, objects and grid, at the zoom bar's cell size, scrolling), the chosen cell outlined in cyan, under it the cell's text, the warning in amber and the problem in red, then OK and Cancel. A click on the picture chooses the cell; a double-click chooses it and presses OK. OK writes the map and cell into the step's boxes; Cancel leaves them.

## New types
- `Editor/Events/WarpDestinationWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style, 1100 x 720, resizable): the picker: the map list beside the chosen map's picture, a click choosing the cell.
  - Layout: `240 | 5 | *` columns with a `GridSplitter` as the main window's; a `ListBox` (`ItemsSource="{Binding Maps}"`, `DisplayMemberPath="Text"`, `SelectedValuePath="Id"`, `SelectedValue="{Binding MapId}"`); a `DockPanel` with a `CellZoomBar` (`DataContext="{Binding Zoom}"`) on top, the cell text, warning and problem lines and the OK and Cancel buttons at the bottom, and a `ScrollViewer` (both bars `Auto`) holding a `Grid` with a `ScaleTransform` as its `LayoutTransform` (`Zoom.ScaleFor(MapDrawing.CellPixels)`), `RenderOptions.BitmapScalingMode="NearestNeighbor"`, three layers: the map `Image`, the grid `Image`, a `Canvas` with the cyan cell `Rectangle`.
  - Code-behind (view only): on `MapId` change, `MapImage.Source = drawing.Draw(layout, focusZ: null)`; on a zoom or map change, the grid at `1 / scale` (`MapDrawing.DrawGrid`) and the scale applied (`ZoomScrolling` keeps the view's center); on `Cell` change, the rectangle placed. `MouseLeftButtonDown` on the grid: `ChooseCell(GridCell.Containing(position / CellPixels))` (`e.GetPosition` is in the grid's own pixels under a `LayoutTransform`); `ClickCount == 2`: OK. Ctrl + wheel zooms, checking the command's `CanExecute` first (`.claude/rules/editor.md`). OK sets `DialogResult = true` only when `Problem` is null.
  - `public static bool TryPick(Window owner, EventStepViewModel step, WarpMaps maps, MapDrawing drawing)`: `step.StartPickingDestination(maps)`, shown; on OK `step.TakeDestination(picker)`; true when taken.

## Changes
- `Events/EventStepEditor.xaml`: in the warp row, after the cell boxes, `<Button Content="Pick..." Margin="8,0,0,0" ToolTip="Choose the map and click the arrival cell on its picture." Click="OnPickWarpDestinationClicked" />`.
- `Events/EventStepEditor.xaml.cs`: `public static readonly RoutedEvent PickWarpDestinationEvent` (bubbling) with its CLR event `PickWarpDestination`; the click raises it from the button, whose `DataContext` is the step.
- `Events/ObjectEventsWindow` and `Events/CellEventWindow`: the constructor takes `MapDrawing drawing` as a second parameter; the root element handles `events:EventStepEditor.PickWarpDestination="OnPickWarpDestination"`, about 5 lines: `WarpDestinationWindow.TryPick(this, step, Choices.WarpMaps, _drawing)`.
- `Shell/MainWindow.EventEditing.cs`: both dialogs get `_drawing!` (2 lines changed).
- `Shell/ControlsHelpWindow.xaml`, Events: "A warp step's Pick...: choose the map on the left, click the arrival cell on its picture (double-click: choose and OK); Ctrl + wheel or − and + zoom it. A cell the player can't stand on, or one that starts an event, is warned about."

## Tests
None new: the rules are WP1's and WP2's. Guards: every `EventListViewModel` and `CellEventViewModel` test passes unchanged.

Size: XAML about 77, code-behind about 107: about 185.

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5). Click the **Events** tab, then a door object on the map (one with a Door preset; or make one: choose a door object, Events tab, click it, press **Door**).
2. In the event dialog, choose the door's event, then its "Warp to" step: the row ends with **Pick...**. Press it.
3. The picker opens on the step's map with its cell outlined in cyan (a new step: no map chosen, the red line says "Choose the map to warp to.").
4. Click another map in the list: its picture shows, unsaved changes included if you painted it earlier in the session. Use − and + (or Ctrl + wheel over the picture): the cells grow and shrink, the grid stays one pixel wide. The Tiles panel's sheet and the composer's canvas still zoom the same way (Ctrl + wheel toward the mouse, the buttons around the center): their code is now shared.
5. Click a cell: the cyan square moves there, the line reads "Cell (x, y)". Click a black (empty) cell, one inside a house's outline (Ctrl+H in the main window shows them in green) and one with a cyan square (a walk-into event): the amber line names each; they are still allowed.
6. Press **OK**: the step's row shows the map and the two numbers. Press **OK** in the event dialog, then Ctrl+S.
7. Start the game (same F5), walk through the door: you arrive on the cell you clicked.
8. Do the same from a cell event (Events tab, click a cell without an object, add a Warp step): the button works there too.

- **Done, with deviations:** the picker's `Zoom` (32 px to start) came with this slice, its first user. The cell outline's cyan is a named brush in the code-behind, as the main window's selection mark. No capture: the maps show Mana Seed art, which agents never look at (decision 438); the look is your hand check. Review: the third zooming view, so the zoom and Ctrl + wheel code of the Tiles panel's sheet, the composer's canvas and the picker moved into `Viewport/ZoomedScroller` (the backlog line done); the hand check covers zooming in all three.
