# FS5. The editor draws NPCs from their records; `FieldSpriteLayout` deleted

Editor + Data, about 170 lines (90 of them tests). Design: "Replacement mapping". Builds on FS1, FS3 (`AreaOver`, `StandingCell`). Folds two backlog lines: `MapDrawing.DrawObjectAt`'s NPC branch has no test; the New NPC dialog's sprite size check (superseded by SI5).

## Goal
The editor's map, the Objects panel's preview, the placing ghost and the Shapes window draw an NPC as the game does: its record's standing frame facing down, scaled, standing on its cell. The fixed layout's last readers go, and with them `Data/Art/FieldSpriteLayout`.

## New types
None.

## Changes
- `Editor/Tiles/TileImages.FieldSpriteFrameOf(string file)`: crops `StandingCell(Facing.Down)` of the file's record (`catalog.Current.FieldSprites`); a missing image, a file without a record (an object made before its record is set up, possible once SI1 lets the draft change) or a frame past the image's edge draws `MissingArtMark` (decision 442), never cached; the cache is keyed by the record, so a changed setup crops anew.
- `Editor/Maps/MapDrawing.DrawObjectAt`: an NPC's picture goes at its cell's top-left plus `AreaOver(CellPixels)` (the record's), the missing mark over the object's cells when it has no record.
- `Editor/Catalogs/MissingArt`: unchanged (its sprite list still reads `FieldSpriteFile`).
- `Data/Art/FieldSpriteLayout` and `tests/Jrpg.Data.Tests/Art/FieldSpriteLayoutTests` deleted; grep `FieldSpriteLayout`, "48 px frames" and "laid out like the player's" in `src/`, `docs/map.md` and comments (`ObjectDefinition.FieldSpriteFile`'s summary names the record instead).
- `docs/map.md`: the Data `Art` row drops `FieldSpriteLayout`.

## Tests
- `TileImagesTests`: `FieldSpriteFrameOf_PlaceholderSheet_IsItsStandingFrame` (48 x 48 from (0, 0)); `FieldSpriteFrameOf_RecordWithOtherFrames_CropsItsSize` (a test-written 64 x 96 PNG, 16 x 24 frames, down on row 2: the crop at (0, 48, 16, 24)); `FieldSpriteFrameOf_FileWithoutRecord_IsMissingMark`; `FieldSpriteFrameOf_FramePastImage_IsMissingMark`.
- `MapDrawingTests` (the backlog's gap): `Draw_PlacedNpc_DrawsItsFrameOverItsArea` (the drawing's bounds include the area above the cell for a tall record).

## Verify by hand
1. `dotnet run --project src/Jrpg.Editor`, open the start map: the placeholder NPC looks as before.
2. With FS3's hand-written bought NPC record: the NPC shows on the map at the game's size, feet on its cell; select it with a click on its feet (its head isn't clickable: design, "Risks"); its Shapes window opens.

- **Done, with deviations:** the picture of an NPC on its own (the ghost, the Objects panel preview, the Shapes window) shrinks a frame taller than the cell into the cells box, standing on its bottom, so the outline and the grid keep their place; the map draws the true area. The crop tests read the placeholder sprite through changed records instead of a test-written PNG (WPF decodes a whole image, the header-only test files would not do).
