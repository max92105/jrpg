# E7a. Importing a sheet image: the Data writer

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Data changes"; decision 125. Nothing invalid may be written (review): a replacement is checked against the tiles cut from it before it is copied. Split from E7 (developer, 2026-10-21): the editor's command, dialog and refresh are [E7b](E7b.md).

- **E7a. Importing a sheet image: the Data writer** (Data, about 150 lines)
  - **Goal:**
    - Data can copy a PNG into `content/art/tilesheets/` without ever leaving content the game would refuse. Nothing is visible in the editor yet (E7b), as with E3c.
    - A file that isn't a PNG is refused, naming it.
    - If a sheet uses that file name, the new image is checked against it: every tile cut from it must still lie inside the new image. If not, the import is refused, naming the tile ("Tile 57 at (11, 3) would be outside the new 320 x 256 image"), and the old file stays. The catalog given is checked, so the editor's unsaved tiles count.
  - **Data:**
    - `Content/ContentWriter.HasTilesheetImage(string fileName)`.
    - `Content/ContentWriter.ImportTilesheetImage(string sourcePath, ContentCatalog catalog)`: checks the header with `PngSize`; for every sheet of `catalog` whose `File` is that name, checks each of its tiles against the new size with the art check's cell rule (`ArtReferenceCheck.IsCellInside`, made internal and reused, not copied); any problem throws `ContentValidationException` and copies nothing; then copies aside and moves into place; returns the file name.
  - **Tests:**
    - `ContentWriterTests` (Data):
      - `ImportTilesheetImage_Png_CopiesIntoTilesheetArt`;
      - `ImportTilesheetImage_NotAPng_ThrowsAndCopiesNothing`;
      - `ImportTilesheetImage_ReplacementTooSmallForItsTiles_ThrowsNamingTheTileAndKeepsTheOldFile`;
      - `ImportTilesheetImage_ReplacementLargeEnough_ReplacesTheFile`.
  - **Verify by hand:** nothing to see until E7b.
- **Done, with deviations:** split from E7 (developer). The cell rule stays private in `ArtReferenceCheck`; it gains an internal `FirstTileOutside(catalog, file, size)` that the writer calls, so the lookup of the file's sheets is in the art check too (sheets match the file name ignoring case). The message keeps the file name first ("Ground.png: tile 57 at (11, 3) would be outside the new 320 x 256 image."), as the art check's do. Review: a test that a replacement named in another case is still checked.
