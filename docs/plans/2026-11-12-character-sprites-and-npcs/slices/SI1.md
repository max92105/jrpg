# SI1. Field sprites save and import through Data

Data + Editor, about 240 lines (120 of them tests). Design: key decisions 6 and 8, "Risks" (coupling with plan 2). Decision 569. Builds on FS1 and CW1.

## Goal
The editor can change sprite records as one undo step and save them with Ctrl+S (`field-sprites.json` written with the other catalogs when it changed, checked as the loader checks it), and Data can copy an image into an art folder with that folder's own check: the field-sprite folder refuses one too small for its record. Data's own message for a bad record is reachable from a view model, so no editor rule copies a content rule. Plan 2's enemy art reuses the import.

## New types
- `Data/Content/CatalogProblems` (public static): the first problem the loader would report in an edited catalog, as its message, or null. `OfFieldSprites(IReadOnlyList<FieldSpriteSheet> sheets, ContentCatalog content)` runs `FieldSpriteCatalogValidator` and `FieldSpriteReferenceCheck` and turns their `ContentValidationException` into its message. Plan 2 adds `OfShops` and `OfEnemies`.
- `Data/Content/ContentWriter.ArtImports.cs` (partial): first the image imports moved out of `ContentWriter.cs` with no change (`HasTilesheetImage`, `ImportTilesheetImage`, `TilesheetImagePath`); then `HasArtImage(string artFolder, string fileName)` and `ImportArtImage(string artFolder, string sourcePath, ContentCatalog catalog)`: the folder must be field sprites, battlers, portraits, effects or backgrounds (else `ArgumentException`); a non-PNG refused; the folder's own check: a field sprite too small for the record of the same file (FS2's counts), an effect strip that isn't whole square frames (the startup check's rule, one internal method shared with `ArtReferenceCheck.AddPartialFrameStrips`); copied aside then moved, as the tilesheet import.
- `Editor/Catalogs/CatalogDraft.FieldSprites.cs` (partial; `CatalogDraft.cs` is at 269): `SetFieldSprites(IReadOnlyList<FieldSpriteSheet> changed)`: one undo step, nothing when none differs.

## Changes
- `Data/Content/ContentWriter.CheckedCatalogWrites`: validates `field-sprites.json` and its references against the objects and new game, and writes it when changed (CW1's stamps); its summary names the sprites.
- `Data/Art/ArtReferenceCheck`: the "too small for its record" test shared with the import (internal, as `FirstTileOutside`).
- `Editor/Tiles/TileImages.Forget`: also drops the cached sprite frames of the file (backlog line "TileImages.Forget keeps cached field-sprite frames").
- No undo refusal: only objects name a record, and they are catalog steps undone first.

## Tests
- `ContentWriterFieldSpriteTests` (new sibling; `ContentWriterTests` is at 254): `WriteMapCatalogs_ChangedSheet_WritesFieldSpritesFile`; `WriteMapCatalogs_SheetBreakingARule_WritesNothing`; `WriteMapCatalogs_ObjectNamingUnknownSprite_WritesNothing`.
- `ContentWriterArtImportTests` (new sibling): `ImportArtImage_NewFieldSprite_CopiesIt`; `ImportArtImage_FieldSpriteTooSmallForItsRecord_RefusesAndCopiesNothing`; `ImportArtImage_EffectNotWholeFrames_Refuses`; `ImportArtImage_Battler_CopiesIt`; `ImportArtImage_NotAPng_Refuses`; `ImportArtImage_TilesheetFolder_Throws` (the tile import keeps its own method).
- `ContentWriterRepositoryTests`: the identical-bytes test covers `field-sprites.json`.
- `CatalogProblemsTests` (new): `OfFieldSprites_RepositorySheets_IsNull`; `OfFieldSprites_ScaleZero_NamesFileAndField`; `OfFieldSprites_ObjectNamingDroppedSheet_NamesObject`.
- `CatalogDraftFieldSpriteTests` (new): `SetFieldSprites_ChangedSheet_IsOneUndoStep`; `SetFieldSprites_SameSheets_RecordsNothing`; `Undo_AfterSetFieldSprites_RestoresTheRecord`.
- `TileImagesTests`: `Forget_SpriteFile_CropsTheNewImage`.

## Verify by hand
None: no window changes the records yet (SI4).
