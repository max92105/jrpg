# E3d. A chosen cell becomes a tile when painted; Save writes the catalogs

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Key decisions" 2 and 4; decisions 113, 114; ADR 0014. Builds on E3b (any cell chosen) and E3c (the writer).

- **E3d. A chosen cell becomes a tile when painted; Save writes the catalogs** (Editor, about 200 lines)
  - **Goal:**
    - Painting with a cell that isn't a tile yet makes it a tile (walkable, id one above the highest; decisions 113, 114) and paints it, with brush, rectangle, fill and fill layer alike, on whatever layer is chosen. Choosing a cell alone adds nothing.
    - The sheet marks the new tile like the others; the line under the sheet names its id.
    - Ctrl+S writes the catalogs (`WriteMapCatalogs`) before the map when the draft changed, so a saved map never names a tile the game doesn't know. Undoing the paint keeps the new tile (an unused tile is harmless; ADR 0014).
    - A catalog the game would refuse, or that breaks the art check, isn't written: the save line shows the loader's message, and the map isn't written either.
  - **Changes:**
    - `Catalogs/CatalogDraft`:
      - `TileAt(SheetCell cell)`: the tile cut from that cell (lowest id if several), or null.
      - `AddTile(SheetCell cell)`: adds a walkable tile there and returns its id.
      - `IsDirty`, `MarkSaved()`.
    - `Tiles/TilePaintingViewModel`: every paint action takes the tile id from `TileAt(Browser.SelectedCell)`, adding it first when there is none.
    - `Tiles/TileImages`: crops looked up through `CatalogDraft.Current`, so a new tile draws at once.
    - `Shell/MainViewModel.Save`: `WriteMapCatalogs(Content.Current)` and `MarkSaved()` when the draft is dirty, then `WriteMap` against `Content.Current`.
  - **Tests:**
    - `CatalogDraftTests`: `AddTile_EmptyCell_AddsWalkableTileAboveHighestId`, `AddTile_EmptyCell_MarksDirty`, `TileAt_CellWithoutTile_ReturnsNull`, `MarkSaved_AfterAddTile_IsClean`.
    - `TilePaintingViewModelTests`: `StartPainting_CellNotATile_AddsTileAndPaintsIt`, `StartPainting_CellAlreadyATile_AddsNoTile`, `FillLayerCommand_CellNotATile_AddsOneTile`.
    - `MainViewModelTests`: `Save_MapPaintedWithNewTile_WritesTileAndMapThatLoadBack` (loads the saved folder back), `Save_CatalogTheArtCheckRefuses_ShowsMessageAndWritesNothing`.
  - **Verify by hand:**
    - [ ] The developer's check: click any cell of any set-up sheet, paint it on the map: it shows at once; the cell is now marked as a tile. Repeat on two other sheets.
    - [ ] Undo: the paint goes, the tile mark stays.
    - [ ] Ctrl+S: `git diff` shows the new tiles at the end of `tiles.json` (CRLF), the map change, and nothing else.
    - [ ] **Above the player:** choose "Above player (roofs, treetops)" (z 5), paint a row of leaves across a path; Ctrl+S; F5: walk along the path: the leaves are drawn over the player. Paint on "Ground detail" (z -4): it is drawn under the player.
- **Done, with deviations:** `TileImages` needed no change (it already read `CatalogDraft.Current` since E3a; a cached crop only goes stale when a tile is re-cut, E6). The browser refreshes its tiles, choice and line when the draft changes, tested by `AddTile_ChosenCell_NamesTheNewTileAndMarksIt` (the E3b follow-up). The tile lookup moved from the browser to `CatalogDraft.TileAt`.
- **Review follow-up:** the save test is named `..._WritesTileAndMapThatLoadBack` (the order is pinned by the refusal test); `StartPainting_NoOpenMap_AddsNoTile` added. Closing or leaving a map asks only about `CurrentMap.IsDirty`, not `CatalogDraft.IsDirty`: handed to E4a.
