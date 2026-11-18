# EB1. Today's choices recorded; a rule names its target

Core, about 270 lines (about 110 of code, 12 tests at about 13 lines), after a step 0 of about 70 lines (a test only).
Needs ADR 0057 and row 6's SW (decision 857). Design: design.md, sections 2.2, 2.5 (eligibility never draws, the
taunt), 3.2 (the targets).

## Goal
An enemy's action may say whom it aims at: itself, the foe with the lowest or highest HP share, the lowest HP share on
its side (itself included), or "that one", whom its condition names. A rule whose target finds no legal target now
isn't eligible. A rule without a target aims exactly as today, with today's draws, and a recorded test proves it.

## Step 0, before touching `EnemyTactics` (its own commit if the slice is reviewed in two parts)
New `Core.Tests/Battle/EnemyTacticsReplayTests` (no file of that name exists): two fights built in the test, run to
their end or 60 turns with a party policy written in the test (each member attacks the first living enemy), each
`Execute`'s `ActionStarted` written as "actor: command(target, ...)", for seeds 1, 2 and 3:
- a Chief-like fight: a boss built with `EnemyDefinitionBuilder` with a start list of the attack, an all-foes skill and
  an `allyHpBelow 50` single-ally heal, a phase below 50 % with a single-foe stun skill and an `allyFallen` all-allies
  guard; two plain enemies beside it; three members built as `EnemyTacticConditionTests` builds them, one taunting
  through a self-granted taunt skill so the taunt roll is exercised;
- a forest-like fight: three plain enemies without tactics against two members.
- `ChooseCommand_ChiefLikeFightSeed1_ChoosesAsRecorded`, `..Seed2..`, `..Seed3..`;
  `ChooseCommand_ForestLikeFightSeed1_ChoosesAsRecorded`, `..Seed2..`, `..Seed3..` (or one Theory per fight with the
  seed and its recorded lines as data).
