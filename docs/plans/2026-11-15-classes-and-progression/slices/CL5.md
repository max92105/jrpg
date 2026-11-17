# CL5. The Levels tab: a class's numbers at every level, through Core's `ClassSheet`

Core + Editor, about 230 lines (about 110 of them tests, about 40 of XAML). Needs CL4 and row 7's BT1. Design: sections
1, 3; decision 948.

## Goal
A read-only Levels tab beside Stats: one row per level from 1 to the draft's max level, each with the experience to the
next level (empty at the max), max HP as a member of the class has it with nothing worn, speed, accuracy and the five
primaries. The numbers come from one Core reading, `Party/ClassSheet.At`, which row 7's `Battle/ClassBattleStats.At`
then maps, so the rule "a member of this class at this level wearing nothing" exists once (no fourth inline copy of
`MemberSheet.Of(ClassGrowth.StatsAt(...))`). The tab follows the boxes as typed; while a box can't be read, or a level's
number would pass the largest whole number (a huge growth), it shows that problem instead of rows.

## New types
- `Core/Party/ClassSheet` (public static): a member's sheet for a class at a level with nothing worn.
  `StatSheet At(ClassDefinition characterClass, int level, StatRules rules)`: `MemberSheet.Of(ClassGrowth.StatsAt
  (characterClass, level), StatBonuses.None, weaponDamage: null, rules)`; throws `ArgumentOutOfRangeException` below
  level 1 (`ClassGrowth`'s own throw).
- `Editor/Party/LevelPreviewRow` (sealed record): one level's numbers in the Levels tab. `Level`, `ExperienceToNext`
  (int?, null at the max level), `MaxHp`, `Speed`, `AccuracyPercent`, `Power`, `Agility`, `Intelligence`,
  `Constitution`, `Will`.
- `Editor/Party/ClassLevelPreview` (static): a class's rows at each level of the table, or why they can't be shown.
  - `static string? ProblemOf(ClassStats baseStats, ClassStats growth, int maxLevel)`: the first stat whose value at the
    max level passes `int.MaxValue`, worked in a `long` ("Growth power is too large: level 21 would pass
    2147483647."), so `ClassGrowth`'s `int` sums never wrap.
  - `static IReadOnlyList<LevelPreviewRow> RowsOf(ClassStats baseStats, ClassStats growth, ContentCatalog content)`:
    one row per level from 1 to `content.Rules.Experience.MaxLevel`, from `ClassSheet.At` over a class built from the
    two stats; throws on a `ProblemOf` input.

## Changes
- `Core/Battle/ClassBattleStats` (row 7's BT1): `At` maps `ClassSheet.At(...)` instead of building the sheet itself;
  its tests pass unchanged.
- `Editor/Party/ClassViewModel`: `IReadOnlyList<LevelPreviewRow> LevelRows` (empty while `Base.Typed` or
  `Growth.Typed` is null or `ClassLevelPreview.ProblemOf` names one) and `string? LevelsProblem` (that problem), raised
  with the boxes' changes. Its constructor takes the content (the table and the stat rules); `From`, `New` and
  `DuplicateOf` pass it, and `ClassCatalogViewModel` hands its own.
- `Editor/Party/ClassCatalogPage.xaml`: a Levels tab after Stats: a header row and an `ItemsControl` over
  `SelectedClass.LevelRows`, virtualized (its template holds the `ScrollViewer`; `VirtualizingStackPanel` as items panel,
  `ScrollViewer.CanContentScroll` true, `VirtualizingPanel.IsVirtualizing` true), each row a `Grid` with
  `SharedSizeGroup` columns Level, To next, Max HP, Speed, Accuracy, Power, Agility, Intelligence, Constitution, Will,
  right-aligned; the problem in its place while `LevelsProblem` is set. No `DataGrid`: the editor has none (grepped
  2026-11-15) and it would need a dark template of its own.
- `docs/map.md`: the Core `Party` row names `ClassSheet`; the Editor `Party` row the Levels tab.

## Tests
- `Core.Tests/Party/ClassSheetTests` (new; a class built in the test):
  - `At_LevelOne_IsTheBaseWithConstitutionsShare` (a Constitution and rate that change max HP: the sheet's number, not
    the class's raw one).
  - `At_LevelThree_AddsGrowthTwice` (power base 10, growth 3: 16).
  - `At_LevelZero_Throws`.
  - `At_SameClassAndLevel_EqualsARecruitedMembersSheet` (a member recruited and levelled in the test).
- `Editor.Tests/Party/ClassLevelPreviewTests` (new; stats built in the test, the test world's rules `with` a table of
  three entries built in the test):
  - `RowsOf_TableOfThree_GivesFourLevels`; `RowsOf_LastLevel_HasNoExperienceToNext`;
    `RowsOf_FirstLevel_ReadsTheFirstEntry` (an entry only it has).
  - `ProblemOf_GrowthIntMax_NamesTheStat` (growth power `int.MaxValue`: a problem, never negative rows);
    `ProblemOf_LargestThatFits_IsNull` (the bound's other side: base plus growth times levels equal to `int.MaxValue`).
- `Editor.Tests/Party/ClassViewModelTests` (CL2's file, about 160 lines, about 200 after):
  `LevelRows_TypedGrowth_FollowsIt`; `LevelRows_BoxNotANumber_IsEmptyWithProblem`;
  `LevelRows_TypedChange_RaisesLevelRowsChanged`.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
2. Database > Classes, `fighter`, Levels tab: 21 rows (the repository's table); level 1's max HP is above the class's
   base 220 (Constitution's share); each level's power is the one before plus 2. Scroll: smooth.
3. Stats tab, type `5` in Per level power, back to Levels: power rises by 5 per level. Type `2147483647`: the tab shows
   "Growth power is too large". Revert.
4. Run the game, start with Ragnir (fighter): the Status page's max HP at level 1 is the Levels tab's level 1 number.
- **Done, with deviations:** split on 2026-11-17, the whole slice estimated at about 400 lines with tests: CL5 built
  Core's `Party/ClassSheet` (14 lines) with `ClassSheetTests` (54, 4 tests), and `Party/ClassLevelPreview` (75) and
  `Party/LevelPreviewRow` (16) with `ClassLevelPreviewTests` (96, 12 tests); `ClassViewModel`'s `LevelRows` and
  `LevelsProblem`, the tab's XAML, their tests and the `ClassFixtures` draft moved to CL5b. Row 7's BT1 hasn't landed,
  so there is no `ClassBattleStats` to change: BT1's file gains a note to map `ClassSheet.At` from birth, and the
  game's `NewGame/LeaderProfile` (the review's ask) reads `ClassSheet.At` instead of its own copy of the rule. `RowsOf`
  takes the draft's `GameRules` (the table and the stat rules), not the whole catalog; the row holds a `PrimaryStats`
  rather than five numbers. `ProblemOf` refuses both bounds ("too large" past `int.MaxValue`, "too small" past
  `int.MinValue`: a typed negative growth wraps too), each side tested at the bound and one past it, plus the first
  stat in the page's order and a max level of 1. Core's formulas are unchanged, so no simulation was run. The sheet's
  own wrap past the check (Constitution's share of max HP, accuracy in permille) is a backlog line.
