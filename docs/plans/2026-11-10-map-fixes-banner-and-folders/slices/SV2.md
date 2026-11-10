# SV2. A refused Ctrl+S that Save all would pass says so

Data + Editor, about 110 lines. Builds on SV1 and design.md, "Key decisions" 2. Decision 519 (Q2).

## Goal
When Ctrl+S is refused (it checks the other maps as saved) and the same checks pass with every open map as it is now, the error line names the other open maps with changes and points to Save all, which writes them together. It works both ways: the map holding the warp refused by the resized map's saved file, and the resized map refused by the other map's saved warp. Ctrl+S still writes only the shown map.

## Changes
- `Data/Content/ContentWriter`: SV1's shared private map checks become `public void CheckMaps(ContentCatalog content)` ("every map's layout against the others, every map's settings and the new game, as the loader would; throws `ContentValidationException` naming the file; writes nothing"), still called by `WriteMaps` and `WriteMapCatalogsAndMaps`.
- `Shell/MainViewModel.Saving.cs`: after a refused Save with a map shown and other open maps with changes, a dry run `_writer.CheckMaps(_openMaps.ContentWith(saved))` (catching only `ContentValidationException`); when it passes, `SaveError` gets " Save all (Ctrl+Shift+S) writes map 'start' with it, which makes this valid." (every other dirty open map named, id order, "map 'a' and map 'b'" as `UnsavedChangesQuestion` joins them; one private `SaveAllHint(MapViewModel shown)` returning the sentence or null).
- `Shell/ControlsHelpWindow.xaml`: the Save all line says "every changed map and the tiles and objects, checked together, all or none".

## Tests
- `Data.Tests/Content/ContentWriterMapsTests`: `CheckMaps_ValidContent_WritesNothing` (the files' bytes unchanged), `CheckMaps_WarpOutsideItsMap_ThrowsNamingTheLayout`.
- `Editor.Tests/Shell/MainViewModelSaveTogetherTests` (SV1's file):
  - `Save_ResizedMapRefusedByTheOthersSavedWarp_PointsToSaveAll` (SV1's set-up, Ctrl+S on forest: the line names map 'start').
  - `Save_WarpMapRefusedByTheResizedMapsSavedFile_PointsToSaveAll` (Ctrl+S on start: the line names map 'forest').
  - `Save_RefusedAndSaveAllWouldBeRefusedToo_GivesTheMessageAlone` (the shown map with an unknown tile while another map is dirty: no hint).
  - `Save_RefusedWithNoOtherMapChanged_GivesTheMessageAlone`.

## Verify by hand
SV1's steps 1-4, then:
1. Ctrl+S on "start": the "Not saved" line names `map-layouts/start.json` and ends "Save all (Ctrl+Shift+S) writes map 'save_check' with it, which makes this valid."
2. Open "Save Check", Ctrl+S: refused naming `map-layouts/start.json`, the hint naming map 'start'.
3. File > Save all saves both. Revert as SV1.
