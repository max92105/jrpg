# T2c. Data tests and the encounter-rate simulation on the test world

Builds on T1 and design.md, "Key decisions" 1 and 2, and "Replacement mapping".

## Goal
Every Data test and simulation that reads the map side or starts a session for a mechanism runs on `test-world`. The tests of what the repository holds stay on `content`, pinning ids only, and the new game tests that pin a cell of the repository's maps move to the test world, so R1 can restate them without the cell.

## Changes
- `tests/Jrpg.Data.Tests/Content/CopiedContentRoot` copies `test-world` (its users write and break maps and catalogs); its summary says "a copy of the test world's content". The writer tests that copy content themselves (`ContentWriterTests`, `ContentWriterShopTests`, `ContentWriterQuestTests`, `ContentWriterMusicTests`) switch their source path the same way, except the two below.
- **Stay on `content`:** `ContentWriterTests.WriteMap_UnchangedRepositoryMap_WritesIdenticalBytes` and `WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes`: they prove the writer keeps the repository's own files byte for byte, which is what the editor saves. The first's `InlineData` list (which lacks `to_aslas_1` today) becomes a `MemberData` reading the map ids from `content/map-layouts/`, so a rebuilt or new map is covered without an edit. They move into a sibling `ContentWriterRepositoryTests` (`ContentWriterTests` is at 289 lines).
- `Content/ContentCatalogLoaderTests`: the tests on the `tree` outline, the `villager`, the map list and the interior's display name move to the test world, their names' scenario becoming `TestWorld` (`Load_TestWorld_ReadsTheTreeOutline` style). **Stay on `content`:** `Load_StartMap_HasARestEvent` and `Load_RepositoryContent_ForestRollsAnEnemyWithAPhysicalResistance` (decision 363): they pin map ids, which stay (decision 447), and say something the repository must keep. The skill, item, class and quest tests stay.
- `Maps/MapFolderReaderTests`, `Session/SaveAndLoadTests`, `Saving/SaveGameFixtures` and the repair tests that start a session: path switched.
- `Session/RepositoryNewGameTests`: `StartNew_RepositoryContent_SkippedIntroWarpsToTheStartMap` (the warp to (23.5, 20.5)), `StartNew_RepositoryContent_DoorLeadsIntoInterior` (below the door at (5.86, 14.6)) and `StartNew_RepositoryContent_EveryLeaderHasOneIntroEndingOnTheStartMap` (`WarpStep("start", (23, 20))`) move, renamed `_TestWorld_`, into a new `Session/TestWorldNewGameTests`. R1 restates the first and last without the cell.
- `tests/Jrpg.Simulations/Exploration/ForestEncounterRateSimulation`: loads `test-world` (it measures the rule against the configured odds from the frozen forest's cell (28, 7), not the repository's geography); its comment says so.
- Also on `content`: `RepositoryEncounterTests`, `RepositoryLootTests`, `RepositoryGearTests`, `RepositoryShopTests`, `RepositoryForgeTests`, `RepositoryInnTests`, `RepositoryQuestTests` (map ids and uids, decision 447), `ArtReferenceCheckTests.FindProblems_RepositoryContent_FindsNone`, `AudioReferenceCheckTests`' repository test, `SoundSourcesTests`, `ContentRootLocatorTests`, `UiMessageLoaderTests`, the skill sound, shop and crafting loader tests, the battle and loot simulations.

## New types
- `Data.Tests/Session/TestWorldNewGameTests`: the new game's map mechanisms on the frozen world.
- `Data.Tests/Content/ContentWriterRepositoryTests`: the writer keeps the repository's own files byte for byte.

## Tests
Moved, not new: `TestWorldNewGameTests: StartNew_TestWorld_SkippedIntroWarpsToTheStartMap`, `StartNew_TestWorld_DoorLeadsIntoInterior`, `StartNew_TestWorld_EveryLeaderHasOneIntroEndingOnTheStartMap`; `ContentWriterRepositoryTests: WriteMap_UnchangedRepositoryMap_WritesIdenticalBytes` (now one case per map in `content/map-layouts/`, `to_aslas_1` included), `WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes`. The guard is the Data and Simulations test counts before and after (plus one: the `to_aslas_1` case).

## Verify by hand
- Temporarily move the forest's exit cell event in `content/map-layouts/start.json` one cell down: the Data tests and simulations pass. Revert.
