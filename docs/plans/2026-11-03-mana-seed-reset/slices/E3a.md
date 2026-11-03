# E3a. Remove a sheet with its tiles when none is used; tile ids never reused

Builds on design.md, "Key decisions" 3, and "Structure" (`Tiles/SheetRemoval`).

## Goal
In the Tiles panel, "Remove sheet" takes the shown sheet and every tile cut from it out of the catalog as one undo step when no map (the open one as on screen, in place of its saved file) and no object uses any of them; otherwise nothing changes and the status names the users (`CatalogUsage.OfSheet`). The image stays in the folder, listed "(not set up)"; it leaves by Explorer or `git rm` (L1). Tile ids are never reused within a session, so undoing a removal after painting never overwrites the new tile (the review's blocker).

## New types
- `Editor/Tiles/SheetRemoval`: takes the shown sheet out of the catalog, and says what it did or why it refused. Constructor `(CatalogDraft catalog, TileBrowserViewModel browser, Func<MapViewModel?> openMap)`: the open map gives its layout as on screen now, and its unsaved state for E3c, which adds the writer as the fourth parameter. `MainViewModel` builds it beside `Painting` as `Removal` (two lines, paid by X1's freed lines). E3c adds the missing-image case.

## Changes
- `Editor/Catalogs/CatalogDraft`:
  - a high-water mark beside `Current`: the highest tile id seen this session (loaded, made, or brought back by an undo or redo); `NewTileAt` takes the mark plus one and raises it. Its summary's "tiles are never renumbered" gains "and an id is never reused".
  - `RemoveTilesheet(string sheetId)`: one `CatalogEdit` removing the sheet and its tiles; its revert adds both back.
- `SheetRemoval.RemoveSheetCommand` (enabled while a set-up sheet is shown) and `Status`: "Removed sheet water_2 and its 12 tiles." or "Can't remove sheet water_2: its tiles are used on maps forest, start and object tree." Cleared when the shown sheet changes.
- `Editor/Tiles/TileBrowserView.xaml`: the button beside Set up and Cut, and the status line (themed, `BasedOn`), bound to `Removal`.

## Tests (new `Tiles/SheetRemovalTests`; `Catalogs/CatalogDraftRemovalTests` from E1b)
- `CatalogDraftRemovalTests: RemoveTilesheet_WithTiles_RemovesBothAsOneUndoStep`.
- `CatalogDraftRemovalTests: Undo_AfterRemoveTilesheet_BringsTheSheetAndItsTilesBack`.
- `CatalogDraftRemovalTests: AddTile_AfterRemovingTheHighestTiles_NeverReusesTheirIds`.
- `CatalogDraftRemovalTests: Undo_RemoveTilesheetAfterANewTileWasPainted_KeepsTheNewTile` (the removal, then a painted tile, then the undo: both the restored tiles and the new one are there, with different ids).
- `SheetRemovalTests: RemoveSheet_NoTileUsed_RemovesItAndSaysSo`.
- `SheetRemovalTests: RemoveSheet_TileUsedOnAMap_RefusesNamingTheUsers`.
- `SheetRemovalTests: RemoveSheet_TileUsedByAnObject_Refuses`.
- `SheetRemovalTests: RemoveSheet_TileUsedOnlyInTheOpenMapsSavedFile_Removes` (the open map, as on screen, replaces its saved file).
- `SheetRemovalTests: RemoveSheet_Refused_LeavesTheCatalogUnchanged` (no step recorded).
- `SheetRemovalTests: RemoveSheet_Removed_ShowsTheImageAsNotSetUp`.

Size: about 70 lines of code, 10 tests (about 200).

## Verify by hand
- In a test copy of the content, on a sheet used by one scratch map: Remove sheet refuses naming the map; erase the map's tiles, Remove sheet: the sheet leaves and its image shows "(not set up)"; paint a tile from another sheet; Ctrl+Z twice: the painted cell is undone and the sheet comes back with its tiles, the painted tile kept; Ctrl+S, restart the editor: it opens.
