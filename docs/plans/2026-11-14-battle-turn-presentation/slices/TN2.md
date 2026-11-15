# TN2. An action lands at its impact, after a 0.7 s wind-up

Game, about 230 lines (about 140 of them tests). Needs TN1 and the ADR 0013 amendment. Decisions 644, 645; design "Key
decisions" (Execute at the impact; the wind-up decides from the command); tunables "Wind-up", "Impact hold".

## Goal
Between the command and Core's `Execute` the turn now holds a wind-up: 0.7 s for an Attack, a skill or an item, nothing
for Defend, Move or Run. Core executes when it ends, which is the impact: the target's HP, the hit's number and sound,
statuses and falls all show then, through the existing stage-change refresh. The impact hold drops to 0.85 s.

Interim until TN3 and TN4: the banner and a skill's own sound still come when the playback starts, at the impact.

## New types
- `Game/Battle/ActionWindUp` (internal sealed): what one action shows before it lands, at the time reached. This slice:
  `static ActionWindUp Of(Combatant actor, BattleCommand command)` (throws `InvalidOperationException` on a command it
  doesn't know, as `ActionPlayback` does); `Combatant Actor`; `TimeSpan Duration` (`BattlePacing.WindUp` for
  `AttackCommand`, `SkillCommand`, `ItemCommand`; zero for `DefendCommand`, `MoveCommand`, `FleeCommand`); `TimeSpan
  Elapsed`; `void Update(TimeSpan delta)`. Later slices add its name (TN3), sounds (TN4, EF6), blinks (FL1), aimed strip
  (EF4) and swing (EF5).

## Changes
- `Game/Battle/TurnStage`: `WindingUp` between AwaitingCommand and Holding ("The action is under way; Core executes it
  when this ends, at the impact").
- `Game/Battle/BattleTurnSequence` (154 → about 185): `ActionWindUp? WindUp` (null until someone acts; kept, like
  `Playback`, until the next action's); `Submit` and the enemy's StepForward end start a wind-up from the command and
  keep the command; `DurationOf(WindingUp)` is the wind-up's; `EndStage(WindingUp)` calls `Act`; `Update` advances
  `WindUp` beside `Playback`; `StepShare` is 1 for a party actor while winding up. Its summary names the wind-up and the
  amended ADR 0013.
- `Game/Battle/BattlePacing`: `WindUp` 0.7 s ("from the action's start to its impact, the clip's"); `ImpactHold` 1.0 →
  0.85 s, its summary giving the clip's reason.
- `BattleScene`: nothing (the stage change to Holding already refreshes the panels at the impact); check `ShowStage`'s
  comment "the vitals once an action lands" still reads true.

## Tests
Wind-ups are built in each test class through one helper (`WindUpOf(command)`), so TN3's and EF5's added parameters
change one line per class.
- `ActionWindUpTests` (new, `tests/Jrpg.Game.Tests/Battle/`), combatants from
  `BattleScenarios.SarxianAgainstGoblinTrio`:
  - `Of_Attack_LastsTheWindUp`, `Of_Skill_LastsTheWindUp` (a skill from `BattleScenarios.Skills`),
    `Of_Item_LastsTheWindUp` (an item from `BattleScenarios.Catalog`).
  - `Of_Defend_LastsNothing`, `Of_Move_LastsNothing`, `Of_Flee_LastsNothing`.
  - `Update_TwoDeltas_AddsThem`.
- `BattleTurnSequenceWindUpTests` (new sibling), battles built as `BattleTurnSequenceTests` builds them; a sure hit
  through `BattleScenarios.WithOneGoblin(..., goblin => goblin with { Dodge = 0 })`:
  - `Submit_Attack_WindsUpWithoutExecuting` (stage `WindingUp`, `Playback` null, the goblin's HP unchanged).
  - `Update_WindUpEnds_ExecutesAtTheImpact` (one tick short: HP unchanged; the next tick: Holding, HP lower), stepped at
    `StartMapScenarios.OneUpdate`.
  - `Submit_Defend_ExecutesAtOnce` (Holding passed within the update, as today).
  - `Update_EnemyPauseEnds_WindsUpItsChoice` (an enemy-first battle: StepForward, then `WindingUp`, then Holding).
  - `StepShare_PartyActorWindingUp_IsOne`.
  - `Update_QuickSkill_WindsUpTheNextCommandToo` (after `ready`, the next `Submit` winds up again).
- `BattleTurnSequenceTests` re-pins (each adds the wind-up's 0.7 s or reads `BattlePacing.WindUp`):
  `Submit_HoldsThenStepsBack`, `Update_TargetFell_HoldsLonger`, `Update_Holding_ActorIsStillTheOneWhoActed`,
  `Update_LongDelta_CarriesIntoNextStage` (TN1's version), `Update_EnemyStunnedByBash_NextStageIsTheFollowingActor`,
  `Update_AfterQuickSkill_StaysForwardAndAwaitsCommand` (`Playback` is null right after `Submit`: update through the
  wind-up first). `Update_EnemyActsFirst_RunsUntilPartyTurn` and `Update_AfterReady_...` check their long constant still
  covers every enemy turn.
- `ActionPlaybackTests.Start_AttackLanded_HasOneImpactBeat` pins `TimeSpan.FromSeconds(1)`: re-pinned to
  `BattlePacing.ImpactHold`.
- `BattleSceneTests` and `BattleSceneSoundTests`: run; a test whose updates end before the impact gets its loop
  lengthened by the wind-up, named in the deviation note (`Update_AttackPlaysOut_AsksItsImpactSound` runs 2 s: enough).

## Verify by hand
1. In a forest battle, pick Attack on a goblin: Sarxian stays forward about 0.7 s before the goblin's HP tag and the
   number change and the hit sounds; then the hold and the step back.
2. A goblin's turn: its pause, then 0.7 s, then the party member's HP drops with the hit.
3. Defend passes at once, as before.

- **Done, with deviations:** a wind-up of no length (Defend, Move, Run) executes in `Submit` or the enemy's pause end at once, never entering `WindingUp`, so those land within the update as before. Re-pinned beyond the plan's list: `ActionPlaybackSoundTests`' two timing tests and `ActionPlaybackFallTests.Start_PoisonFellsSkippedHolder_TicksAndFadesAfterTheSkip` (their times now read `BattlePacing`'s holds, the 1 s impact hold having become 0.85 s); `Submit_HoldsThenStepsBack` is now `Submit_WindsUpHoldsThenStepsBack`. `BattleScene`, `BattleSceneTests` and `BattleSceneSoundTests` needed nothing.
  TN2's review: two test comments give the new timings; `Submit_Defend_ExecutesAtOnce` asserts Holding.
