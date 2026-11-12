# SI3. The sprite list, import and Copy setup from...

Editor, about 220 lines (110 of them tests). Decisions 569, 570. Builds on SI1, SI2.

## Goal
The Character sprites window's state: every PNG in the field-sprite folder, set up or not, then every record whose image is missing (as the sheet box lists tilesheets, decision 442); choosing one opens its setup; Import... copies images in through Data, asking before replacing one; **Copy setup from...** copies another set-up sheet's layout onto the chosen image (the usual case: a second character from the same pack); OK gives the draft every changed record as one undo step.

## New types
- `Editor/Sprites/FieldSpriteChoice` (internal sealed record): one row of the list: the file, whether a record sets it up, whether its image is missing; `Label` ("villager.png", "villager.png (not set up)", "old.png (image missing)").
- `Editor/Sprites/FieldSpriteCatalogViewModel` (internal sealed partial, `ObservableObject`): the list and the chosen image's `FieldSpriteSetupViewModel`; the edited setups kept per file while the window is open; `CopySources` (the other set-up files) and `CopySetupFrom(string file)` (frame size as columns and rows for this image, scale, feet, the walk's rows, frames and time, decision 570; a frame size this image doesn't split into shows as the setup's problem); `Problem` (the chosen setup's, then any other edited one's, named by file); `ToSheets()`. Built with an image-size lookup (`Func<string, (int Width, int Height)?>`), never reading files itself.
- `Editor/Sprites/FieldSpriteImport` (internal sealed): copies chosen images through `ContentWriter.ImportArtImage(ArtFolders.FieldSprites, ...)`, asking before a replacement (decision 125's rule), then `TileImages.Forget` per copied file; returns imported files and refusals, as `Tiles/SheetImport`.
- `Editor/Sprites/FieldSpriteEditingViewModel` (internal sealed): `Start()` and `Apply(edited)` (`CatalogDraft.SetFieldSprites`), as `Quests/QuestEditingViewModel`.

## Changes
- `FieldSpriteCatalogViewModel.Refresh()` after an import: the list rebuilt, the chosen file kept, an imported image without a record shown "(not set up)" and chosen.

## Tests
`FieldSpriteCatalogViewModelTests` (new): `Choices_FolderAndRecords_ListsEveryImageThenMissingOnes`; `Choices_ImageWithoutRecord_MarkedNotSetUp`; `Choose_SetUpImage_StartsFromItsRecord`; `Choose_NewImage_StartsFromTheGuess`; `Choose_BackToEditedImage_KeepsItsEdits`; `ToSheets_OneImageEdited_ReturnsOnlyIt`; `Problem_OtherEditedImageBroken_NamesItsFile`; `Refresh_AfterImport_ChoosesTheImportedImage`.
`FieldSpriteCopySetupTests` (new): `CopySetupFrom_SameSizedSheet_CopiesEveryField`; `CopySetupFrom_FrameSizeNotSplittingThisImage_ShowsProblem`; `CopySources_ExcludeTheChosenFile`.
`FieldSpriteImportTests` (new): `Import_ExistingFile_AsksBeforeReplacing`; `Import_Refused_ReportsDataMessage`.
`FieldSpriteEditingViewModelTests` (new): `Apply_EditedSetup_IsOneUndoStep`; `Start_AfterUndo_ReadsTheDraftAsItIs`.

## Verify by hand
None: the window is SI4.
