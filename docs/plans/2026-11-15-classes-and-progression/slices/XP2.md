# XP2. The experience table as typed, one undo step

Editor, about 220 lines (about 120 of them tests). Needs XP1. Design: sections 1, 3, 7 (risk 3); decisions 943, 956;
the mode bar's WS12 recipe, steps 1, 2 and 9.

## Goal
The Experience page's state, headless: one row per level step ("Level 4 to 5"), its experience as a typed box and the
total experience to reach that level (read-only, blank while a box above it can't be read); Add level appends a row
copying the last one's amount; Remove last level takes the last row out, greyed at one row. The problem line names the
first box that isn't a whole number ("Level 4 to 5 must be a whole number"), then Data's message
(`CatalogProblems.OfRules` on the draft's rules with the typed table: at least one entry, each at least 1, and no class
skill learned past the max level). A note under the table says when the max level falls below the saved one: "Max level
18, was 21: saved members above it drop to 18 when a save loads." Applying records one
`CatalogDraft.SetExperienceTable` step, nothing when unchanged. The view model is an `IEditedCatalog` with nothing to
choose.

## New types
- `Progression/ExperienceRowViewModel` (`ObservableObject`): one level step's experience as typed, with its running
  total. `Level` (the step starts at), `Label` ("Level 4 to 5"), `[ObservableProperty] AmountText`, `long? TotalToReach`
  (set by the table: a `long`, since rows near the largest whole number add past it).
- `Progression/ExperienceTableViewModel` (`ObservableObject`, `IEditedCatalog`): the table as the Experience page types
  it.
  - `ExperienceTableViewModel(ContentCatalog content)`; `ObservableCollection<ExperienceRowViewModel> Rows`;
    `int MaxLevel` (rows + 1); `string? MaxLevelNote` (raised whenever the rows change).
  - `[RelayCommand]` `AddLevel`, `RemoveLastLevel` (greyed at one row).
  - `ChosenKey` is null (one table); `Choose` changes nothing.
  - `Problem` (raises its change), `CanApply`, `ExperienceTable ToTable()` (throws on a problem); totals recomputed when
    any amount changes.
- `Progression/ExperienceEditingViewModel(CatalogDraft catalog)` (`ICatalogEditing<ExperienceTableViewModel>`): starts
  the Experience page from the draft and applies its session as one step.

## Changes
- `Catalogs/CatalogDraft.Database.cs` (118 lines, about 135 after): `SetExperienceTable(ExperienceTable table)`, one
  step replacing `Rules` with `Rules with { Experience = table }`; nothing when the lists are equal by sequence. No undo
  refusal (decision 943's repair); the summary says so.
- `docs/map.md`, Editor table: a `Progression` row ("The Experience page's rules: the table as typed with its totals,
  the fill from a curve"; `ExperienceTableViewModel`, `ExperienceRowViewModel`, `ExperienceEditingViewModel`).

## Tests
- `Progression/ExperienceTableViewModelTests` (new; a draft over the test world `with` a table built in the test:
  `[10, 20, 40]`):
  - `Constructor_Table_ListsOneRowPerStep` (three rows, max level 4, labels "Level 1 to 2" ...).
  - `TotalToReach_SumsTheRowsAbove` (10, 30, 70); `TotalToReach_BoxAboveNotANumber_IsBlank`.
  - `TotalToReach_RowsNearIntMax_DoesNotOverflow` (two rows of `int.MaxValue`).
  - `AddLevel_CopiesTheLastAmount`; `RemoveLastLevel_OneRow_IsGreyed`; `RemoveLastLevel_TakesTheLastOut`.
  - `Problem_RowNotANumber_NamesTheLevelStep`; `Problem_ZeroAmount_ShowsDataMessage` and `Problem_AmountOne_IsNull`.
  - `Problem_ClassSkillPastNewMax_ShowsDataMessage` (a class built in the test learning a skill at level 4; one row
    removed).
  - `MaxLevelNote_FewerLevels_SaysTheDrop`; `MaxLevelNote_SameOrMore_IsNull`;
    `MaxLevelNote_LevelAddedBack_ClearsAndRaisesChanged`.
  - `ChosenKey_IsNull`; `Choose_AnyKey_ChangesNothing`.
- `Catalogs/CatalogDraftExperienceTests` (new): `SetExperienceTable_ChangedRow_RecordsOneStep`;
  `SetExperienceTable_SameList_RecordsNothing`; `SetExperienceTable_Undo_RestoresTheTable`;
  `SetExperienceTable_KeepsTheOtherRules`.
- `Progression/ExperienceEditingViewModelTests` (new): `Apply_EditedRow_ChangesTheDraftsTable`.

## Verify by hand
None: the page comes in XP3.
