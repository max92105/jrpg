# E3c. The catalog writer

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Data changes" and "Risks" (the art check guards the editor; first write may reformat `objects.json`); ADR 0014. Split up front from the painting slice (review); its first user is E3d's Save, the next slice.

- **E3c. The catalog writer** (Data, about 170 lines)
  - **Goal:** Data can write the three map catalogs the editor changes, and refuses anything the loader or the art check would refuse, writing nothing then. No editor change yet.
  - **Data:**
    - `Content/ContentWriter.WriteMapCatalogs(ContentCatalog catalog)`:
      - validates `tilesheets.json`, `tiles.json` and `objects.json` with their validators, every map's layout with `MapLayoutValidator`, and `ArtReferenceCheck.FindProblems(catalog, ArtFolders.RootIn(contentRoot))`;
      - any problem throws `ContentValidationException` (the loader's message, naming the file and id) and writes nothing;
      - writes the three lists sorted by id (ordinal for strings), CRLF, each aside first, then moved, as `WriteMap` does.
  - **Tests:**
    - `ContentWriterTests` (Data):
      - `WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes` (into a temporary copy of `content/`);
      - `WriteMapCatalogs_NewTile_LoadsBackWithIt`;
      - `WriteMapCatalogs_TileOnUnknownSheet_ThrowsAndWritesNothing`;
      - `WriteMapCatalogs_TileOutsideItsImage_ThrowsNamingTheTile`;
      - `WriteMapCatalogs_MapUsesMissingTile_ThrowsNamingTheMap`.
  - **If the identical-bytes test fails** on number formatting in `objects.json` (design.md, "Risks"): a one-off reformat commit of the content before this slice, noted here, not a custom number format.
  - **Verify by hand:**
    - [x] After the tests, `git status` shows no change under `content/` (they write into a temporary copy).
- **Done, with deviations:** as planned; the identical-bytes test passed, so `objects.json` needed no reformat. `WriteMap` and `WriteMapCatalogs` share one aside-then-move writer (`WriteAll`); art problems are joined one per line into the exception message.
- **Review follow-ups (pre-existing, now covering three files):** `WriteAll`'s final moves aren't atomic across files (a failed 2nd move leaves the 1st replaced); its catch's `File.Delete` can throw for a folder not created yet, hiding the first error.
