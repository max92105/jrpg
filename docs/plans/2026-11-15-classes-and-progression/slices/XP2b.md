# XP2b. Add level, Remove last level and the max level note

Editor, about 130 lines (about 80 of them tests). Needs XP2 (`Progression/ExperienceTableViewModel`, its rows, its
fixture). Split from XP2 on 2026-11-17 (XP2's estimate passed 320 lines with its tests). Design: sections 1, 3, 7
(risk 3); decisions 943, 956.

## Goal
The Experience page's table gains its two list commands and its note: Add level appends a row copying the last one's
amount as typed; Remove last level takes the last row out, greyed at one row (Data needs one entry). Every change of
the list renews the totals, `MaxLevel`, the note and `Problem`. A note under the table says when the max level falls
below the saved one: "Max level 18, was 21: saved members above it drop to 18 when a save loads." (null at the same
or more levels).

## Changes
- `Progression/ExperienceTableViewModel`: the rows watched through `ListItemWatch.WatchRowsOf` (XP2 subscribes each
  row once, since its list never changes), its list change re-greying Remove last level and raising `MaxLevel`,
  `MaxLevelNote` and `Problem`; `[RelayCommand]` `AddLevel` and `RemoveLastLevel` (`CanExecute`: more than one row);
  `string? MaxLevelNote` against the saved max level, given by `ExperienceEditingViewModel.Start`.
- `Catalogs/CatalogDraft`: `int SavedMaxLevel`, the table's max level as last loaded or saved (set at construction
  and in `MarkSaved`), so an apply, which restarts the page, keeps the note until Save.
- `docs/map.md`, Editor `Progression` row: the two commands and the note.

## Tests
- `Progression/ExperienceTableViewModelTests` (or `ExperienceTableListTests` if the file passes 300 lines):
  - `AddLevel_CopiesTheLastAmount` (the label "Level 4 to 5", max level 5, the total).
  - `RemoveLastLevel_TakesTheLastOut`; `RemoveLastLevel_OneRow_IsGreyed` (a table of one entry);
    `RemoveLastLevel_ToOneRow_RaisesCanExecuteChanged`.
  - `Problem_ClassSkillPastNewMax_ShowsDataMessage` (the fixture's archer as `ClassFixtures.SkilledArcher`, expose at
    level 4; one row removed: "classes.json: class 'archer': skills lists 'expose' at level 4, past the max level 3.").
  - `MaxLevelNote_FewerLevels_SaysTheDrop`; `MaxLevelNote_SameOrMore_IsNull`;
    `MaxLevelNote_LevelAddedBack_ClearsAndRaisesChanged`.
  - `AddLevel_RowTypedAfterwards_RaisesProblemChanged` (the added row is watched).

## Verify by hand
None: the page comes in XP3.
- **Note from XP2 review:** `ExperienceFixtures`' `Classes` override does nothing and its stated reason is wrong
  (`ClassFixtures.World` holds only the scholar and the archer): drop it, or say it swaps in `SkilledArcher` for this
  slice's past-max test, and fix the XP2 Done note's matching sentence.
- **Done, with deviations:** the fixture's one class is now `ClassFixtures.SkilledArcher` (expose at level 4, the max
  level), which the past-max test needs, and XP2's Done note says the old override changed nothing. From the review:
  the note compares against `CatalogDraft.SavedMaxLevel` (set at load and in `MarkSaved`), passed by
  `ExperienceEditingViewModel.Start`, since an apply restarts the page while `rules.json` keeps the old max; tests
  `Start_LastLevelRemovedAndApplied_NoteNamesTheSavedMax`, `Start_LastLevelRemovedAppliedAndSaved_HasNoNote`,
  `SavedMaxLevel_LowerTableNotSaved_KeepsTheLoadedOne`, `SavedMaxLevel_LowerTableSaved_TakesItsMax`; fixture
  `TableOf`. Beyond the list: `AddLevel_FromOneRow_LightsRemoveLastLevel`, `RemoveLastLevel_TwoRows_IsLit` (the
  bound's lit side), `RemoveLastLevel_RaisesChanged` (`MaxLevel`, `Problem`),
  `RemoveLastLevel_RemovedRowTyped_RaisesNothing` (a removed row is no longer watched). About 270 lines (70 code, 200
  tests), past the 130 estimate. Sizes: Editor `ExperienceTableViewModel` 130, `CatalogDraft` 274, its `.Party`
  partial 108; tests `ExperienceTableViewModelTests` 264, `ExperienceEditingViewModelTests` 68,
  `CatalogDraftExperienceTests` 71, `ExperienceFixtures` 34.
