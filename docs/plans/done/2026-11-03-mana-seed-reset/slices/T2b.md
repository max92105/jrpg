# T2b. Editor tests on the test world; the deleted tilesheets staged

Builds on T1 and design.md, "Key decisions" 1.

## Goal
Every Editor test that opens a map, paints, cuts a sheet, reads tiles or objects, or lists the tilesheet images loads or copies `test-world`. After it, no test reads a tilesheet image from `content/`, so the 23 sheets no sheet names, which S0 restored (`Floor.png`, `Grass3.png`, `RA_Jungle_Extras.png`, `Wall.png`, `Water3.png`, `Water4.png` and the seventeen `tree0x_s_0y_animation.png` other than `tree01_s_01_animation.png`), leave with `git rm` in this slice.

## Changes
- `tests/Jrpg.Editor.Tests/Shell/EditorTestContent`: `RepositoryContent` becomes `TestWorld` (the `test-world` folder), and `CopyOfRepositoryContent` becomes `CopyOfTestWorld`; its summary says so. Its users (`MainViewModelTests`, `MainViewModelUndoTests`, `MainViewModelSaveTests`, `MainViewModelMusicTests`, `CombinedHistoryQuestTests`) follow the rename.
- The Editor tests that load `content` and read the map side switch their path: `Tiles/TileBrowserViewModelTests`, `Tiles/TilePaintingViewModelTests`, `Tiles/WalkabilityMarkingTests`, `Tiles/SheetImportTests`, `Objects/ObjectBrowserViewModelTests`, `Objects/ObjectPlacingViewModelTests`, `Objects/NewObjectViewModelTests`, `Maps/CollisionShapesTests`, `Maps/MapViewModelTests`, `Catalogs/CatalogDraftTests`, `Catalogs/CatalogUsageTests`, `Events/EventEditingViewModelTests`, and any other the grep `BaseDirectory, "content"` finds that does. `Quests/QuestEditingViewModelTests`, `Catalogs/CatalogDraftQuestTests`, `Catalogs/CatalogUsageQuestTests` and `Events/EventPresetsTests` switch only if they read a map.
- Comments naming "the repository's" sheets, tiles or maps in the moved files read "the test world's" (`MainViewModelTests` line 192's `Grass3.png` comment among them).

## New types
None.

## Tests
None new: the guard is the Editor test count before and after, and every test passing, `MainViewModelTests`' and `MainViewModelUndoTests`' `Grass3.png` tests included, now on the test world's copy.

## Verify by hand
- `git status` shows the 23 sheets deleted with the slice; the editor opens on the repository's content (the ten named sheets stay).

- **Done, with deviations:** 513 Editor tests before and after, every one passing. Switched one more file than listed, `Catalogs/CatalogUsageQuestTests` (it gates events on the start and forest layouts); `Events/EventPresetsTests` switched too (it reads the objects). `Quests/QuestEditingViewModelTests` and `Catalogs/CatalogDraftQuestTests` stay on `content`: quests only. Test names saying "Repository" for data now read from the test world say "TestWorld" (the T2a review's rule).
  One Data test moved early: `Art/ArtFoldersTests.TilesheetImagesIn_...` counted the 34 repository images and listed the last tree sheet, so it reads the test world now, with its count derived from the folder (never a total, `tests.md`).
