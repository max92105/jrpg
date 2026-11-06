# OM2. The canvas a map selection makes

Builds on design.md, "What exists" (the composer) and "Key decisions" 11; decision 514 (Q10's answer); ADR 0040 (levels -7 to 7).

## Goal
Headless: a rectangle of map cells and the base layer (the layer chosen in the tool bar, `baseZ` everywhere in this phase) make a composer canvas of the rectangle's size. Every layer holding a tile inside the rectangle is taken: the base layer on level 0, the taken layers under it on -1, -2 and so on (the nearest first), those over it on 1, 2 and so on; a layer with no tile in the rectangle takes no level (empty layers compacted); the base layer stays level 0 even when empty there. Each cell's tile goes back to its sheet cell. The reasons it can't: no tile in the rectangle on any layer, a rectangle over 32 cells either way, more than 7 taken layers under the base or more than 7 over it. Nothing is reachable from the window yet (OM3).

## New types
- `Editor/Objects/Composing/MapSelectionCanvas` (`internal static class`): the canvas a map rectangle makes, every layer on its level around the base one, and why it can't.
  - `public static IReadOnlyList<(int Z, int Level)> TakenLayersOf(MapLayout layout, MapCellRectangle cells, int baseZ)`: the layers with a tile in the rectangle (and the base layer), bottom first, each with its level. Read by `Of`, by OM3's warning and by OM4's replacement, so the levels are worked out once.
  - `public static string? ProblemOf(MapLayout layout, MapCellRectangle cells, int baseZ)`: "No tile in the selection." / "The selection is {c} x {r} cells; an object is at most 32 each way." / "The selection has tiles on {n} layers under z {baseZ}; an object has 7 levels under its base." (the same over it); null when it can. The limits read `ObjectCanvas.MaxCells`, `-ObjectTile.MinLevel` and `ObjectTile.MaxLevel` (no new number).
  - `public static ObjectCanvas Of(MapLayout layout, MapCellRectangle cells, int baseZ, IReadOnlyDictionary<int, TileDefinition> tiles)`: throws `ArgumentException` with `ProblemOf`'s text when it isn't null; the canvas is built at once with `ObjectCanvas.OfCells` (below), one `SheetCell` per painted map cell on its layer's level. Not a `Stamp` per cell: each stamp copies the whole dictionary, up to 15 x 32 x 32 times.

## Changes
- `Objects/Composing/ObjectCanvas` (124, ends about 140): `public static ObjectCanvas OfCells(int width, int height, IReadOnlyDictionary<CellOnLevel, SheetCell> cells)`: a canvas holding exactly those cells; throws `ArgumentOutOfRangeException` for a cell outside the size or a level outside `ObjectTile.MinLevel` to `MaxLevel` (the existing `ThrowIfOutsideTheLevels` and `IsInside`).
- `ObjectCanvasTests` (140): `OfCells_CellsInside_HoldsThem`, `OfCells_CellOutsideTheSize_Throws`, `OfCells_LevelOutsideTheRange_Throws` (one side each: the range check is `ThrowIfOutsideTheLevels`, tested on both sides already).

## Tests (new `Objects/Composing/MapSelectionCanvasTests`, layouts, tiles and sheets built in code; layers set up out of z order where order matters)
- `TakenLayersOf_BaseAndOneOver_AreLevelsZeroAndOne`.
- `TakenLayersOf_TwoLayersUnderTheBase_AreMinusOneThenMinusTwoFromTheNearest` (z 0 and z -2 under base z 1: z 0 on -1, z -2 on -2).
- `TakenLayersOf_LayerWithNoTileInTheSelection_TakesNoLevel` (over the base: z 2 empty there, z 3 on level 1).
- `TakenLayersOf_BaseEmptyInTheSelection_StaysLevelZero` (the layer over it is level 1).
- `Of_GroundUnderTheBase_IsOnLevelMinusOne` (the developer's case: grass under a tree).
- `Of_EachCell_IsItsTilesSheetCell` (two sheets).
- `Of_EmptyMapCell_StaysEmptyOnTheCanvas`.
- `Of_Rectangle_GivesTheCanvasItsSize`.
- `ProblemOf_NoTileOnAnyLayer_SaysSo`.
- `ProblemOf_ThirtyThreeColumns_SaysTheLimit`; `ProblemOf_ThirtyThreeRows_SaysTheLimit`; `ProblemOf_ThirtyTwoByThirtyTwo_IsNull` (the accepted bound).
- `ProblemOf_EightLayersOverTheBase_SaysTheLimit`; `ProblemOf_EightLayersUnderTheBase_SaysTheLimit`; `ProblemOf_SevenUnderAndSevenOver_IsNull` (one test per side and the bound).
- `Of_WithAProblem_Throws`.

Size: about 70 lines of code; 20 tests (about 170 with a small shared layout builder): about 240. The largest of phase 6; its tests are one layout and one assert each, and the code one type.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `Composing/MapSelectionCanvas` (the canvas a map selection makes: every layer on its level around the base layer).

## Hand check (yours)
Nothing to see yet (OM3).
