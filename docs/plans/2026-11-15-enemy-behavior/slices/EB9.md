# EB9. Reactions: it was hit, critically hit, hit on a weakness

Core + Data + Editor, about 300 lines (Core about 90 and 16 tests at about 12 lines; Data 2 tests; Editor 3 lines of
words). Needs EB8. Design: design.md, sections 2.5 (the automatic heal, reactions remembered, the taunt), 3.1;
decisions 835, 838, 844-846 (the timing is the first default to confirm).

## Goal
The developer's "when critically hit - skill X - lowest HP enemy": three reactions remembered from the moment a foe's
action lands them until the end of the next turn the enemy acts. "That one" is the foes who did it. With priority
(EB8), the answer is certain. The memory every later option reads is born here, written at one place, the end of
`BattleState.Execute`. The automatic heal stops narrowing under a trigger that doesn't name allies, so the first
foe-naming trigger can't empty it.

## New types (Core, `Battle/`)
- `TacticMemory` (internal sealed class): what an enemy's rules remember of this battle so far.
  - `public void NoteReaction(TacticConditionKind kind, Combatant subject)` (a subject noted once per kind);
    `public IReadOnlyList<Combatant> Noted(TacticConditionKind kind)` (in the order noted, fallen ones kept);
    `public void ForgetReactions()`.
- `TacticMemoryNotes` (internal static): writes one `Execute`'s events into the enemies' memories.
  - `public static void Note(IReadOnlyList<BattleEvent> events)`: the call's `ActionStarted` (always its first event)
    names the actor. First, an enemy actor's memory forgets its reactions (its turn answered them, decision 844). Then
    each `EffectApplied` of kind damage whose target is an enemy and whose actor is on the other side notes `WasHit`,
    plus `WasCriticallyHit` when `IsCritical`, plus `WasHitOnWeakness` when `Affinity` is `Weak`, each with the actor
    as subject (a private `NoteHit(hit, actor)`). A `HitMissed` notes nothing; a `StatusTicked` isn't an
    `EffectApplied`; a blocked hit still reports its `EffectApplied` and counts (decision 845). Its summary says it is
    the one place the memory is written, after every event of the call (ADR 0034's rule for phases).

## Changes
- Core `Battle/TacticConditionKind`: `WasHit`, `WasCriticallyHit`, `WasHitOnWeakness`, each summary saying "since its
  last turn, by a foe's action".
- Core `Battle/TacticConditionShape`: `bool IsReaction` (a third positional, false by default); rows for the three
  (Foes, None, reaction).
- Core `Battle/Combatant`: `internal TacticMemory TacticMemory { get; } = new();` ("read by enemy tactics only; fresh
  each battle, as every combatant is").
- Core `Battle/BattleState.Execute`: `TacticMemoryNotes.Note(events);` beside `EnemyPhases.EnterReached` in the existing
  `if (!IsOver)` block (283 → 284 lines); the method summary names it.
- Core `Battle/TacticConditions.CombatantsMeeting`: one arm, `_ when TacticConditionKinds.ShapeOf(condition.Kind)
  .IsReaction => enemy.TacticMemory.Noted(condition.Kind)`, before the default (every later reaction kind takes this
  arm).
- Core `Battle/EnemyTargeting.HealCandidates` (decision 835): intersect the legal allies with `CombatantsMeeting` only
  when the kind is `OwnHpBelow` or `TacticConditionKinds.ShapeOf(kind).Subject == TacticSubject.Allies`; otherwise the
  legal allies. Its comment keeps decision 401's reason and adds why: a trigger naming foes or the fallen would leave
  nobody, and `LowestShareOf`'s `Aggregate` would throw.
- Data: nothing in code (shape-driven).
- Editor `TacticConditionChoice.All`: "it was hit", "it was critically hit", "it was hit on a weakness".
- `docs/map.md`, Core `Battle` row: `TacticMemory`, `TacticMemoryNotes`, the reactions.

## Tests
- New `Core.Tests/Battle/EnemyReactionTests` (battles run through `Execute` with `FixedSequenceRandom`; members built in
  the test with crit chance 100 or 0 as each test needs; enemies built with `EnemyDefinitionBuilder`):
  - `ChooseCommand_WasHitSinceItsLastTurn_IsEligible`
  - `ChooseCommand_WasHitBeforeItsLastTurn_IsForgotten` (hit, then it acts, then nothing: not eligible)
  - `ChooseCommand_HitMissed_IsNotAHit`
  - `ChooseCommand_HitOnAnotherEnemy_IsNotEligible` (two enemies, only the other one hit)
  - `ChooseCommand_AllEnemiesSkill_NotesEveryTarget` (a member's all-foes skill: both enemies' rules are eligible)
  - `ChooseCommand_WasCriticallyHitByACrit_IsEligible`
  - `ChooseCommand_WasCriticallyHitByAPlainHit_IsNotEligible`
  - `ChooseCommand_WasHitOnWeakness_EligibleOnlyForAWeakHit` (a weakness of -25 to the hit's type)
  - `ChooseCommand_StunnedThroughItsTurn_KeepsTheReaction` (hit, stunned past its turn, then chooses: eligible)
  - `ChooseCommand_SubjectOfWasHit_AimsAtTheHitter` (two members, only the second hit it)
  - `ChooseCommand_SubjectOfWasHitWhoFell_RuleNotEligible` (the hitter fell since: `subject` finds no legal one)
  - `ChooseCommand_SubjectOfWasHitWhileAnotherMemberTaunts_AimsAtTheTaunter` (decision 838: the hitter doesn't taunt,
    another member does, the shrug-off roll fails)
  - `ChooseCommand_AutomaticSingleAllyHealOnWasHit_HealsTheLowestShare`,
    `ChooseCommand_AutomaticSingleAllyHealOnWasCriticallyHit_HealsTheLowestShare`,
    `ChooseCommand_AutomaticSingleAllyHealOnWasHitOnWeakness_HealsTheLowestShare` (no `on`; two hurt allies; before
    decision 835 each threw)
- New `Core.Tests/Battle/TacticMemoryNotesTests`: `Note_BlockedHit_CountsAsAHit` (the events written in the test,
  `ActionStarted`, `HitBlocked`, `EffectApplied`, since no enemy can block through `Execute` today: none has a block
  chance).
- `Data.Tests/Battle/EnemyTacticConditionValidatorTests` (new; filled through EB11):
  `Validate_WasCriticallyHitWithAPercent_IsRefused`, `Load_WasCriticallyHitOnLowestHpFoe_ReadsTheRule`.
- `Editor.Tests/Battle/TacticChoiceTests`' every-kind guard covers the words; EB1's replay test runs unchanged (no
  replayed rule reacts; `allyHpBelow`'s heal still narrows).

## Verify by hand
1. Editor: Database > Enemies, `goblin`, Tactics: Add "When it was hit: cleave on automatic", Priority, apply, Save all.
   In the forest: a goblin hit since its last turn cleaves; one nobody hit attacks. Then make it "When it was
   critically hit: attack on that one" and play until a member crits (the big number): that goblin's next turn hits
   the member who crit it. Ctrl+Z twice, Save all; the diff is empty.
