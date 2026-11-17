# CL5b. The Levels tab on the page: the chosen class's rows as typed

Editor, about 160 lines (about 60 of them tests, about 40 of XAML). Split from CL5 on 2026-11-17; needs CL5's
`Party/ClassLevelPreview`. Design: sections 1, 3; decision 948.

## Goal
The Classes page gains CL5's read-only Levels tab beside Stats: one row per level from 1 to the draft's max level, each
with the experience to the next level (empty at the max), max HP as a member of the class has it with nothing worn,
speed, accuracy and the five primaries. The tab follows the boxes as typed; while a box can't be read, or a level's
number would pass a whole number's bounds, it shows that problem instead of rows.

## Changes
- `Editor/Party/ClassViewModel`: `IReadOnlyList<LevelPreviewRow> LevelRows` (empty while `Base.Typed` or
  `Growth.Typed` is null or `ClassLevelPreview.ProblemOf` names one) and `string? LevelsProblem` (that problem, or the
  box's), raised with the boxes' changes. Its constructor takes the content (the table and the stat rules, CL6's skills
  later); `From`, `New` and `DuplicateOf` pass it, and `ClassCatalogViewModel` hands its own.
- `Editor/Party/ClassCatalogPage.xaml`: a Levels tab after Stats: a header row and an `ItemsControl` over
  `LevelRows`, virtualized (its template holds the `ScrollViewer`; `VirtualizingStackPanel` as items panel,
  `ScrollViewer.CanContentScroll` true, `VirtualizingPanel.IsVirtualizing` true), each row a `Grid` with
  `SharedSizeGroup` columns Level, To next, Max HP, Speed, Accuracy, Power, Agility, Intelligence, Constitution, Will,
  right-aligned (`Primaries.Power` and so on: the row holds a `PrimaryStats`); the problem in its place while
  `LevelsProblem` is set. No `DataGrid`: the editor has none and it would need a dark template of its own.
- `tests/Jrpg.Editor.Tests/Party/ClassFixtures`: the test world with Archer and Scholar as its classes (CL3b review's
  note), used by `ClassViewModelTests` for the content its constructor now takes and by `CatalogDraftClassTests`,
  `ClassEditingViewModelTests` and `ClassCatalogViewModelTests` in place of their own copies.
- `docs/map.md`: the Editor `Party` row names the Levels tab.

## Tests
- `Editor.Tests/Party/ClassViewModelTests` (237 lines on 2026-11-17; a sibling file if it would pass 250):
  `LevelRows_TypedGrowth_FollowsIt`; `LevelRows_BoxNotANumber_IsEmptyWithProblem`;
  `LevelRows_GrowthPastTheBound_IsEmptyWithProblem`; `LevelRows_TypedChange_RaisesLevelRowsChanged`.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: "Database>Classes: shown as a page", no `ERROR`, no binding-error
   line.
2. Database > Classes, `fighter`, Levels tab: 21 rows (the repository's table); level 1's max HP is above the class's
   base 220 (Constitution's share); each level's power is the one before plus 2. Scroll: smooth.
3. Stats tab, type `5` in Growth power, back to Levels: power rises by 5 per level. Type `2147483647`: the tab shows
   "Growth power is too large". Revert.
4. Run the game, start with Ragnir (fighter): the Status page's max HP at level 1 is the Levels tab's level 1 number.
