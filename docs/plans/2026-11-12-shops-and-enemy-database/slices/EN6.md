# EN6. Enemy art: battle sprite, portrait and attack effect picked or imported

Editor, about 180 lines (90 of them tests). Decisions 589 (the picker pattern), 590. Builds on EN2, NP2 (`ArtFolders.ImagesIn`) and plan 1's SI1 (`ContentWriter.ImportArtImage`, reused as it is).

## Goal
An enemy's three pictures are picked from their folders (battle sprite from `battlers/`, portrait from `portraits/`, attack effect from `effects/`) in editable lists, with **Import...** beside each copying a PNG in through Data (asking before a replacement; an effect strip that isn't whole square frames refused by SI1's check), and the flip box. A missing file is allowed and marked (decision 442).

## New types
- `Editor/Battle/EnemyArtViewModel` (internal sealed partial, `ObservableObject`): the three file names, `IsSpriteFlipped`, the three folders' choices (lookups given, refreshed after an import), `MissingFiles` (which of the three the folder lacks, for the marks); `From(EnemyDefinition)`; writes its fields into the enemy record.
- `Editor/Battle/BattleArtImport` (internal sealed): copies chosen images into one of the three folders through `ContentWriter.ImportArtImage`, asking before a replacement, returning the copied names and refusals (as `Tiles/SheetImport` and plan 1's `Sprites/FieldSpriteImport`).

## Changes
- `Editor/Battle/EnemyViewModel`: `Art` (`EnemyArtViewModel`), carried into `ToEnemy`.

## Tests
- `EnemyArtViewModelTests` (new): `From_Goblin_ReadsThreeFilesAndFlip`; `MissingFiles_SpriteNotInFolder_MarksIt`; `Choices_AfterImport_ListTheNewFile`; `ToEnemy_FlipTicked_WritesIt`.
- `BattleArtImportTests` (new): `Import_ExistingFile_AsksBeforeReplacing`; `Import_EffectNotWholeFrames_ReportsDataMessage`.

## Verify by hand
None: the window is EN7.

- **Done, with deviations:** the folders' files live in `BattleArtChoices` on the catalog view model (one lookup given by the shell, `Refresh()` after an import, `MissingOf(art)` for the marks) rather than on each enemy's art view model, which only holds the three names and the flip and raises the enemy's `Art` on a change; `BattleArtImport.Import` takes the folder, so one import serves the three boxes; `EnemyCatalogViewModel` and `EnemyEditingViewModel` take the folder lookup as a constructor parameter (the tests pass an empty one).
