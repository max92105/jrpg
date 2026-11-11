# GR1. The Options page's pips drawn apart; its tests reach rows by name; the backlog closed

Game, Game tests and docs, about 60 lines; a draw extraction (hand check) and a test arrangement change, no behavior change. Builds on decisions 539 and 557 and design.md, "Backlog survey". The plan's last slice.

## Goal
The Options page, which the settings-kept-between-runs plan opens first (decision 415), reads at one level: `DrawRow` places a row and hands the volume rows' pips to their own method; its older tests reach the text speed row by what it is. The backlog holds no Game line this plan covered.

## Changes
- `src/Jrpg.Game/Menus/OptionsPage.cs` (236 → about 240): `DrawRow` (25 lines since the volume pips) keeps the label and the value; the volume's pips move to `private float DrawVolumePips(SpriteBatch batch, TextureLibrary textures, Point at, PipRowLook look)`, which draws them at `at` (worked out in `DrawRow` from the value's left and the row's middle, as today) and returns the width `DrawRow` adds to `numberLeft` (`PipRow.WidthOf(VolumeLevel.Max) + PipsToNumberGap`). `DrawRow` builds the `PipRowLook` as today and draws the number after. Nothing allocated per frame, as before.
- `tests/Jrpg.Game.Tests/Menus/OptionsPageTests.cs` (220): the four text speed tests (`Update_RightOnTextSpeed_GoesToFaster`, `Update_RightOnTextSpeed_LastValue_StaysOnLast`, `Update_LeftOnTextSpeed_GoesToSlower`, `Update_LeftOnTextSpeed_FirstValue_StaysOnFirst`) start from `PageOn(OptionsPage.TextSpeedRow)` instead of one Down press (the tests rule: reach a row by what it is). Their act and assert are unchanged.
- `docs/plans/backlog.md`, the closing pass:
  - delete the music plan's `OptionsPageTests` and `OptionsPage.DrawRow` lines;
  - the two `ScreenFade` lines become one (decision 557): the skip is unreachable under `JrpgGame`'s fixed 1/60 s step, since MonoGame's `Game.Tick` (develop branch) passes `ElapsedGameTime = TargetElapsedTime` to every catch-up `DoUpdate`, its backlog capped at 500 ms; reopen if the step is ever made variable;
  - delete the Status Skills section line once hand check 3 confirms the stats plan placed it (design.md, "Already done");
  - `VictoryScreenView`'s count becomes 277; the battle plan's banner band line is rewritten (design.md, "Stays");
  - reread each "Stays" group of design.md against the file it names (a count that moved is corrected).

## Tests
None new. The runner's totals per project equal before and after (Done note); the four changed tests are named there.

## Verify by hand
1. Menu, Options: every row reads as before; the three volume rows show their pips and number in the same place; Left and Right on a volume move the pips and the number together.
2. Text speed: Left and Right step through the speeds as before.
3. Menu, Status, on the member with the most gear: the Skills section and the resources sit side by side and nothing runs past the page (the backlog's F1 line).
