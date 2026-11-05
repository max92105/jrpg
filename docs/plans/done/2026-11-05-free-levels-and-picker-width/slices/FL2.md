# FL2. The composer paints any level from -7 to 7

Builds on design.md, "Key decisions" 2, 3, 5 and 6; decisions 490, 492 and 493. Needs FL1 (`ObjectTile.MinLevel`; Data accepting a negative level at Ctrl+S).

- **Done, with deviations:** `Level_SetOnAnEmptyLevelAboveTheBase_IsChosen` became `PaintAt_OnAnEmptyLevelAboveTheBase_StampsThere` at review: it paints on 5 over an empty canvas, not only sets the level.

## Goal
The composer's Level box lists every level, "7" at the top down to "-7" at the bottom, "0 (base)" chosen when the composer opens. The developer chooses any level at any time, empty or not, -1 included; strokes paint and erase on it; an undo or redo never changes the choice. The canvas draws a negative level under the base, and with -1 chosen the base and everything over it are faded (the drawing already compares levels as numbers). Save writes the tiles lowest level first, so a -1 tile is listed before the base. The painted levels are not marked yet (FL3).

## New types
- `Editor/Objects/Composing/LevelChoice` (`internal sealed class LevelChoice(int level)`): one entry of the composer's Level box: a level and how the box names it. `int Level`; `string Name` ("0 (base)" for 0, else the number, invariant culture, so "-1"); `static IReadOnlyList<LevelChoice> TopDown()`: one entry per level from `ObjectTile.MaxLevel` down to `ObjectTile.MinLevel`. A class, not a record: FL3 makes it observable (its marks), and no caller compares two entries.

## Changes
- `Editor/Objects/Composing/ObjectCanvas`:
  - `ThrowIfOutsideTheLevels`: `ThrowIfLessThan(level, ObjectTile.MinLevel)` instead of `ThrowIfNegative`; the two `<exception>` docs say "outside `ObjectTile.MinLevel` to `ObjectTile.MaxLevel`".
  - `HighestLevel` removed: its only reader, the choices' rebuild, leaves (design.md, "Key decisions" 6).
- `Editor/Objects/Composing/CanvasPainting`:
  - `Level` becomes `[ObservableProperty] public partial int Level { get; set; }` (0 at the start): the level strokes paint and erase, from `ObjectTile.MinLevel` to `MaxLevel`; the hand-written setter, its `_level` field and its -1 guard go (-1 is a level now; the box can only push a level of its fixed list).
  - `LevelChoices` becomes `public IReadOnlyList<LevelChoice> LevelChoices { get; } = LevelChoice.TopDown();`, made once, never replaced; `BaseLevelChoice` goes.
  - `OnCanvasChanged` (the rebuild and the clamp) goes; the summary's "on the chosen level (decision 478)" names the free choice instead.
  - `using Jrpg.Core.Maps` goes if nothing else in the file reads it (IDE0005).
