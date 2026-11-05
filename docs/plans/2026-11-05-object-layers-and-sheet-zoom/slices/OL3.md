# OL3. The composer's levels

Builds on design.md, "Key decisions" 4 and 5; decisions 478 and 480. The rules before the window (OL4), as every editor panel is split.

- **Done, with deviations:** `Stamp_PastTheHighestLevel_Throws` is a theory over both sides (-1 and 8, `Stamp_OutsideTheLevels_Throws`) with `Stamp_OnTheHighestLevel_PaintsIt` and `HighestLevel_Blank_IsMinusOne` beside it; the choices and the clamp follow every canvas change (`OnCanvasChanged`: a stroke, an undo, a redo or a resize), which covers the undo rule; a level is chosen only once one below it is painted, so the save test paints the base first. The review asked for `ObjectCanvasDrawingTests.Draw_LevelOnePaintedBeforeTheBase_DrawsTheBaseFirst` (the drawing's order by level had no test).

## Goal
The composer's canvas holds its cells by cell and level. Stamping and erasing name their level and leave the others alone; a stacked object opens in Edit with each tile on its level (closing OL1's known gap); Save writes the tiles bottom level first with their levels. The painting has a chosen level its strokes act on and offers the levels from 0 to one above the highest painted, so one empty level always waits on top; a value outside those choices is ignored, and an undo that leaves the chosen level above them chooses the highest. Nothing in the window shows the choice yet (OL4), so by hand the composer still paints the base level; the tests choose others.

## New types
- `Editor/Objects/Composing/CellOnLevel` (`readonly record struct CellOnLevel(GridCell Cell, int Level)`): a canvas cell on one level, the key of the canvas's painted cells.

## Changes
- `Editor/Objects/Composing/ObjectCanvas` (summary: an object's picture being composed: its size and the sheet cell painted on each of its cells, level by level):
  - `Cells` becomes `IReadOnlyDictionary<CellOnLevel, SheetCell>`;
  - `Stamp(int level, GridCell at, SheetCellRectangle cells)` and `Erase(int level, GridCell at)` replace the level-less ones (`ArgumentOutOfRangeException` outside 0 to `ObjectTile.MaxLevel`);
  - `int HighestLevel`: the highest level holding a cell, -1 when blank;
  - `Resize` keeps every level's cells inside; `Of(SheetCellRectangle)` paints level 0; `Of(ObjectDefinition, tiles)` keys each tile by its cell and level;
  - `TilesOf(tileIdOf)`: by level from the base, then row by row, each `ObjectTile` with its level (an unchanged one-level object still compares equal).
- `Editor/Objects/Composing/CanvasPainting` (summary names the chosen level):
  - `int Level` (0 at the start): what `PaintAt` and `EraseAt` act on. A hand-written property, not `[ObservableProperty]`: its setter ignores a value outside `LevelChoices` (a combo box bound by `SelectedIndex` pushes -1 while its list is rebuilt, as `ObjectShapesWindow.OnZoneListSelectionChanged` guards);
  - `IReadOnlyList<string> LevelChoices`: "0 (base)", then "1", "2"... up to `HighestLevel + 1`, at most `ObjectTile.MaxLevel`; raised when the canvas's highest level changes, followed by `Level` so a bound box shows the choice again;
  - after an undo or redo, when `Level` is past the last choice, it becomes the last choice.
- `Editor/Objects/Composing/ObjectComposing.StartNew`: stamps level 0. `TryApply`'s `Canvas.Cells.Values` is unchanged.
- `Editor/Objects/Composing/ObjectCanvasDrawing.Draw`: reads `Cells` with their level, drawing levels from the base up (no fading yet: OL4).
- Call-site churn, mechanical: `.Stamp(` / `.Erase(` gain a leading `0, ` and `Cells[new GridCell(...)]` becomes `Cells[new CellOnLevel(new GridCell(...), 0)]` in `ObjectCanvasTests` (about 15 lines), `ObjectCanvasDrawingTests` (3), `CanvasPaintingTests` (3), `ObjectComposingTests` (3, including the dictionary built at line 189).

## Tests
- `ObjectCanvasLevelTests` (new, beside `ObjectCanvasTests`):
  - `Stamp_OnLevelOne_KeepsTheBaseCellUnder`;
  - `Erase_OnLevelOne_LeavesTheBaseCell`;
  - `Stamp_PastTheHighestLevel_Throws` (level 8);
  - `Of_StackedObject_PutsEachTileOnItsLevel` (levels 0 and 2 used);
  - `TilesOf_TwoLevels_ListsTheBaseLevelFirstWithTheirLevels`.
- `CanvasPaintingLevelTests` (new, beside `CanvasPaintingTests`):
  - `PaintAt_OnLevelOne_StampsOverTheBaseCell`;
  - `LevelChoices_BlankCanvas_IsTheBaseOnly`;
  - `LevelChoices_PaintedOnLevelOne_EndsWithAnEmptyLevelTwo`;
  - `LevelChoices_PaintedOnTheHighestLevel_StopsAtIt` (level 7 painted: eight choices, no ninth);
  - `LevelChoices_StrokeOnTheTopLevel_RaisesPropertyChanged`;
  - `Undo_StrokeThatOpenedTheChosenLevel_ChoosesTheHighestLevelLeft` (paint on 1, choose 2, undo the stroke: the choices end at 1, which is chosen);
  - `Level_SetToMinusOne_KeepsTheChosenLevel`;
  - `Level_SetPastTheChoices_KeepsTheChosenLevel` (the other side of the guard).
- `ObjectComposingLevelTests` (new, beside `ObjectComposingTests`, which is 261 lines): `TryApply_OverlayOnLevelOneOverABaseCell_SavesBothTilesBaseFirst` (two test-world cells; the saved tiles are `(0, 0, base)` then `(0, 0, overlay, 1)`); `StartEdit_StackedObject_OffersEveryLevelAndOneAbove`.
- `ObjectComposerViewModelTests: Problem_OnlyAnUpperLevelPainted_IsNull`.
- Guards that must pass unchanged in meaning: every `ObjectCanvasTests`, `CanvasPaintingTests`, `ObjectComposingTests` and `ObjectComposerViewModelTests` case after the mechanical edit (test counts before and after equal).

Size: about 75 lines of code, 16 tests (about 135), about 25 lines of one-token call-site churn: about 235. Over the 200 aim by the churn alone; the model and its chosen level are one rule (the choices read the canvas), so no split.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `Composing/ObjectCanvas` becomes "the object composer's picture: sheet cells on a grid by cell and level (`CellOnLevel`, ADR 0040), stamped, erased, resized, made into tiles"; `Composing/CanvasPainting` gains "on the chosen level, one empty level offered on top".

## Hand check (yours)
Regression only; the Level box comes in OL4.
1. Start the editor ("Game and editor" profile, F5).
2. In the Objects tab choose an object you made with the composer; Objects > **Edit object...**: the composer shows it as before.
3. Stamp one cell from the picker, then press Ctrl+Z in the composer: the cell goes back. Press **Cancel**.
4. Objects > **New object...** with no cells chosen in the Tiles panel: an empty 3 x 3 canvas; stamp two cells, type an id and a category, **Save**; the Shapes window opens; press **OK**. The Objects panel's preview shows the two cells. Ctrl+Z in the main window removes the object.
