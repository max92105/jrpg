# E6a. Re-cutting a set-up sheet

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Key decisions" 3 and "Risks" (a re-cut changes art on every map); decision 116. Builds on E5b2's Cut panel. Split from E6 (developer, 2026-10-21): the warning for a sheet whose tiles are used, and asking before Apply, are [E6b](E6b.md).

- **E6a. Re-cutting a set-up sheet** (Editor, about 200 lines)
  - **Goal (decision 116):**
    - A **Cut** button for a set-up sheet (and Tiles > Cut sheet) opens E5b2's Cut panel pre-filled with the sheet's cut. Apply replaces the cut; tiles keep their column and row.
    - A cut that would put a tile's cell outside the image is refused with its id: "Tile 57 at (11, 3) would be outside the 10 x 8 grid." (the art check would refuse the save, and the editor would no longer open the maps).
    - Apply re-cuts at once (one catalog step, E4b); the map, the sheet marks and the object preview redraw with the new cut. Ctrl+Z restores the old cut.
    - **Known gap until E6b:** re-cutting a sheet whose tiles are used asks nothing first.
  - **Changes:**
    - `Tiles/SheetCutViewModel`: takes the tiles cut from the sheet; `Problem` also covers tiles outside the new grid.
    - `Tiles/TileBrowserViewModel`: `CutSheetCommand` (can execute on a set-up sheet with no panel open: opens `Cut` with its cut).
    - `Tiles/TileImages`: crops cached by (sheet record, column, row) instead of tile id, so a re-cut sheet gets fresh crops.
    - `Shell/MainWindow`: redraws the map and the object preview when `CatalogDraft.Current` changes (until now only map changes redrew it).
  - **Tests:**
    - `SheetCutViewModelTests`: `Problem_TileOutsideNewGrid_NamesTheTile`.
    - `TileBrowserViewModelTests`: `CutSheetCommand_SetUpSheet_OpensCutWithItsColumnsAndRows`, `ApplyCutCommand_ReCut_ReplacesTheCut`.
    - `CatalogDraftTests`: `Undo_AfterReCut_RestoresTheOldCut`.
  - **Verify by hand:**
    - [ ] Re-cut `grass_3` (set up in E5b, unused): Apply; the grid changes.
    - [ ] On a copy of the content (`--content`), re-cut `ground` to 24 px: the map redraws with quarter tiles; Ctrl+Z: back to 48 px.
    - [ ] A cut too small for a used tile is refused, naming it.
- **Done, with deviations:** split from E6 (developer). The Cut button shows only on a set-up sheet with no panel open (`CanCutSheet`), as Set up does. `ObjectBrowserView.ShowPreview` became public so the window redraws the preview on a catalog change; the map also redraws on every catalog change (a painted tile included), which costs one extra draw. Review: the panel's tile list (`SheetCutViewModel.Tiles`) is kept current while it is open, so a tile painted meanwhile outside the new grid refuses Apply; tests added for that, the row boundary and when Cut can't run.
