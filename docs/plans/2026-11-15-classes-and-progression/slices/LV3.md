# LV3. The victory screen says "Omer learned Meteor."

Game + content, about 200 lines (about 90 of them tests; the new view at the 1.6x a Game view costs). Needs LV2. Design:
sections 1, 5; decision 964.

## Goal
On the victory screen, under the members' cards, one line per skill learned: "Omer learned Meteor.", in party order and
then the skills' order. A line appears once its member's bar has filled into the skill's level (the same moment that
member's "Level up!" pops), so a two-level gain shows its second skill after the second fill; Confirm's skip shows them
all at once. No line, no space taken. The level-up sound already plays; the line adds none.

Look first: `docs/ui-reference/battle-victory.jpg` (the cards) and `menu-skill-learned-dialog.webp` (a framed "skill
learned" dialog with OK). This slice draws a line, not a dialog (decision 964's default; the dialog look is his
alternative): the cards' body face, pale text, centered under the cards, each line fading in over 0.2 s.

## New types
- `Game/Battle/LearnedSkillLine` (sealed record): one learned skill as the victory screen shows it: `Text` (formatted
  once), `MemberIndex`, `LevelsToFill` (how many of that member's level ups must have shown).
- `Game/Battle/VictoryLearnedSkillsView`: draws the shown lines under the cards, faded in by their time shown; texts
  measured once. Keeps `VictoryScreenView.cs` (277 lines) to its call: one field, one `Draw` line.

## Changes
- `Game/Battle/VictoryScreen` (131 lines, about 160 after): `IReadOnlyList<LearnedSkillLine> LearnedLines`, built once
  in the constructor from `Rewards.Gains[i].LearnedSkills` (the level minus the member's level before the gain gives
  `LevelsToFill`) with `UiMessages`; `bool IsShown(LearnedSkillLine line)`: the member's bar `LevelUps` at or past
  `LevelsToFill` (no allocation per frame). Its summary gains the lines.
- `Game/Battle/VictoryScreenView`: builds and draws `VictoryLearnedSkillsView` (+about 4 lines).
- `Game/Ui/MessageIds.Battle.cs`: `BattleSkillLearned`; `content/text/ui.json`: `"battle_skill_learned": "{0} learned
  {1}."`.
- `docs/map.md`, Game `Battle` row: `LearnedSkillLine`, `VictoryLearnedSkillsView`.

## Tests
- `Game.Tests/Battle/VictoryScreenLearnedSkillTests` (new; spoils built in the test with gains whose `LearnedSkills`
  name skills built there, as `VictoryScreenTests` builds gains):
  - `LearnedLines_GainWithTwoSkills_FormatsMemberAndSkill` (the text names the member and the skill).
  - `IsShown_BeforeTheBarFills_IsFalse`; `IsShown_AfterTheLevelFills_IsTrue` (stepped at 1/60 s).
  - `IsShown_SecondLevelSkill_WaitsForTheSecondFill` (a two-level gain).
  - `IsShown_AfterConfirmSkip_IsTrueForAll`.
  - `LearnedLines_NoSkillLearned_IsEmpty`.
- `Game.Tests/Ui/UiMessagesTests` (the repository's message ids check): passes with the new id.

## Verify by hand
1. Set Omer's `meteor` to `"level": 2` in `classes.json` by hand (or on the Classes page once CL6 lands). Run the game
   as Omer, win battles until level 2: "Omer learned Meteor." fades in under the cards as the level-up pops. F12
   captures it; compare with `battle-victory.jpg`'s spacing.
2. Win a battle with no level up: no line, the cards where they were.
3. Undo the hand edit.
