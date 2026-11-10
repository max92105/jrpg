# RN1. `MapIdRename` and `ContentWriter.RenameMap`

Data, about 260 lines (14 tests of about 13 lines and 90 of code; under 300, a Data slice). Builds on SV1 (the shared map checks), ADR 0042 and design.md, "Data changes". Decisions 530, 531.

## Goal
Data renames a map id and every content reference to it in one checked write: the map's two files under the new id (their `id` fields changed), every map whose warp steps lead to it, and `new-game.json` when the game starts there; then the old two files are deleted. A rename the game would refuse writes nothing.

## New types
- `Data/Maps/MapIdRename` (internal static): the content with one map's id, and every reference to it, renamed.
  - `public static RenamedContent Apply(ContentCatalog content, string oldId, string newId)` returning the new content and the ids of the maps whose layouts changed (besides the renamed one) and whether the new game changed. `RenamedContent` is a small internal record beside it (`Content`, `ChangedMapIds`, `IsNewGameChanged`): name it in the summary as "what a rename changed, so the writer writes only that".
  - References renamed: each `WarpStep.MapId` in every object event and cell event of every map (the renamed map's own warps to itself included), and `NewGameDefinition.StartLocation.MapId`. Steps are rebuilt with `with`, nothing else in a layout moves.
  - Refuses (`ArgumentException`) an unknown old id or a new id already used (the editor checks first; this is the backstop).

## Changes
- `Data/Content/ContentWriter`: `public void RenameMap(ContentCatalog content, string oldId, string newId)`:
  1. `MapIdRename.Apply`;
  2. every layout, the renamed map's settings and the new game checked as `WriteMapCatalogsAndMaps` checks them (SV1's shared private method);
  3. written: the renamed map's two files under the new id, every changed layout, `new-game.json` when changed (`ContentCatalogLoader.NewGameFile`), all aside first then moved, as `WriteAll` does;
  4. then the old two files deleted (deletes last: a failure leaves both maps, never none; design.md, "Risks").
  - Its summary says the old id stays in saves, which the repair handles (decision 532).
- `docs/map.md`: the Data Content row names `RenameMap`.

## Tests
`Data.Tests/Content/ContentWriterRenameTests` (new, on `CopiedContentRoot("writer-rename", "test-world")`):
- `RenameMap_ValidId_MovesBothFiles` (the new files exist with their `id` fields, the old ones are gone).
- `RenameMap_WarpsFromAnotherMap_FollowTheNewId` (a warp into the renamed map from another map, the cell unchanged).
- `RenameMap_WarpToItself_FollowsTheNewId`.
- `RenameMap_StartMap_NewGameFollows` (the test world's start map renamed; `new-game.json` names the new id; the catalog loads).
- `RenameMap_OtherMap_LeavesNewGameUntouched` (bytes unchanged).
- `RenameMap_UnrelatedMap_LeavesItsLayoutUntouched` (bytes unchanged).
- `RenameMap_NewIdTaken_WritesNothing`.
- `RenameMap_ContentInvalid_WritesNothingAndKeepsTheOldFiles` (an unknown tile in the passed content).
- `RenameMap_Renamed_LoadsBack` (the whole content loads with `ContentCatalogLoader.Load`).
`Data.Tests/Content/ContentWriterRepositoryTests` (63 lines): `NewGame_RepositoryFile_WritesBackUnchanged` (written through the writer's options to a temporary file: the same bytes as `content/new-game.json`; if it fails, re-lay `content/new-game.json` once as the writer writes it, and say so in the Done note).
`Data.Tests/Maps/MapIdRenameTests` (new): `Apply_CellEventWarp_Renamed`, `Apply_ObjectEventWarp_Renamed`, `Apply_WarpToAnotherMap_Untouched`, `Apply_UnknownOldId_Throws`.

## Verify by hand
Tests only: the editor's command comes in RN2-RN3. A save loaded after a rename is checked in RN3's hand check.