- `Editor/Objects/Composing/ObjectComposerWindow.xaml`, the Level box: `SelectedValue="{Binding Painting.Level}"`, `SelectedValuePath="Level"`, `DisplayMemberPath="Name"` (instead of `SelectedIndex`); width 110; tool tip "The level strokes paint and erase, from -7 to 7: 0 is the base, a higher level is drawn over a lower one." The comment over it: bound by value to a list that never changes, so the box never clears its choice.
- `Editor/Shell/ControlsHelpWindow.xaml`, Objects: "In the composer, the Level box chooses the level strokes paint and erase, from -7 to 7 (0 is the base; a higher level is drawn over a lower one; the levels above the chosen one are faded)."
- `docs/plans/backlog.md`, "From the object levels and sheet zoom plan (at approval)": "a level above an empty one" struck from the composer levels line (this plan delivers it).
- `ObjectCanvasDrawing` unchanged: it orders by level and fades `at.Level > chosenLevel`, which is right for negative levels; no test is added there, since one would pass today (OL2's day-one pins left for that reason).

## Tests
- `ObjectCanvasLevelTests`:
  - `Stamp_OutsideTheLevels_Throws`: rows re-pinned to `ObjectTile.MinLevel - 1` and `ObjectTile.MaxLevel + 1` (the -1 row is a level now).
  - `Stamp_AtEachBound_PaintsIt` (theory: `ObjectTile.MinLevel`, `ObjectTile.MaxLevel`; the cell is in `Cells` on that level): replaces `Stamp_OnTheHighestLevel_PaintsIt`, which read `HighestLevel`.
  - `Stamp_UnderTheBase_KeepsTheBaseCellOver` (the wall on 0, a cell on -1 on the same cell: both kept).
  - `TilesOf_LevelUnderTheBase_ListsItFirst` (the base stamped first, then -1, the opposite of the expected order).
  - `HighestLevel_Blank_IsMinusOne` removed with `HighestLevel`.
- `CanvasPaintingLevelTests` (the file's summary: the chosen level, any from -7 to 7):
  - `Level_New_IsTheBase`.
  - `LevelChoices_New_ListsEveryLevelFromTheTopDown` (fifteen; the first is 7, the last -7, the base's name "0 (base)").
  - `PaintAt_OnLevelMinusOne_StampsUnderTheBaseCell` (replaces `Level_SetToMinusOne_KeepsTheChosenLevel`: the rule it pinned is gone by the developer's decision).
  - `Level_SetOnAnEmptyLevelAboveTheBase_IsChosen` (5 on a blank canvas; replaces `Level_SetPastTheChoices_KeepsTheChosenLevel`, same reason).
  - `Undo_OnlyStrokeOnTheChosenLevel_KeepsTheLevelChosen` (paint on 3, undo: the stroke is gone, `Level` is still 3; replaces `Undo_StrokeThatOpenedTheChosenLevel_ChoosesTheHighestLevelLeft`).
  - `PaintAt_OnLevelOne_StampsOverTheBaseCell` unchanged.
  - Removed with the rule they pinned (one empty level on top, decision 478 replaced): `LevelChoices_BlankCanvas_IsTheBaseOnly`, `LevelChoices_PaintedOnLevelOne_EndsWithAnEmptyLevelTwo`, `LevelChoices_PaintedOnTheHighestLevel_StopsAtIt`, `LevelChoices_StrokeOnTheTopLevel_RaisesPropertyChanged`. FL3's marks tests take their place.
- `ObjectComposingLevelTests`:
  - `TryApply_TileUnderTheBase_SavesItFirst` (the test world's ground tile painted on 0, then a city tile on -1 on the same cell: the saved tiles are `(0, 0, city, -1)` then `(0, 0, ground)`).
  - `StartEdit_StackedObject_OffersEveryLevelAndOneAbove` becomes `StartEdit_StackedObject_ChoosesTheBaseWithTheUpperTileOnItsLevel` (its choices assertion goes with the rule; the level-2 cell assertion stays; `Level` is 0).
- Guards that must pass unchanged: every `ObjectCanvasTests`, `CanvasPaintingTests`, `ObjectCanvasDrawingTests`, `ObjectComposingTests`, `ObjectComposerViewModelTests` case.

Size: about 60 lines of code and XAML (`LevelChoice` about 25, `CanvasPainting` about 25 changed, net shorter, `ObjectCanvas` 5, XAML and help 6), tests about 115 changed or new: about 175.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `Composing/CanvasPainting` "on the chosen level, one empty level offered on top" becomes "on the chosen level, any from -7 to 7"; `Composing/LevelChoice` (an entry of the composer's Level box: a level and its name) added after it.

## Hand check (yours)
A wall with a shadow under it on -1 and a window on 3, with 1 and 2 left empty.
1. Start the editor ("Game and editor" profile, F5).
2. Tiles tab: in the sheet box choose the half-timber house sheet; drag over a 2 x 2 block of wall cells (a dashed rectangle).
3. Objects > **New object...**: the composer opens with the wall painted; the Level box shows "0 (base)". Open it: fifteen entries, "7" at the top, "-7" at the bottom, "0 (base)" in the middle. Click "0 (base)" to close it.
4. In the **Columns** box type `3`: a third, empty column appears on the right of the canvas.
5. Choose "-1" in the Level box. In the composer's picker (left), click any cell with a solid picture (a ground or shadow cell); click the top cell of the empty third column: it shows whole, and the wall is drawn faded (the levels above -1).
6. Choose "3". In the picker, click a window or door cell (one with clear pixels); click the top-left canvas cell: the window shows over the wall, with no error although levels 1 and 2 are empty. The wall and the -1 cell are drawn whole.
7. Still on "3", click the top cell of the third column: the window shows over the -1 cell, which shows through the window's clear pixels.
8. Press Ctrl+Z: the window over the third column goes; the Level box still shows "3". Click that canvas cell again: it comes back with no error. Press Ctrl+Z once more.
9. Choose "0 (base)": the window on the top-left cell is drawn faded; right-click the top-left canvas cell: the wall cell under the window goes, the faded window stays. Press Ctrl+Z: the wall is back.
10. Type an id (for example `free_levels_test`) and a category; press **Save**; in the Shapes window press **OK**.
11. Objects tab: the preview shows the -1 cell in the third column and the window over the wall.
12. Objects > **Edit object...**: "0 (base)" is chosen; choose "-1": everything but the third column's -1 cell is faded. Press **Cancel**.
13. Press Ctrl+S. In Visual Studio's Git Changes window, open `content/objects.json`: `free_levels_test`'s tiles start with the one carrying `"level": -1`, then the base tiles (no `level`), then the one with `"level": 3`.
14. Start the game from the same profile (the editor may stay open); it reaches the title with no content error. To see the object in the game, place it as OL4's hand check did (Objects tab, click the start map about three cells above the player's start, Ctrl+S, new game): the -1 cell under the window, the wall unchanged.
15. If you don't keep it: in the editor, right-click the placed object on the map (if placed), choose `free_levels_test` in the Objects tab, press **Remove**, then Ctrl+S.
16. Press F1: the Objects section's Level line says "from -7 to 7".
