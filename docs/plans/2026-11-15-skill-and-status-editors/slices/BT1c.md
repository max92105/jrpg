# BT1c. The table's level and modifier boxes

Editor, about 190 lines (about 120 of them tests). Decisions 992, 993. Design: design.md, section 1 (`StartingLevels`,
damage modifier). Split from BT1b at its start; builds on BT1b's `Balance/ClassSkillBalanceRows` and its
`Editor.Tests/Balance/BalanceFixtures`.

## Goal
The balance table's boxes, headless: three typed levels and a typed damage modifier in percent, their problem line,
and the class skill rows read through them, rebuilt on every typed change and empty while a box can't be read.

## New types
- `Editor/Balance/BalanceTableViewModel` (internal sealed partial, `ObservableObject`): the table's boxes and rows.
  - `BalanceTableViewModel(ContentCatalog content)`; `FirstLevelText`, `SecondLevelText`, `ThirdLevelText`
    (`StartingLevels` 1, 10, 20; whole numbers from 1 to `content.Rules.Experience.MaxLevel`),
    `ModifierPercentText` (0; a whole number above -100, and below a bound of the slice's own so the percent x 10
    fits an int beside a member's gear bonus: a decision number from "Next free numbers").
  - `ClassSkillRows`: `Balance/ClassSkillBalanceRows.Of(content, levels, percent x 10)`; empty while `BoxesProblem`
    shows; each box's change raises it and `BoxesProblem` (a test per box).
  - `BoxesProblem` ("Level 2 must be a whole number from 1 to 20."): the boxes' own, never named `Problem`, since BT3
    makes the view model an `IEditedCatalog` whose `Problem` is always null (see BT3's note from BT1).
  - Kept under 300 lines with BT2's rows coming: the rows' building stays in `ClassSkillBalanceRows` and BT2's own row
    builders; the view model only reads the boxes and hands them on.

## Changes
- `docs/map.md`: the Editor `Balance` line names the view model.

## Tests
- `Editor.Tests/Balance/BalanceTableViewModelTests` (new; over `BalanceFixtures.World` with an experience table of a
  known max level):
  - `New_Boxes_StartAtOneTenTwentyAndZero`.
  - `ClassSkillRows_LevelsTyped_ReadAtThoseLevels`; `ClassSkillRows_ModifierTyped_PassedAsPermille`.
  - `BoxesProblem_LevelPastTheMax_SaysSoAndEmptiesTheRows`; the accepted bounds, level 1 and the max, and the
    modifier's, -99 and its max, beside the refused 0, max + 1, -100, max + 1 and a word.
  - `TypedBox_Changed_RaisesRowsAndProblem` (a theory over the four boxes).

## Verify by hand
None: the page comes in BT3.
