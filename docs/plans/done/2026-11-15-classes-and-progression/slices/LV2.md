# LV2. A member knows the class skills at or below its level, and learns on a level up

Core, about 220 lines (about 140 of them tests). Needs LV1. Design: section 5; decisions 963, 966, 970.

## Goal
`PartyMember.Skills` is the class's skills whose level is at or below the member's, in the class's order. A level up
adds the skills of each level reached; a member restored from a save at a higher level knows every skill up to it; a
recruit (level 1) knows the level-1 ones. A won battle's `ExperienceGain` names the skills each member learned with the
level each came at, for the victory screen (LV3). With every repository level at 1, play is unchanged; the tests set
levels of their own.

## New types
- `Core/Party/SkillsByLevel` (internal sealed): a class's skills resolved from the catalog with the level each is
  learned at. Built by `SkillsByLevel.Resolve(ClassDefinition, IReadOnlyDictionary<string, SkillDefinition>)` (the
  unknown-id throw moves here from `PartyMember.SkillsOf`); `IReadOnlyList<SkillDefinition> KnownAt(int level)`;
  `IReadOnlyList<SkillDefinition> LearnedAt(int level)` (exactly that level). It keeps `PartyMember.cs` (241 lines)
  near its size: the rule lives here, the member only asks.
- `Core/Battle/LearnedSkill` (public sealed record): one skill a member learned in a battle's experience, with the level
  it came at. `LearnedSkill(int Level, SkillDefinition Skill)`.

## Changes
- `Core/Party/PartyMember`: holds a `SkillsByLevel`; `Skills` becomes `{ get; private set; }`, set from `KnownAt(Level)`
  in the constructor, `LevelUp` and `RestoreProgress`; `public IReadOnlyList<SkillDefinition> SkillsLearnedAt(int
  level)`. `Recruit`'s summary: "the class's skills known at level 1, in its order (ADR 0020, ADR 0062)".
- `Core/Battle/ExperienceGain`: `public IReadOnlyList<LearnedSkill> LearnedSkills { get; init; } = [];` (an init
  property: its three test sites in `VictoryScreenTests` don't change).
- `Core/Battle/BattleRewards.GainOf`: after `GainExperience`, the levels the steps reached (`step.Level + 1` for each
  `IsLevelUp` step) give `LearnedSkills` through `member.SkillsLearnedAt`.
- `docs/map.md`: the Core `Party` row: `PartyMember` knows the skills up to its level (`SkillsByLevel`, ADR 0062); the
  `Battle` row: `ExperienceGain.LearnedSkills`, `LearnedSkill`.

## Tests
- `Core.Tests/Party/PartyMemberSkillLevelTests` (new; a class built in the test through `ClassBuilder.WithSkills` with
  levels 1, 3 and 3, and a skill catalog of its own):
  - `Recruit_LevelOne_KnowsOnlyLevelOneSkills`.
  - `GainExperience_ReachingLevelThree_KnowsBothNewSkills` (in the class's order).
  - `GainExperience_ReachingLevelTwo_KnowsNoMore` (no skill at 2: the list unchanged).
  - `RestoreProgress_LevelFive_KnowsEverySkillUpToIt`.
  - `RestoreProgress_LevelOne_KnowsOnlyLevelOne` (the other side: a restore never adds early).
  - `SkillsLearnedAt_LevelThree_NamesOnlyThatLevel`.
  - `Recruit_UnknownSkillId_Throws` (the moved check).
- `Core.Tests/Battle/BattleRewardsTests` (or its sibling past 250 lines, `BattleRewardsLearnedSkillTests`):
  - `GrantExperience_TwoLevelsGained_NamesTheSkillsOfBoth` (levels 2 and 3 reached: the level-3 skill with level 3).
  - `GrantExperience_NoLevelUp_NamesNone`.
  - `GrantExperience_FallenMember_NamesNone` (fallen members gain nothing).
- `Core.Tests/Party/PartyMemberTests`: every existing test passes (all its classes' skills at level 1).

## Verify by hand
1. In `content/classes.json` set Omer's (mage) `meteor` entry to `"level": 2` by hand. Run the game as Omer: the
   battle's Skills menu lacks Meteor; win battles until level 2; in the next battle Meteor is listed. The Status page
   shows it after the level up, not before.
2. Undo the hand edit; the game plays as before.

- **Done, with deviations:** the restore tests sit at the bound, `RestoreProgress_LevelThree_KnowsEverySkillUpToIt`
  and `RestoreProgress_LevelTwo_KnowsOnlyLevelOne`; the moved check's test is
  `Recruit_UnknownSkillIdAtALaterLevel_Throws` (`PartyMemberTests` already holds the level-1 case); one test beyond the
  list, `SkillsLearnedAt_LevelTwo_NamesNone`. `ClassBuilder.WithSkills` gains a `ClassSkill` overload; the rewards
  tests' class has skills at levels 1, 2 and 3 so both levels reached name one. The simulations' output identical;
  `formatVersion` stays 7. Sizes: `SkillsByLevel` 35, `LearnedSkill` 6, `PartyMember` 246, `BattleRewards` 55;
  `PartyMemberSkillLevelTests` 119, `BattleRewardsTests` 210.
