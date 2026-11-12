# CW1. A save writes only the catalog files that changed

Data + Editor, about 240 lines (130 of them tests). Design: "Catalog writes". Decision 559. The review's blocker; plan 2 depends on it.

## Goal
Today any catalog change makes Ctrl+S rewrite every catalog file from the editor's loaded snapshot (`MainViewModel.Saving` calls `WriteMapCatalogs` when `content.IsDirty`; `CheckedCatalogWrites` writes them all), so a hand edit made to `objects.json` or `field-sprites.json` while the editor is open is silently reverted. After this slice a save writes only the catalog files whose content differs from what was last read or written, and refuses, writing nothing, when such a file was changed on disk since then, naming it.

## New types
- `Data/Content/CatalogFileStamps` (public sealed): per catalog file, the hash (SHA-256) of its bytes on disk and of its content as the writer serializes it, as last read or written. `Take(string contentRoot, ContentCatalog loaded)` after a load; `IsChanged(file, contentBytes)`; `IsEditedOnDisk(file)`; `Renew(file, bytes)` after a write. No other mutable state; one instance per editor session.

## Changes
- `Data/Content/ContentWriter`: built with the stamps (`ContentWriter(string contentRoot, CatalogFileStamps stamps)`); `CheckedCatalogWrites` still validates every catalog, then serializes each in memory and keeps only the files `IsChanged`; before writing, any kept file `IsEditedOnDisk` throws `ContentValidationException` ("objects.json changed on disk since the editor read it: ..."), nothing written; after `WriteAll`, each written file's stamp is renewed. `WriteAll` takes the bytes already serialized for catalogs (no second serialization). The map writes and `RenameMap` are unchanged but renew the stamps of any catalog file they write (`new-game.json` is not a catalog; unchanged).
- `Editor/Shell/MainViewModel` (constructor, one line): takes the stamps right after `ContentCatalogLoader.Load` and builds the writer with them; the purge (`SheetRemoval`) and the rename go through the same writer.
- Test helpers building a `ContentWriter` (grep `new ContentWriter(`: count the sites first; a `ContentWriterFixtures.For(root)` helper taking the stamps if more than five).
- `docs/map.md`: the Data `Content` row names `CatalogFileStamps`.

## Tests
- `ContentWriterStampTests` (new sibling): `WriteMapCatalogs_OnlyObjectsChanged_WritesOnlyObjectsFile` (other files' write times and bytes untouched); `WriteMapCatalogs_NothingChanged_WritesNothing`; `WriteMapCatalogs_UnchangedFileEditedOnDisk_KeepsTheHandEdit`; `WriteMapCatalogs_ChangedFileEditedOnDisk_RefusesNamingFileAndWritesNothing`; `WriteMapCatalogs_TwoSavesInARow_SecondWritesNothing` (stamps renewed); `WriteMapCatalogs_HandLaidFileUnchanged_IsNotRelaid` (a catalog file in a hand layout, not changed in the draft, stays byte for byte).
- `ContentWriterRepositoryTests`: still green (the identical-bytes test now proves the writer's serialization matches, through a changed draft).
- `MainViewModelSaveStampTests` (new, Editor): `Save_ObjectsEditedOnDiskWhileOpen_ShowsRefusalNamingFile`; `Save_TileChangedAfterHandEditToQuests_KeepsQuestsHandEdit`.

## Verify by hand
1. Start the editor on the repository's content. In a text editor, change an object's `category` in `content/objects.json` and save it.
2. In the editor, paint a tile on a sheet cell that isn't a tile yet (a tiles.json change) and press Ctrl+S: the save goes through; `git diff content` shows `tiles.json` and your hand edit to `objects.json`, both kept.
3. Now make an object change in the editor (Objects > Edit shapes..., move a point, OK) and Ctrl+S: the save is refused, the error line naming `objects.json`. Close the editor without saving, reopen it: your hand edit is there; redo the shape change and save.
4. Revert with `git checkout -- content`.

- **Done, with deviations:** the stamps are an internal type the writer owns and fills through `ContentWriter.StampCatalogs(loaded)`, called by the editor right after the load, so the sixteen test sites that build a writer are untouched (a writer never stamped writes every file, as before, with a test saying so); the image imports moved to `ContentWriter.ArtImports.cs` now (the file had reached 299 lines), ahead of SI1. The review added: a file already holding the bytes about to be written is not refused (a save whose moves failed partway), and the hand-edit mismatch risk is under the design's Risks.
