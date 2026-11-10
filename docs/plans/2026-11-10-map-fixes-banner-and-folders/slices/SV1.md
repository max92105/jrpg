# SV1. The save deadlock reproduced, then fixed

Data + Editor, about 250 lines (25 of them re-pinned tests). Builds on design.md, "Diagnosis: the save deadlock" and "Key decisions" 1. Decision 518 (Q1). The first slice of the plan: the developer is blocked on saving.

## Goal
A map resized so that a warp on another map had to move into its new cells saves: Save all writes both maps (and the tiles and objects when changed) in one go, every map checked against the open maps as they are now. Save writes the changed tiles and objects and the shown map in one go. When a map is refused, no map is written; the changed tiles and objects are still written alone when they pass on their own (as Ctrl+S always did), and the error line says so.

## Order inside the slice
1. Write `MainViewModelSaveTogetherTests.SaveAll_ResizedMapAndTheWarpMovedIntoItsNewCells_WritesBoth` and see it fail with today's code (the error line names one of the two layouts). Note the failure message in the Done note.
2. Then the fix below, then the other tests.

## Changes
- **Data `Content/ContentWriter`:** `public void WriteMaps(ContentCatalog content, IReadOnlyCollection<string> mapIds)`: every map in `content.Maps` checked as `WriteMapCatalogsAndMaps` checks them (layouts against each other, the named maps' settings, the new game), then the named maps' two files written, all or none. The catalogs are neither checked nor written. Extract the shared map checks from `WriteMapCatalogsAndMaps` into one private method both call (no copy).
- **Editor `Maps/OpenMaps`:** `public ContentCatalog ContentWith(ContentCatalog saved)`: `saved` with every open map as it is now in place of its saved one (a new map added); and `public ContentCatalog ContentWith(ContentCatalog saved, MapViewModel only)`: with that one open map only.
  - Name both in the summary: "the content a save checks: every open map, for Save all; the shown one, for Save".
- **Editor `Shell/MainViewModel.Saving.cs`:**
  - `Save`: one `TryWrite` of `WriteMapCatalogsAndMaps(_openMaps.ContentWith(saved, shown), [shown.Map.Id])` when `content.IsDirty`, else `WriteMaps(...)`; with no map shown, `WriteMapCatalogs` as today. On success: `content.MarkSaved()` when the catalogs were written, `AddSavedMap` and `MarkSaved` on the map.
  - `SaveAll`: the same two calls with `_openMaps.ContentWith(saved)` and every id in `_openMaps.Dirty`; on success every dirty map is added and marked saved.
  - On a refused combined write with `content.IsDirty`: `WriteMapCatalogs(content.Current)` alone (it checks the saved maps, as today's Ctrl+S); when that passes, `content.MarkSaved()` and `SaveError` = "{the map refusal} The tiles and objects were saved; no map was written."; when it fails too, the map refusal alone ("Nothing was written.").
  - `TryWriteCatalogs`/`TryWriteMap` go (replaced by one `TryWriteTogether(ContentCatalog checkedContent, IReadOnlyList<MapViewModel> maps)` and the fallback above).
  - Summaries: Save all's "the first the game would refuse stops it, the maps written before it staying saved" becomes "all or none"; grep "written before" and "stops it" in `Shell/` summaries and `ControlsHelpWindow.xaml`.

## Tests
- `ContentWriterMapsTests` (65 lines; this slice adds about 60):
  - `WriteMaps_TwoMapsValidOnlyTogether_WritesBoth`: on a copy of the test world, the forest's size swapped and the start map's warp moved into the forest's new cells; both written, loaded back.
  - `WriteMaps_OneMapInvalid_WritesNothing`: the start map's layout and the forest's bytes unchanged.
  - `WriteMaps_CatalogsChanged_LeavesTheCatalogFilesUntouched`: a tile blocked in the content passed in; `tiles.json`'s bytes unchanged.
- `MainViewModelSaveTogetherTests` (new, Editor `Shell/`, a copy of the test world through `EditorTestContent`):
  - `SaveAll_ResizedMapAndTheWarpMovedIntoItsNewCells_WritesBoth` (the reproduction): on start, a cell event warping to forest at (width - 1, 0) saved first; forest resized to (width - 1, height + 2) (the warp now outside); the warp moved to (0, height + 1) (outside the saved forest, inside the new one); Save all leaves `SaveError` null and both maps load back with the new size and the new cell.
  - `Save_ShownMapNeedsAnotherOpenMapsChange_RefusesAndWritesNothing`: the same set-up, Ctrl+S on start: refused, both files unchanged.
  - `Save_CatalogChangeValidOnlyWithTheShownMapsChange_WritesBoth`: an object only the forest places (a new object made and placed by the test, then saved), its placement removed and then the object removed from the catalog (Remove object reads the open maps, decision 502); Save on forest writes `objects.json` and the forest together (refused today: the catalogs went first, checked against the saved forest).
  - `SaveAll_OneMapRefused_WritesNoMapAndKeepsEveryMark`.
  - `SaveAll_MapRefusedAndTilesChanged_WritesTheTilesAlone` (a tile blocked and an unknown tile painted on forest: `tiles.json` written, the forest not, the error line ends "The tiles and objects were saved; no map was written.").
  - `Save_MapRefusedAndTilesChanged_WritesTheTilesAlone` (the same through Ctrl+S).
  - `Save_MapRefusedAndTilesRefusedAlone_WritesNothing` (a catalog change only the shown map's draft makes valid, and that map refused for another reason: nothing written, "Nothing was written.").
- Re-pinned in `MainViewModelSavingTests` (decision 518 replaces decision 500's partial write; the old expectations are wrong now, said in the Done note):
  - `SaveAll_SecondMapRefused_KeepsTheFirstSavedAndShowsTheReason` becomes `SaveAll_SecondMapRefused_WritesNeitherAndShowsTheReason` (forest stays dirty and unwritten).
  - `SaveAll_FirstMapRefused_LeavesTheNextUnwritten` becomes `SaveAll_FirstMapRefused_WritesNeither` (same assertions on start, plus forest unwritten).

- **Done, with deviations:** the reproduction and the six other new tests all failed on the old code first; the reproduction's message was "map-layouts/start.json: cell event 'save_check' at ... step 0: warp cell (0, 18) is outside map 'forest'." `TryWriteTogether` is named `SaveTogether` (it marks the drafts saved or falls back, so it is no `Try`; the review's Critical). `OpenMaps.ContentWith(saved, only)` is static (it reads no open map but the one given). `PaintUnknownTile` moved from `MainViewModelSavingTests` into `EditorTestContent` (its second user), with `LayoutBytes` and `ObjectsPath` beside it. `ContentWriter.WriteMap` stays for its Data tests, with no editor caller left. The two re-pinned tests are `SaveAll_SecondMapRefused_WritesNeitherAndShowsTheReason` and `SaveAll_FirstMapRefused_WritesNeither`: decision 500's partial write was the old expectation.

## Verify by hand
1. In the editor: File > New map..., name "Save Check", width 10, height 8, OK; Ctrl+S.
2. Open the map "start" in the map list; the Events tab; click a free cell; in the dialog Add step > Warp; Pick...; choose "Save Check (save_check)", click its top-right cell (9, 0); OK; OK; Ctrl+S (saved).
3. Open "Save Check"; Map > Settings...: width 8, height 12; OK; answer Yes to the resize question.
4. Open "start"; click the cell with the warp (Events tab); Pick...; click the bottom-left cell (0, 11); OK; OK.
5. Ctrl+S on "start": refused, naming `map-layouts/start.json` (warp outside, the saved "Save Check" being 10 x 8), "Nothing was written."; expected, SV2 adds the hint.
6. File > Save all: no "Not saved" line; both rows lose their " *".
7. Open "Save Check"; Map > Settings...: width 5, height 5; OK; Yes (start's saved warp at (0, 11) is now outside it). Tiles tab: tick Mark blocked, click one sheet cell to flip it, untick. File > Save all: refused, naming `map-layouts/start.json`, the line ending "The tiles and objects were saved; no map was written."; "Save Check" keeps its " *"; the flipped mark is still there after closing (Don't save) and reopening the editor.
8. Revert: `git checkout -- content` then delete `content/maps/save_check.json` and `content/map-layouts/save_check.json`.
