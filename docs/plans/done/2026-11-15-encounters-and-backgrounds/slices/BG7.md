# BG7. The Battle backgrounds page's rules: every background, still or animated, and its problem line

Editor (`Backgrounds/`, a new folder; `Catalogs/`); about 320 lines with tests (about 150 of code and the extraction's
caller edits, 18 tests). **Split point, if it passes 300 at the size check:** `Catalogs/ArtImageRows` with its two
callers and its tests first (about 80 lines), the rest second. Needs BG3, BG4, the mode bar plan's stage 1
(`Workspaces/IEditedCatalog`) and row 7's AN1 (`FramePreview` owns the playing). Decisions 897, 901, 902. Design: "New
types", "Replacement mapping".

## Goal
The Battle backgrounds page's state, headless: every image of the backgrounds folder in name order, then every record
whose image is missing; each still or animated with a line saying which; an animated one's frame time as typed (the
box `FramePreview` shows) and the cells its preview plays; the problem line (the chosen background's, then another's
by file, then Data's for the whole list); the records Apply hands back. The image list's rule, now in its third
editor, is extracted.

## New types
- `Catalogs/ArtImageRows` (internal static): an art folder's images in the order given, then the records' files whose
  image is missing, by file (decision 442). `static IReadOnlyList<ArtImageRow> Of(IReadOnlyList<string> images,
  IEnumerable<string> recordFiles)`.
- `Catalogs/ArtImageRow` (internal sealed record): one of them. `(string File, bool IsImageMissing)`.
- `Backgrounds/BattleBackgroundViewModel` (internal sealed partial, `ObservableObject`): one background image as the
  page shows it.
  - `static BattleBackgroundViewModel From(string file, AnimatedBackgroundDefinition? record, (int Width, int Height)?
    size)`; `size` null when the image is missing or unreadable;
  - `string File`, `bool IsImageMissing`, `bool IsAnimated` (has a record);
  - `[ObservableProperty] FrameMillisecondsText` (an animated one's frame time as typed, bound to the preview's
    `FrameTimeText`; empty for a still), notifying `Problem`;
  - `string KindLine`: "Still, 1920 x 1080 px"; "Animated: 4 frames of 960 x 540 px in a 2 x 2 grid"; "Image missing"
    (decision 442: listed, never refused);
  - `(int Width, int Height)? FrameSize` and `IReadOnlyList<SpriteCell> PreviewCells`: an animated one's cells in play
    order, from `FrameOrigin` over its image's width (each origin divided by the frame size), for row 7's
    `PreviewFrames.OfCells`; empty for a still (the page shows the whole image as one frame);
  - `string? Problem`: the typed time first ("Frame time must be a whole number of milliseconds from 1."), then
    `ArtReferenceCheck.GridProblemOf(record, size)` when the size is known (BG4's public entry);
  - `AnimatedBackgroundDefinition? ToRecord()`: the record with the typed time; null for a still; throws
    `InvalidOperationException` on a `Problem`.
  - The class summary: reads no file; the size comes in from the list's lookup. The imports' members come with their
    callers: `TakeStillImage` with BG8's PNG import, `TakeAnimation` with BG9's GIF import.
- `Backgrounds/BattleBackgroundCatalogViewModel` (internal sealed partial, `ObservableObject`, `Workspaces/IEditedCatalog`):
  the Battle backgrounds page's state.
  - `BattleBackgroundCatalogViewModel(ContentCatalog current, IReadOnlyList<string> images, Func<string, (int Width,
    int Height)?> sizeOf)`;
  - `ObservableCollection<BattleBackgroundViewModel> Backgrounds` (the imports add rows);
  - `[ObservableProperty] SelectedBackground` (the first row on starting), notifying `Problem`;
  - `string? Problem` (raises its change, as the page recipe needs): `SelectedBackground?.Problem`, then the first
    other row's as "<file>: <problem>", then `CatalogProblems.OfAnimatedBackgrounds(records)`;
  - `IReadOnlyList<AnimatedBackgroundDefinition> ToRecords()`: every animated row's record, sorted by file; throws on a
    `Problem`;
  - `IEditedCatalog`: `ChosenKey` (the chosen file), `Choose(key)` (an unknown file keeps the choice);
  - a row's typed `FrameMillisecondsText` refreshes the problem line; its own computed notices are skipped (the editor
    rule on relayed `PropertyChanged`).

## Changes
- `Battle/EffectStripCatalogViewModel` and `Sprites/FieldSpriteCatalogViewModel.BuildChoices` (or their page
  successors from the mode bar plan): build their lists over `ArtImageRows.Of`. Their tests pass unchanged; that is the
  guard.
- `docs/map.md`: `Catalogs` names `ArtImageRows`; a new Editor row, `Backgrounds`, names the two view models (BG8 and
  BG9 extend it).

## Tests
- `ArtImageRowsTests` (Editor, new):
  - `Of_ImagesAndRecords_ImagesFirstInTheGivenOrder`
  - `Of_RecordWithoutImage_ComesAfterMarkedMissing` (records given out of order: sorted by file)
  - `Of_RecordWithItsImage_ListedOnce`
- `BattleBackgroundViewModelTests` (Editor, new; records built in the test, 960 x 540 frames, 4 in 2 x 2, 200 ms, a
  1920 x 1080 size):
  - `From_NoRecord_IsStillWithItsSize`
  - `From_Record_IsAnimatedWithItsTime` (the box reads "200")
  - `From_SizeNull_IsImageMissing`
  - `PreviewCells_TwoByTwoGrid_AreTheCellsInRowOrder` (cell 2 is column 0, row 1)
  - `Problem_TimeZero_NamesTheFrameTime` and `Problem_TimeOne_IsNull` (the bound's two sides)
  - `Problem_ImageTooSmallForTheGrid_IsDatasGridLine` (a 960 x 1080 size)
  - `ToRecord_TypedTime_IsTheRecordWithThatTime` (typed 350, a value the fixture doesn't hold)
  - `ToRecord_WithAProblem_Throws`
- `BattleBackgroundCatalogViewModelTests` (Editor, new; a catalog built in the test with one record, images "A.png" and
  "Pulse.png", and a record "Gone.png" whose image is missing):
  - `Constructor_ImagesThenMissingRecords_InThatOrder`
  - `Problem_AnotherRowsTimeInvalid_NamesItsFile` (the chosen row fine), raising `Problem`'s change
  - `ToRecords_TwoAnimated_SortedByFile` (rows set up in the reverse order)
  - `Choose_KnownFile_ChoosesIt`, `Choose_UnknownFile_KeepsTheChoice`, `ChosenKey_RowChosen_IsItsFile` (the page
    recipe's three)

## Verify by hand
None: no page yet (BG8). Open the Effect strips and Character sprites pages once (`open-editor-windows.ps1`'s default
list): no `ERROR` line. Read the script's lines only: the Character sprites page shows Mana Seed sheets, so its capture
is the developer's to look at (he checks its list is in the same order as before).
- **Note from BG3 review:** the Data validator and problems tests each build the same `_pulse` record (960 x 540, 4
  frames, 200 ms); if this slice's tests need it a third time, move it to one shared test fixture.
- **Note from BG4 review:** `ArtReferenceCheck.GridProblemOf` divides by the frame size and reads `Animation`: the
  page's problem line calls it only on a record `AnimatedBackgroundCatalogValidator` accepts (a 0 frame width throws
  `DivideByZeroException`; the page may hold a record with no animation, BG3).
- **Note from BG7 (the split):** the size check estimated about 580 lines with tests (the two view models about 230 of
  code with their doc comments, 15 tests and a fixture), past ~320, so this run stopped at the named split point; the
  two view models, their tests, the map's `Backgrounds` row and the BG3 and BG4 review notes moved to
  [BG7b](BG7b.md), which names its own split point (about 450 lines).
- **Done, with deviations:** split at the named point: `Catalogs/ArtImageRows` (23 lines) and `ArtImageRow` (6) under
  the Effect strips (168) and Character sprites (193) pages' lists, their tests unchanged; `ArtImageRowsTests` (36, 3
  tests). The Effect strips list keeps its name order by passing the folder's images sorted. Tests: 7199. The smoke
  script opened both pages with no `ERROR` line; the Character sprites capture is the developer's to look at.
