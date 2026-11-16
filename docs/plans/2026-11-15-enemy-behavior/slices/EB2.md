# EB2. `on` in `enemies.json`; which skills an enemy can use moves to Core

Core + Data, about 290 lines (about 145 of code; Core tests about 75, Data tests about 70 and 20 for the writer).
Needs EB1. Design: design.md, sections 2.5 (the shapes are one table), 2.7, 3.3 (the fit), 4.1 (the file and its
messages).

## Goal
`enemies.json` reads an action's `on`; Data refuses a target that can't aim the action's skill (or the attack) after
its trigger, naming the enemy, the rule's place, the skill and its aim; the writer leaves `on` out while null. The Core
rules the editor's boxes will read land here with their first user, Data: the trigger kinds' shapes, the fit, and which
skills an enemy can use.

## New types (Core, `Battle/`)
- `TacticSubject` (public enum): who a trigger names: `Itself`, `Allies`, `FallenAllies`, `Foes`.
- `TacticConditionNumber` (public enum): the number a trigger reads: `None`, `Percent`.
- `TacticConditionShape` (public sealed record `(TacticSubject Subject, TacticConditionNumber Number)`): what a trigger
  kind names as "that one" and which number it reads.
- `TacticConditionKinds` (public static): every trigger kind's shape, one table.
  - `public static TacticConditionShape ShapeOf(TacticConditionKind kind)` over a private dictionary; an unknown kind
    throws `ArgumentOutOfRangeException`. Rows: `OwnHpBelow` (Itself, Percent), `AllyHpBelow` (Allies, Percent),
    `AllyFallen` (FallenAllies, None).
- `TacticTargetFit` (public static): whether a rule's target can aim its skill, or the attack, after its trigger.
  - `public static bool Fits(EnemyAction action, SkillDefinition? skill)`: null `On` fits; the aim is
    `skill?.Effect.Targeting ?? AttackEffect.Targeting`; then design.md, section 3.3's table. `Subject` without a
    condition never fits.
- `EnemySkillRefusal` (public enum): why an enemy can't use a skill: `UsesClassResource`, `AimsAlongFormation`,
  `PlacesReadiedTurn`.
- `EnemySkillUse` (public static): whether an enemy can use a skill, and if not, why.
  - `public static EnemySkillRefusal? RefusalOf(SkillDefinition skill)`: today's three rules from Data's
    `FindUsabilityProblem`, with their comment (ADR 0019, ADR 0023).

## Changes
- `Data/Battle/EnemyTacticsValidator`:
  - `FindUsabilityProblem` maps `EnemySkillUse.RefusalOf(skill)` to today's three messages (unchanged words).
  - `FindConditionProblem` reads `TacticConditionKinds.ShapeOf(kind).Number`: `None` → "percent must be left out of
    allyFallen (was 30)." as today's message; `Percent` → `Within(...percent, 1, 99)`. The switch on `AllyFallen` goes.
  - `FindActionProblem` chains a new `FindTargetProblem(action, field, skills)` after the skill's own check: "tactics[1]
    (cleave).on: lowestHpFoe doesn't fit cleave, which aims at allEnemies." or "...on: subject needs a when that names
    someone." (both through `ContentJsonOptions.EnumName`). The skill label is the id or "attack".
- `Data/Content/AbsentWhenNullFilter`: `(typeof(EnemyAction), "on")` with "decision 848: most rules aim as their skill
  does".
- `docs/map.md`: Core `Battle` row (`TacticConditionKinds`, `TacticTargetFit`, `EnemySkillUse`); Data's validator line
  ("an action's target against its skill's aim, ADR 0057; which skills an enemy can use is Core's `EnemySkillUse`").

## Tests
- New `Core.Tests/Battle/TacticConditionKindsTests`: `ShapeOf_EveryKind_HasAShape` (every
  `Enum.GetValues<TacticConditionKind>()`: no throw; later kinds join this guard).
- New `Core.Tests/Battle/TacticTargetFitTests` (skills built in the test):
  - `Fits_TargetTriggerAndAim_MatchesTheTable` (Theory rows: `Self`/`self` yes, `Self`/`singleEnemy` no,
    `LowestHpFoe`/the attack yes, `LowestHpFoe`/`singleAlly` no, `HighestHpFoe`/`allEnemies` no,
    `LowestHpOnItsSide`/`singleAlly` yes, null/`allEnemies` yes, `Subject` after `AllyHpBelow`/`singleAlly` yes,
    `Subject` after `AllyHpBelow`/`singleEnemy` no, `Subject` after `AllyFallen`/`singleFallenAlly` yes, `Subject`
    after `OwnHpBelow`/`self` yes).
  - `Fits_SubjectWithoutCondition_IsFalse`.
- New `Core.Tests/Battle/EnemySkillUseTests`: `RefusalOf_Skill_MatchesTheRule` (Theory rows: a resource change →
  `UsesClassResource`; an aura in range → `AimsAlongFormation`; a Ready → `PlacesReadiedTurn`; a plain single-foe
  damage → null).
- New `Data.Tests/Battle/EnemyTacticRuleValidatorTests` (`EnemyTacticsValidatorTests` is at 233; enemies and skills
  built in the test as that file does):
  - `Load_ActionWithOn_ReadsTheTarget`
  - `Validate_OnThatDoesntFitTheSkill_NamesTheEnemyTheRuleAndTheAim` (asserts the whole message)
  - `Validate_OnThatFitsTheSkill_IsAccepted`
  - `Validate_SubjectWithoutWhen_IsRefused`
  - `Validate_SubjectInAPhase_NamesThePhaseField` ("phases[0].tactics[1] (attack).on: ...")
  - `Load_UnknownOn_NamesTheField` (a JSON `"on": "everyone"`)
- New `Data.Tests/Content/ContentWriterEnemyTacticsTests` (`ContentWriterEnemyTests` is at 152; this file fills through
  EB11): `WriteEnemies_ActionWithoutOn_LeavesItOut`, `WriteEnemies_ActionWithOn_WritesItAndReadsItBack`.
- Existing guards: `EnemyTacticsValidatorTests`' percent, `allyFallen` and the three usability tests, now through Core.

## Verify by hand
1. In `content/enemies.json`, add `"on": "subject"` to the Chief's `field_dressing` rule; run the game: it starts.
   Add `"on": "lowestHpFoe"` to `cleave`: the game stops at start naming "enemy 'goblin_chief': tactics[1] (cleave).on:
   lowestHpFoe doesn't fit cleave, which aims at allEnemies.". Revert the file.
- **Note from EB1:** the `(typeof(EnemyAction), "on")` row of `AbsentWhenNullFilter` landed in EB1 (the identical-bytes
  test needed it once `On` existed); EB2 keeps the rest of its Data changes.
