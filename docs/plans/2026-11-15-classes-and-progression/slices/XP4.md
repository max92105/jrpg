# XP4. Fill from a curve: a first amount and a growth percent

Editor, about 190 lines (about 100 of them tests, about 25 of XAML). Needs XP3. Design: sections 1, 3; decisions 957,
958.

## Goal
Under the table, a Fill from curve row: First (the experience from level 1 to 2), Growth (%), Max level, and Fill. Fill
replaces the rows with the first amount, then each the one before times (100 + growth) / 100, rounded down, up to the
max level. It is greyed, with its reason on the fill's own line, while a box can't be read, the growth is -100 or below,
the max level is outside 2 to 99 (decision 958's default), or a row would pass the largest whole number or fall below
1. The boxes open on the table's first amount, 20 % and the table's max level (design section 1). The rows stay
editable after a fill; Revert throws the whole session away. The game is unchanged: it reads the list (today's table
is first 100, 20 %, max level 21).

## New types
- `Progression/ExperienceCurve` (static): the fill's rule, a first amount and a growth percent into a table's rows, or
  why it can't.
  - `const int MostLevels = 99`, `const int FewestLevels = 2`.
  - `static IReadOnlyList<int> Rows(int first, int growthPercent, int maxLevel)`: `maxLevel - 1` rows, each in a `long`
    product divided last (`previous * (100 + growth) / 100`), rounded down; throws `ArgumentOutOfRangeException` on
    inputs `ProblemOf` refuses.
  - `static string? ProblemOf(int first, int growthPercent, int maxLevel)`: "Max level must be 2 to 99", "Growth must
    be above -100 %", "First must be at least 1", "Level 87 to 88 would need more than 2147483647", "Level 9 to 10 would
    need less than 1" (the first row that breaks, so he sees where).
- `Progression/ExperienceCurveBoxes` (`ObservableObject`): the fill's three typed boxes and the rows they make.
  `FirstText`, `GrowthText`, `MaxLevelText`; `Problem` (`TypedNumber` per box, then `ExperienceCurve.ProblemOf`);
  `IReadOnlyList<int> ToRows()`.

## Changes
- `Progression/ExperienceTableViewModel`: `ExperienceCurveBoxes Curve` (built from the table); `[RelayCommand(CanExecute
  = ...)] Fill` greyed while `Curve.Problem` is set (the same problem the line shows, `.claude/rules/editor.md`),
  replacing `Rows` and recomputing the totals and the max level note.
- `Progression/ExperienceTablePage.xaml`: the fill row and its problem line under the table.
- `docs/map.md`, Editor `Progression` row: the fill (`ExperienceCurve`).

## Tests
- `Progression/ExperienceCurveTests` (new):
  - `Rows_HundredAtTwentyPercent_GrowsAndRoundsDown` (100, 20, max level 6: 100, 120, 144, 172, 206).
  - `Rows_ZeroGrowth_RepeatsTheFirst`; `Rows_NegativeGrowth_Shrinks` (100, -10, 4: 100, 90, 81).
  - `Rows_MaxLevelTwo_GivesOneRow` and `ProblemOf_MaxLevelOne_Refuses` (the lower bound's two sides);
    `ProblemOf_MaxLevel99_IsNull` and `ProblemOf_MaxLevel100_Refuses` (the upper bound's).
  - `ProblemOf_GrowthMinus100_Refuses` and `ProblemOf_GrowthMinus99_IsNull`.
  - `ProblemOf_RowPastIntMax_NamesTheLevelStep` (first `int.MaxValue / 2`, 300 %: "Level 2 to 3").
  - `ProblemOf_RowBelowOne_NamesTheLevelStep` (first 2, -60 %).
  - `ProblemOf_FirstIntMaxGrowthZero_IsNull` (no overflow in the product: it is a `long`).
- `Progression/ExperienceTableViewModelTests` (XP2's file, about 160 lines; the fill's tests go to the sibling
  `Progression/ExperienceTableFillTests`, new):
  - `Fill_ValidCurve_ReplacesTheRows` (from a three-row table to five rows of the curve).
  - `Fill_WithProblem_IsGreyed` (and the rows unchanged).
  - `Fill_RecomputesTotalsAndNote`.
  - `Curve_OpensOnTheTablesFirstAndMaxLevel`.
  - `Fill_CurveBoxTyped_RaisesCanExecuteChanged`.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
2. Database > Experience: the fill row shows 100, 20, 21. Fill: the twenty rows equal the repository's table (the curve
   reproduces it). Apply, Save all: `git diff content/rules.json` is empty.
3. Type growth `15`, max level `40`, Fill: 39 rows, max level 40. Type max level `100`: Fill greys and the line says
   "Max level must be 2 to 99". Type first `2000000000`, max level 10: the line names the first level step past the
   largest number. Revert: the table is as before.