The recorded lines are what today's code produces, pasted from a first run; the test stays asserted for the whole
plan (decision 860). Skills and statuses are built in the test (Stun and Taunt as SW's records, `Builders/TestStatuses`).

## New types (Core, `Battle/`)
- `TacticTarget` (public enum): the closed list of whom a rule aims at; null on an action is automatic. Members, each
  with a one-line summary: `Subject`, `Self`, `LowestHpFoe`, `HighestHpFoe`, `LowestHpOnItsSide` ("the lowest HP share
  on its side, itself included").
- `TacticTargetCandidates` (internal static): whom a rule's target may aim at among the skill's legal targets now, with
  no draw.
  - `public static IReadOnlyList<Combatant> For(EnemyAction action, IReadOnlyList<Combatant> legal, Combatant enemy,
    BattleState battle)`: `Self` → the enemy if legal; `LowestHpFoe`, `HighestHpFoe` → the legal foes;
    `LowestHpOnItsSide` → the legal living allies (itself included); `Subject` → `TacticConditions.CombatantsMeeting`
    ∩ legal, or none without a condition. The default arm throws `ArgumentOutOfRangeException`.

## Changes
- `Battle/EnemyAction`: `public TacticTarget? On { get; init; }` ("whom it aims at, decision 834; null aims as the
  skill or the attack always did, decisions 400, 401").
- `Battle/EnemyTargeting`: `public Combatant ForRule(Combatant enemy, TacticTarget target, IReadOnlyList<Combatant>
  candidates, IReadOnlyList<Combatant> legal)` (candidates non-empty). For a foe-aimed target (`LowestHpFoe`,
  `HighestHpFoe`, `Subject` naming foes), the taunt narrows first (decision 838): when any of `legal`'s foes taunts
  (SW's `Statuses.FirstWith(StatusPartKind.DrawAttacks)`) and the enemy doesn't shrug it off (today's `ResistsTaunt`,
  rolled only then), the taunting legal foes replace the candidates. Then `Self` → the one; `LowestHpFoe`,
  `LowestHpOnItsSide`, `Subject` → `LowestShareOf` (today's, no draw); `HighestHpFoe` → a new `HighestShareOf`
  (cross-multiplied, a tie to the first). Four arms grouped by how they pick; the narrowing is a private
  `TauntingOrAll(enemy, legal, candidates)` that today's `ByTauntRule` also calls before its draw (its draws
  unchanged). Class summary: one sentence for the rule's pick.
- `Battle/EnemyTactics`:
  - `IsEligible` becomes three named predicates joined: `ConditionHolds`, `SkillUsable`, `HasTarget` (a rule with
    `On` needs `TacticTargetCandidates.For(...)` non-empty; legal = `battle.TargetChoiceFor(skill).Candidates`, or
    `battle.TargetChoiceForAttack().Candidates` for the attack).
  - `CommandFor`: with `On`, `SkillCommand(skill, [ForRule(...)])` or `AttackCommand(ForRule(...))`; without, today's
    lines.
  - Summary: "a rule with a target aims by it (decision 834) and is eligible only when it finds someone (decision 836);
    deciding eligibility never draws".
- `docs/map.md`, Core `Battle` row: `TacticTarget`, `TacticTargetCandidates`, `EnemyTargeting.ForRule`.

## Tests
New `Core.Tests/Battle/EnemyTacticTargetTests` (no file of that name exists; enemies built with
`EnemyDefinitionBuilder`, members and skills built in the test, draws through `FixedSequenceRandom`; the rule is the
enemy's only action, so a wrong extra draw runs the sequence out):
- `ChooseCommand_SelfOnASingleAllyHeal_AimsAtItself`
- `ChooseCommand_LowestHpFoe_AimsAtTheLowestShare` (members at 30/100 HP and 20/40 HP, listed in that order: 30 %
  wins over 50 % although 20 is the lower number)
- `ChooseCommand_LowestHpFoeTie_AimsAtTheFirstInBattleOrder` (the tied pair listed in the opposite order of a sort)
- `ChooseCommand_HighestHpFoe_AimsAtTheHighestShare`
- `ChooseCommand_LowestHpFoeWhileAMemberTaunts_AimsAtTheTaunter` (the taunter's share is the highest; the roll fails
  the shrug-off)
- `ChooseCommand_LowestHpFoeTauntShruggedOff_AimsAtTheLowestShare` (crowd control resistance 50, the roll succeeds)
- `ChooseCommand_LowestHpOnItsSide_AimsAtItselfWhenLowest`
- `ChooseCommand_SubjectOfAllyHpBelow_AimsAtThatAlly` (one ally at 40 % meets 50 %, one at 60 % doesn't, the enemy
  itself at 10 %: the 40 % one)
- `ChooseCommand_SubjectOfAllyFallen_AimsAtTheFallenAlly` (a `SingleFallenAlly` skill)
- `ChooseCommand_SubjectWithoutCondition_AttacksInstead` (not eligible: the attack and its one draw)
- `ChooseCommand_TargetFindsNoLegalOne_AttacksInstead` (`Subject` of `AllyFallen` on a `SingleAlly` heal: the fallen
  aren't legal)
- `ChooseCommand_AttackOnLowestHpFoe_AttacksThatFoe`

Existing guards, unchanged: step 0's replay, `EnemyTacticsTests`, `EnemyTacticConditionTests`, `EnemyHealTargetTests`,
`EnemySkillTacticsTests`, `EnemyPhaseTests`.

## Verify by hand
None: the replay test is the check that nothing moved (decision 860).

- **Done, with deviations:** `AbsentWhenNullFilter` gets EB2's `(EnemyAction, "on")` line now, as the writer's
  identical-bytes test failed on `"on": null`; the predicate is `CanUseSkill`, not `SkillUsable`; a foe-aimed pick is
  one whose candidates are foes; tests add `HighestHpFoeTie` and a Theory over each candidate filter left with no one.
