# BT1b. The table's class skill rows

Editor, about 300 lines (about 140 of them tests). Decisions 992, 993, 786. Design: design.md, sections 1
(`StartingLevels`, damage modifier) and 2.4. Split from BT1 at its start; builds on BT1's `Battle/ClassBattleStats`
and `Battle/EffectAmounts` and row 12's class skill entries (`ClassDefinition.Skills`, each a
`ClassSkill(SkillId) { Level }`, absent meaning 1, ADR 0062).

## Goal
The balance table's first rows, headless: every class's damaging and healing skills with what one hit and all its hits
come to at three typed levels, a typed damage modifier added to the modifiers' sum, worked out by the battle's own
rule (a member of the class at that level wearing nothing); a skill the class learns above a level reads "-" at that
level.

## New types
- `Editor/Balance/SkillBalanceRow` (internal sealed record): one skill of one class with its amounts at the table's
  levels: `ClassName`, `SkillName`, `TierLine` ("fire_1 x2", "flat 15"), `Hits`, `IsHeal`, `AmountTexts` (three:
  "24", or "28 x 4 = 112" with hits).
- `Editor/Balance/BalanceTableViewModel` (internal sealed partial, `ObservableObject`): the table's boxes and rows.
  - `BalanceTableViewModel(ContentCatalog content)`; `FirstLevelText`, `SecondLevelText`, `ThirdLevelText`
    (`StartingLevels` 1, 10, 20; whole numbers from 1 to `content.Rules.Experience.MaxLevel`),
    `ModifierPercentText` (0; a whole number above -100).
  - `ClassSkillRows` (`IReadOnlyList<SkillBalanceRow>`): each class in id order (the Classes page's, which its Apply
    writes back), its skills in `ClassDefinition.Skills`' order, only damage, restoreHp and revive, "-" at a level
    below its `ClassSkill.Level`; each amount `EffectAmounts.PerHit(skill.Effect, stats)` over
    `ClassBattleStats.At(class, level, content.Rules.Stats)`; rebuilt when a box changes; empty while `BoxesProblem`
    shows.
  - `BoxesProblem` ("Level 2 must be a whole number from 1 to 20."): the boxes' own, never named `Problem`, since BT3
    makes the view model an `IEditedCatalog` whose `Problem` is always null (see BT3's note from BT1).
  - The member's stats `with { DamageModifierPermille = stats.DamageModifierPermille + typed percent x 10 }`: added
    to the sum, not put in its place (row 9's gear bonus starts that sum, 0 with nothing worn), so Core's rule decides
    what the modifier raises (damage, not heals, decision 786).

## Changes
- `docs/map.md`: an Editor `Balance` line.

## Tests
- `Editor.Tests/Balance/BalanceTableViewModelTests` (new; classes and skills with `with` over the test world, the
  classes' skill entries built in the test, never the repository's):
  - `ClassSkillRows_TieredSkill_AmountsAtTheThreeLevels`.
  - `ClassSkillRows_FourHits_ShowsPerHitAndTotal`.
  - `ClassSkillRows_StatusOnlySkill_NotListed`.
  - `ClassSkillRows_SkillLearnedAtFive_DashAtLevelOne` (and its value at level 5, the bound's other side).
  - `ClassSkillRows_TwoClasses_InIdOrder` (set up opposite to id order).
  - `ModifierPercentText_MinusTwentyFive_LowersDamageNotHeals`.
  - `BoxesProblem_LevelPastTheMax_SaysSoAndEmptiesTheRows`; the accepted bounds, level 1 and the max, and the
    modifier's, -99, beside the refused 0, max + 1 and -100.

## Verify by hand
None: the page comes in BT3.
