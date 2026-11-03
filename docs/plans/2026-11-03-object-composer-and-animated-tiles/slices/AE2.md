# AE2. The Animate panel in the Tiles panel

Builds on design.md, "Key decisions" 6 and 9, and decisions 463, 467 and 470.

## Goal
Tiles > Animate tile... (the menu) and an "Animate" button in the Tiles tab, enabled by `Animating.CanOpen`, open the Animate panel under the sheet view: the "frames to the right" box, the time box, a live preview at the map's cell size, Apply, Remove animation, Close, and the problem line. Apply makes the cell a tile when it isn't one yet (walkable, outside the history, decision 113) and sets its animation as one undo step through AI2's `SetTileAnimations`; Remove animation makes an animated tile still, also one step; Ctrl+Z covers both. The preview cycles on WPF's render tick, through the editor's animation time this slice adds (AE3 reuses it).

## New types
- `Editor/Tiles/TileAnimationTime` (`internal sealed class`): the editor's animation time, moved on by the render tick; says when an animated tile's frame changed.
  - `TileAnimationTime(CatalogDraft catalog)`; `TimeSpan Elapsed`; `void Advance(TimeSpan delta)` (refuses a negative delta, as `FrameAt` refuses a negative time); `event EventHandler? FramesChanged` (raised once per `Advance` in which some animated tile's `FrameAt` index changed; never with no animated tile).
- `Editor/Shell/MainWindow.TileAnimation.cs` (partial; AI2 may have created it): subscribes to `CompositionTarget.Rendering`, advances `AnimationTime` by the rendering time since the last tick, and on `FramesChanged`, or when the panel's boxes change, shows `TileImages.CellImageOf(Animating.PreviewCellAt(AnimationTime.Elapsed))` in the preview. View only.

## Changes
- `Editor/Tiles/TileAnimationViewModel`: `[RelayCommand] Apply` (enabled without a problem; closes the panel) and `[RelayCommand] RemoveAnimation` (enabled on an animated tile); the class summary takes "applied or removed as one undo step".
- `Editor/Shell/MainWindow.xaml`: the panel in the Tiles tab (bound to `Animating`, under the Remove sheet block), the Animate button, Tiles > Animate tile....
- `Editor/Shell/MainWindow.xaml.cs`: starts the tick (2 lines; the file is at 569, its split is in the backlog).
- `Editor/Shell/MainViewModel`: `public TileAnimationTime? AnimationTime { get; }` (+2 lines).

## Tests
- `Tiles/TileAnimationApplyTests` (new, beside `TileAnimationViewModelTests` to keep each under 300):
  - `Apply_CellNotATileYet_MakesTheTileWithTheAnimation`.
  - `Apply_AnimatedTile_ReplacesItsCountAndTime`.
  - `Apply_WithAProblem_CannotExecute`.
  - `Apply_Undo_MakesTheTileStillAgain`.
  - `RemoveAnimation_AnimatedTile_LeavesItStill`.
  - `RemoveAnimation_StillTile_CannotExecute`.
- `Tiles/TileAnimationTimeTests` (new):
  - `Advance_WithinAFrame_RaisesNothing`.
  - `Advance_PastAFrameTime_RaisesFramesChangedOnce`.
  - `Advance_NoAnimatedTile_RaisesNothing`.
  - `Advance_NegativeDelta_Throws`.

Size: about 45 lines of XAML, 55 of code and code-behind, 10 tests (about 205).

## Verify by hand
- Choose a still cell of the waterfall B sheet (or one the import didn't animate) in the Tiles panel, click Animate: the panel opens with 4 frames and 150 ms, the preview cycling through the cell and the three to its right. Type 8 and 70: it follows at once. Type more frames than the row holds: the problem line says so and Apply greys. Apply: the panel closes; Ctrl+Z undoes it, Ctrl+Y redoes it. Open it again on that cell: its count and time come back. Remove animation makes it still.
- Save, run the game with the tile painted on a map: it animates there (AN2).
- With the panel closed, clicks on the sheet choose cells as before; Mark blocked still flips cells.
