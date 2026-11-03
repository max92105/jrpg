# AE3. The Animate panel in the Tiles panel

Builds on design.md, "Key decisions" 6 and 9, and the README's pick 13.

## Goal
Tiles > Animate tile... (the menu) and an "Animate" button in the Tiles tab, enabled by `Animating.CanOpen`, open the Animate panel under the sheet view: the frames count and status line, the time box, a live preview at the map's cell size, Remove last frame, Apply, Remove animation, Close, and the problem line. While it is open, a click on the sheet adds the cell under it as the next frame (instead of choosing it), and the frames are numbered over the sheet (1, 2, 3 in their cells). The preview cycles with the editor's view timer, which this slice adds (AE4 reuses it for the map).

## New types
- `Editor/Tiles/AnimationFrameMarks` (static): the open panel's frames numbered over the sheet. `static DrawingImage? Draw(TilesheetDefinition sheet, Size imageSize, IReadOnlyList<SheetCell> frames)` (null with no panel open), in the screen-pixel line widths `TilesheetMarks` uses.
- `Editor/Shell/MainWindow.TileAnimation.cs` (partial): the view timer moving the editor's animation time on: one `DispatcherTimer` at 50 ms adding its interval to a `TimeSpan`, showing the panel's preview (`TileImages.CellImageOf(Animating.PreviewCellAt(time))`). View only.

## Changes
- `Editor/Tiles/TileBrowserView.xaml(.cs)`: `TileAnimationViewModel? Animating` (set by the window, as `Marking`); while `Animating.IsOpen`, a click calls `Animating.AddFrameAt` with the browser's `CellAt` and no drag starts; a third image layer `FrameMarks` redrawn when the frames change (about +25 lines of code-behind).
- `Editor/Shell/MainWindow.xaml`: the panel in the Tiles tab (bound to `Animating`, under the Remove sheet block), the Animate button, Tiles > Animate tile....
- `Editor/Shell/MainWindow.xaml.cs`: sets `TileBrowser.Animating` and starts the timer (2 lines; the file is at 569, its split is in the backlog).
- `Editor/Shell/MainViewModel`: `public TileAnimationViewModel? Animating { get; }`, built beside `Marking` (+3 lines after phase 1's −40).
- `Editor/Shell/ControlsHelpWindow`: "With the Animate panel open, a click on the sheet adds a frame."

## Tests
- `Tiles/AnimationFrameMarksTests` (new): `Draw_NoFrames_IsNull`; `Draw_ThreeFrames_IsClippedToTheImage` (the drawing's bounds equal the image's, as `TilesheetMarks` pins).
- The panel's behavior is AE1's and AE2's tests; the rest is the hand check.

Size: about 60 lines of XAML, 70 of code-behind and drawing, 10 elsewhere, 2 tests (about 200).

## Verify by hand
- Choose the first frame cell of a Mana Seed water or waterfall sheet in the Tiles panel, click Animate: the panel opens with frame 1 marked on it. Click the next cells in order: 2, 3, 4 appear; the preview cycles. Type 300: it slows down. Click a cell of another sheet (switch the sheet box): the status says frames must be on the tile's own sheet. Remove last frame takes 4 off; the first can't be removed. Apply: the panel closes; Ctrl+Z undoes it, Ctrl+Y redoes it. Open it again on that cell: the frames and time come back. Remove animation makes it still.
- Save, run the game with the tile painted on a map: it animates there (AN2).
- With the panel closed, clicks choose cells as before; Mark blocked still flips cells. F1 lists the panel's gesture.
