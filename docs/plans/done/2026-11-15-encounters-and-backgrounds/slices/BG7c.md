# BG7c. The Battle backgrounds page's rules: the list, its problem line and the records Apply hands back

Editor (`Backgrounds/`); BG7b's split: BG7b landed `Backgrounds/BattleBackgroundViewModel` (one background, still or
animated, its typed frame time, kind line, preview cells and problem) and the Editor tests' shared record
(`Backgrounds/BackgroundFixtures.Pulse`, `PulseSheet`). About 200 lines with tests. Needs BG7b, BG7 (`Catalogs/ArtImageRows`),
the mode bar plan's stage 1 (`Workspaces/IEditedCatalog`). Decisions 897, 901, 902. Design: "New types", "Replacement
mapping".

## Goal
The Battle backgrounds page's list, headless: every image of the backgrounds folder in name order, then every record
whose image is missing (`ArtImageRows.Of`), each a `BattleBackgroundViewModel`; the problem line (the chosen
background's, then another's by file, then Data's for the whole list); the records Apply hands back.

## New types
- `Backgrounds/BattleBackgroundCatalogViewModel` (internal sealed partial, `ObservableObject`, `Workspaces/IEditedCatalog`):
  the Battle backgrounds page's state.
  - `BattleBackgroundCatalogViewModel(ContentCatalog current, IReadOnlyList<string> images, Func<string, (int Width,
    int Height)?> sizeOf)`, its rows from `Catalogs/ArtImageRows.Of`, each `BattleBackgroundViewModel.From(file,
    record, sizeOf(file))` (a missing image's size null);
  - `ObservableCollection<BattleBackgroundViewModel> Backgrounds` (the imports add rows);
  - `[ObservableProperty] SelectedBackground` (the first row on starting), notifying `Problem`;
  - `string? Problem` (raises its change, as the page recipe needs): `SelectedBackground?.Problem`, then the first
    other row's as "<file>: <problem>", then `CatalogProblems.OfAnimatedBackgrounds(records)`;
  - `IReadOnlyList<AnimatedBackgroundDefinition> ToRecords()`: every animated row's record (`ToRecord`), sorted by file;
    throws on a `Problem`;
  - `IEditedCatalog`: `ChosenKey` (the chosen file), `Choose(key)` (an unknown file keeps the choice); the mode bar's
    WS2 hasn't landed as of BG7b, so no `ICatalogEditing` unless it has by then;
  - a row's typed `FrameMillisecondsText` refreshes the problem line; its own computed notices (`Problem`) are skipped
    (the editor rule on relayed `PropertyChanged`).

## Changes
- `docs/map.md`: the `Backgrounds` row names `BattleBackgroundCatalogViewModel`.

## Tests
- `BattleBackgroundCatalogViewModelTests` (Editor, new; a catalog built in the test with one record,
  `BackgroundFixtures.Pulse` over `PulseSheet`, images "A.png" and "Pulse.png", and a record "Gone.png" whose image is
  missing):
  - `Constructor_ImagesThenMissingRecords_InThatOrder`
  - `Problem_AnotherRowsTimeInvalid_NamesItsFile` (the chosen row fine), raising `Problem`'s change
  - `ToRecords_TwoAnimated_SortedByFile` (rows set up in the reverse order)
  - `Choose_KnownFile_ChoosesIt`, `Choose_UnknownFile_KeepsTheChoice`, `ChosenKey_RowChosen_IsItsFile` (the page
    recipe's three)

## Verify by hand
None: no page yet (BG8).
- **Note from BG7b (the split):** BG7b named its own split point; this file holds its second part, as listed above.
- **Done, with deviations:** `Backgrounds/BattleBackgroundCatalogViewModel` (78 lines) and its tests (183, 16 tests:
  beyond the list, the empty folder choosing nothing, a missing image's record never looked up, the chosen row's own
  problem bare, the selection raising `Problem`'s change, Data's line for a record listed twice, a typed time in its
  record, a throw on a problem). The problem line's row part was its third copy (the Effect strips and Character
  sprites pages held the other two), so it is extracted as `Catalogs/ArtImageRowProblem` (29 lines,
  `ArtImageRowProblemTests` 41, 4 tests), its two older callers' tests unchanged; `EffectStripCatalogViewModel.cs` is
  168, `FieldSpriteCatalogViewModel.cs` 182. About 340 lines with tests, the extraction about 80 of them. Tests: 7243.
