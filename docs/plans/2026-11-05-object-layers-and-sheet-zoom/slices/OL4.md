# OL4. The Level box in the composer; the levels above the chosen one faded

Builds on design.md, "Key decisions" 5; decisions 478 and 479. The window part of OL3's rules.

- **Done, with deviations:** `Draw_TwoLevelsOnOneCell_DrawsTheUpperLast` left at review: OL3's `Draw_LevelOnePaintedBeforeTheBase_DrawsTheBaseFirst` covers the order, painted the harder way round.

## Goal
The composer window shows a Level box over the canvas (the painting's `LevelChoices`, the chosen one painted and erased, one empty level always offered on top). The canvas draws every level bottom first, those above the chosen level faded, so the developer sees what will cover the cells being painted. F1 lists it. This slice's hand check makes the first stacked object, gives it an above-player zone, and looks at it in the editor and the game, the canopy bake of OL2 included.

## New types
None.

## Changes
- `Editor/Objects/Composing/ObjectCanvasDrawing`: `Draw(ObjectCanvas canvas, int chosenLevel, Func<SheetCell, ImageSource> imageOf)` (if SZ3 ran first, its `gridLineWidth` stays the last parameter); levels drawn from the base up, each above `chosenLevel` inside `PushOpacity(UpperLevelOpacity)`; `private const double UpperLevelOpacity = 0.4` (placeholder look). Summary: the levels above the chosen one are faded.
- `Editor/Objects/Composing/ObjectComposerWindow.xaml`: a row under the size boxes: a `Level` label and a `ComboBox` (`ItemsSource` `Painting.LevelChoices`, `SelectedIndex` `Painting.Level`, the theme's combo box as the category box uses; tool tip "The level strokes paint and erase: 0 is the base, higher levels are drawn over it. The last one is empty, ready to paint."). The hint line over the canvas ends "...stamp it on the chosen level".
- `ObjectComposerWindow.xaml.cs`: `OnPaintingChanged` redraws on `Level` too; `DrawCanvas` passes `Painting.Level`.
- `Editor/Shell/ControlsHelpWindow.xaml`, Objects: "In the composer, the Level box chooses the level strokes paint and erase (0 is the base; higher levels are drawn over it, those above the chosen one faded; the last level offered is empty, ready to paint)."

## Tests
- `ObjectCanvasDrawingTests: Draw_LevelAboveTheChosenOne_IsFaded` (its image sits in a group of opacity `0.4`).
- `ObjectCanvasDrawingTests: Draw_ChosenAndLowerLevels_AreDrawnWhole` (the other side: their images aren't under an opacity group).
- `ObjectCanvasDrawingTests: Draw_TwoLevelsOnOneCell_DrawsTheUpperLast`.
- The existing `ObjectCanvasDrawingTests` gain the `chosenLevel` argument (3 lines).

Size: about 25 lines of code and 15 of XAML, 3 tests (about 45): about 85.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `Composing/ObjectCanvasDrawing` gains "the levels above the chosen one faded".

## Hand check (yours)
The first stacked object, from the half-timber sheet's wall and a window or door (any wall and any part with transparent pixels will do).
1. Start the editor ("Game and editor" profile, F5).
2. Tiles tab: in the sheet box choose the half-timber house sheet; drag over a 2 x 2 block of wall cells (a dashed rectangle).
3. Objects > **New object...**: the composer opens with the wall painted; the Level box shows "0 (base)" and lists "0 (base)" and "1".
4. Choose "1" in the Level box. In the composer's picker (left), click a window or door cell; click the top-left canvas cell: the window shows over the wall, the wall visible through its clear pixels; the Level box now also lists "2".
5. Press Ctrl+Z: the window goes, the box still shows "1" and no longer lists "2". Click the top-left canvas cell again: the window comes back, with no error.
6. Choose "0 (base)" in the Level box: the window is drawn faded over the wall.
7. Right-click the top-left canvas cell (still on level 0): the wall cell under the window goes, the faded window stays. Press Ctrl+Z: the wall cell is back.
8. Type an id (for example `wall_window_test`) and a category; press **Save**. The Shapes window opens.
9. In the Shapes window, click **Over the player (leaves, roofs)**, then **New zone**. Hold Shift and drag from the object's top-left corner to its right edge, halfway down (the line between its two rows): a blue rectangle covers the top row.
10. Click **Collision (blocks the player)**, hold Shift and drag over the bottom row: a green rectangle. Press **OK**.
11. Objects tab: the preview shows the window over the wall. In the map list on the left, open the map the game starts on; with the object chosen in the Objects tab, click the map about three cells above where the player appears at the start of a new game: the placed object shows the window over the wall.
12. Objects > **Edit object...**: the Level box lists "0 (base)", "1" and "2", the window on 1. Press **Cancel**.
13. Press Ctrl+S.
14. Start the game from the same profile; from the title, start a new game (any leader). Walk up to the object: the window over the wall, as in the editor; its bottom row blocks you.
15. Walk around it and up behind it, so your character stands on the cells behind its top row: the top row is drawn over your character, the wall showing through the window's clear pixels, with no hole where the window is.
16. If you don't keep the object: in the editor, right-click the placed object on the map, choose it in the Objects tab and press **Remove**, then Ctrl+S.
17. Press F1 in the editor: the Objects section has the Level line.
