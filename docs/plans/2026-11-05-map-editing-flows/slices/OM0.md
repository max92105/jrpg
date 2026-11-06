# OM0. Moves only: the Tiles mode handlers

Builds on README, "Guiding rules" (pure moves); C1; the backlog line "`MainWindow.xaml.cs` (547 lines): move the viewport (pan, zoom, fit) and the tile painting handlers into partial files". No behavior change: the existing tests pass unchanged and none is added.

## Goal
`Shell/MainWindow.xaml.cs` (about 540 after MS0 and MS5) makes room before OM1 adds the map cell selection: `HandleToolKey`, `HandlePaintButton`, `StartDrag`, `FinishRectangle`, `SquareCorners` and `ShowBrushAt`, with the `_rectangleStart` and `_brushCell` fields and their comments, move into a new partial `Shell/MainWindow.TilePainting.cs` (summary: "The window's Tiles mode on the map: paint, erase, fill, pick, the rectangle tool, the tool keys, and the brush outline."), about 120 lines. `MainWindow.xaml.cs` ends about 420. The backlog line's tile painting half is struck (the viewport half stays).

## New types
None (one new file holding moved code).

## Tests
None new. Guard: the whole suite passes unchanged (same count before and after).

Size: about 120 lines moved, 10 of file header.

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5); Tiles tab; choose a tile.
2. Drag paints, right drag erases, R then drag fills a rectangle on release, Shift + click fills, Alt + click picks, [ and ] resize the brush (the white outline follows): all as before.
