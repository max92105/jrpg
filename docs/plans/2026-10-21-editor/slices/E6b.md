# E6b. Warning before re-cutting a used sheet

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Risks" (a re-cut changes art on every map); decision 116. Builds on [E6a](E6a.md) and E4b's `CatalogUsage`. Split from E6 (developer, 2026-10-21).

- **E6b. Warning before re-cutting a used sheet** (Editor, about 170 lines)
  - **Goal (decision 116):**
    - For a sheet with tiles, the Cut panel warns while editing: "12 tiles are cut from this sheet, used on 2 maps (forest, start) and by 1 object (tree); they will show other pixels."
    - Apply asks once with the warning; on yes, it re-cuts as in E6a; on no, nothing changes and the panel stays open.
  - **Changes:**
    - `Catalogs/CatalogUsage`: `OfSheet(ContentCatalog catalog, string sheetId, MapLayout? openLayout)`: the tiles cut from a sheet and the maps and objects using them.
    - `Tiles/SheetCutViewModel`: takes the sheet's usage; `Warning`.
    - `Tiles/TileBrowserViewModel`: `CutSheetCommand` passes the usage (open map included); `ApplyCutCommand` asks through the shell's confirm when `Warning` is set. The browser's image list and image size lookup become one dependency, so the confirm and the open map fit within four constructor parameters.
  - **Tests:**
    - `CatalogUsageTests`: `OfSheet_SheetUsedByMapAndObject_CountsBoth`, `OfSheet_UnusedSheet_IsEmpty`.
    - `SheetCutViewModelTests`: `Warning_SheetWithUsedTiles_NamesMapsAndObjects`, `Warning_SheetWithoutTiles_IsNull`.
    - `TileBrowserViewModelTests`: `ApplyCutCommand_UserDeclinesWarning_KeepsTheCut`, `ApplyCutCommand_UserAccepts_ReplacesTheCut`.
  - **Verify by hand:**
    - [ ] On a copy of the content (`--content`), re-cut `ground` to 24 px: the warning names the maps; No keeps 48 px; Yes redraws the map with quarter tiles.
